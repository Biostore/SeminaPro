using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;
using SeminaPro.Services.Interfaces;
using System.Text.RegularExpressions;

namespace SeminaPro.Pages.Seminaires
{
    public class PaiementModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<PaiementModel> _logger;

        public Seminaire? Seminaire { get; set; }
        public Participant? Participant { get; set; }
        public PaymentIntentDto? PaymentIntent { get; set; }
        public Inscription? CurrentInscription { get; set; }

        [BindProperty]
        public string? SelectedPaymentMethod { get; set; } = "card";

        [BindProperty]
        public string? CardName { get; set; }

        [BindProperty]
        public string? CardNumber { get; set; }

        [BindProperty]
        public string? CardExpiry { get; set; }

        [BindProperty]
        public string? CardCvc { get; set; }

        public PaiementModel(
            ApplicationDbContext context,
            IPaymentService paymentService,
            IInvoiceService invoiceService,
            ILogger<PaiementModel> logger)
        {
            _context = context;
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        public IActionResult OnGet(int id)
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToPage("/Account/Login");
                }

                // Récupérer séminaire
                Seminaire = _context.Seminaires
                    .FirstOrDefault(s => s.Id == id);

                if (Seminaire == null)
                {
                    return NotFound();
                }

                // Récupérer participant
                Participant = _context.Participants
                    .FirstOrDefault(p => p.Email == userEmail);

                if (Participant == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                // Vérifier si déjà inscrit et payé
                var dejaInscrit = _context.Inscriptions
                    .Any(i =>
                        i.ParticipantId == Participant.Id &&
                        i.SeminaireId == id &&
                        i.PaymentStatus == "Payée");

                if (dejaInscrit)
                {
                    TempData["Message"] = "Vous êtes déjà inscrit et payé à ce séminaire";
                    return RedirectToPage("/Dashboard/Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des données de paiement");
                TempData["Error"] = "Une erreur est survenue";
                return RedirectToPage("/Seminaires/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToPage("/Account/Login");
                }

                // Récupérer séminaire et participant
                Seminaire = _context.Seminaires.FirstOrDefault(s => s.Id == id);
                Participant = _context.Participants.FirstOrDefault(p => p.Email == userEmail);

                if (Seminaire == null || Participant == null)
                {
                    return NotFound();
                }

                // Vérifier les places disponibles
                var inscriptionCount = _context.Inscriptions
                    .Count(i => i.SeminaireId == id);

                if (inscriptionCount >= Seminaire.NombreMaximal)
                {
                    TempData["Error"] = "Le séminaire est complet";
                    return RedirectToPage("/Seminaires/Index");
                }

                // Valider les données du formulaire
                if (!ValidatePaymentData())
                {
                    return Page();
                }

                // Créer une inscription en attente de paiement
                var inscription = new Inscription
                {
                    ParticipantId = Participant.Id,
                    SeminaireId = Seminaire.Id,
                    DateInscription = DateTime.Now,
                    PaymentStatus = "En Attente",
                    MontantPaye = Seminaire.Tarif,
                    AffichageConfirmation = true
                };

                _context.Inscriptions.Add(inscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Inscription créée pour participant {ParticipantId} au séminaire {SeminaireId} - ID: {InscriptionId}",
                    Participant.Id,
                    Seminaire.Id,
                    inscription.Id);

                // Créer une intention de paiement
                PaymentIntent = await _paymentService.CreatePaymentIntentAsync(
                    inscription.Id,
                    Participant,
                    Seminaire);

                // Enregistrer l'intention de paiement
                inscription.PaymentMethodId = PaymentIntent.PaymentIntentId;
                _context.Inscriptions.Update(inscription);
                await _context.SaveChangesAsync();

                // Traiter le paiement
                bool paymentSucceeded = await ProcessPaymentAsync(inscription);

                if (paymentSucceeded)
                {
                    // Paiement réussi - générer la facture
                    await GenerateAndSaveInvoiceAsync(inscription);

                    _logger.LogInformation(
                        "Paiement réussi pour inscription {InscriptionId}",
                        inscription.Id);

                    TempData["Success"] = $"Paiement réussi! Facture: {inscription.FactureNumero}";
                    return RedirectToPage("/Seminaires/ConfirmationPaiement", new { id = inscription.Id });
                }
                else
                {
                    // Paiement échoué
                    inscription.PaymentStatus = "Échouée";
                    _context.Inscriptions.Update(inscription);
                    await _context.SaveChangesAsync();

                    _logger.LogWarning(
                        "Paiement échoué pour inscription {InscriptionId}",
                        inscription.Id);

                    TempData["Error"] = "Le paiement a échoué. Veuillez réessayer avec une autre méthode.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du paiement");
                TempData["Error"] = "Une erreur est survenue lors du paiement";
                return Page();
            }
        }

        /// <summary>
        /// Valide les données du formulaire de paiement
        /// </summary>
        private bool ValidatePaymentData()
        {
            if (string.IsNullOrEmpty(SelectedPaymentMethod))
            {
                ModelState.AddModelError("", "Veuillez sélectionner une méthode de paiement");
                return false;
            }

            if (SelectedPaymentMethod == "card")
            {
                return ValidateCardPayment();
            }
            else if (SelectedPaymentMethod == "paypal")
            {
                // Validation PayPal (simplifié)
                return true;
            }
            else if (SelectedPaymentMethod == "transfer")
            {
                // Validation virement bancaire
                return true;
            }

            ModelState.AddModelError("", "Méthode de paiement non valide");
            return false;
        }

        /// <summary>
        /// Valide les données de carte bancaire
        /// </summary>
        private bool ValidateCardPayment()
        {
            // Valider le nom sur la carte
            if (string.IsNullOrWhiteSpace(CardName) || CardName.Length < 3)
            {
                ModelState.AddModelError("CardName", "Nom sur la carte invalide");
                return false;
            }

            // Valider le numéro de carte
            if (string.IsNullOrWhiteSpace(CardNumber))
            {
                ModelState.AddModelError("CardNumber", "Numéro de carte requis");
                return false;
            }

            string cleanCardNumber = Regex.Replace(CardNumber, @"\s+", "");
            if (!Regex.IsMatch(cleanCardNumber, @"^\d{13,19}$"))
            {
                ModelState.AddModelError("CardNumber", "Numéro de carte invalide");
                return false;
            }

            // Valider Luhn
            if (!ValidateLuhn(cleanCardNumber))
            {
                ModelState.AddModelError("CardNumber", "Numéro de carte invalide (checksum)");
                return false;
            }

            // Valider la date d'expiration
            if (string.IsNullOrWhiteSpace(CardExpiry) || !CardExpiry.Contains("/"))
            {
                ModelState.AddModelError("CardExpiry", "Date d'expiration invalide");
                return false;
            }

            string[] expiryParts = CardExpiry.Split('/');
            if (expiryParts.Length != 2 || 
                !int.TryParse(expiryParts[0], out int month) || 
                !int.TryParse(expiryParts[1], out int year))
            {
                ModelState.AddModelError("CardExpiry", "Date d'expiration invalide");
                return false;
            }

            // Vérifier que la carte n'est pas expirée
            int fullYear = 2000 + year;
            DateTime expiryDate = new DateTime(fullYear, month, 1).AddMonths(1).AddDays(-1);
            if (expiryDate < DateTime.Now)
            {
                ModelState.AddModelError("CardExpiry", "Carte expirée");
                return false;
            }

            // Valider le CVV
            if (string.IsNullOrWhiteSpace(CardCvc) || !Regex.IsMatch(CardCvc, @"^\d{3,4}$"))
            {
                ModelState.AddModelError("CardCvc", "CVV invalide");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valide un numéro de carte avec l'algorithme de Luhn
        /// </summary>
        private bool ValidateLuhn(string cardNumber)
        {
            int sum = 0;
            bool isEven = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = cardNumber[i] - '0';

                if (isEven)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
                isEven = !isEven;
            }

            return (sum % 10) == 0;
        }

        /// <summary>
        /// Traite le paiement (simulation pour développement)
        /// </summary>
        private async Task<bool> ProcessPaymentAsync(Inscription inscription)
        {
            try
            {
                // En développement: simuler un paiement réussi
                // En production: intégrer Stripe, PayPal, etc.

                if (SelectedPaymentMethod == "card")
                {
                    // Simuler le traitement de la carte
                    await Task.Delay(500);

                    // Vérifier certains numéros de test qui échouent
                    if (CardNumber?.Contains("0000") == true)
                    {
                        return false;
                    }

                    return true;
                }
                else if (SelectedPaymentMethod == "paypal")
                {
                    // Simuler PayPal
                    await Task.Delay(500);
                    return true;
                }
                else if (SelectedPaymentMethod == "transfer")
                {
                    // Virement: marquer comme en attente de confirmation
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du paiement");
                return false;
            }
        }

        /// <summary>
        /// Génère et enregistre la facture
        /// </summary>
        private async Task GenerateAndSaveInvoiceAsync(Inscription inscription)
        {
            try
            {
                // Charger l'inscription complète avec les relations
                var completeInscription = await _context.Inscriptions
                    .Include(i => i.Participant)
                    .Include(i => i.Seminaire)
                    .FirstOrDefaultAsync(i => i.Id == inscription.Id);

                if (completeInscription == null)
                    return;

                // Générer le numéro de facture
                string invoiceNumber = _invoiceService.GenerateInvoiceNumber(inscription.Id);

                // Générer le contenu HTML
                string invoiceHtml = await _invoiceService.GenerateInvoiceHtmlAsync(completeInscription);

                // Convertir en PDF (ou garder en HTML pour test)
                byte[] invoiceContent = await _invoiceService.ConvertHtmlToPdfAsync(invoiceHtml);

                // Enregistrer la facture
                bool saved = await _invoiceService.SaveInvoiceAsync(
                    inscription.Id,
                    invoiceNumber,
                    invoiceContent);

                if (saved)
                {
                    // Mettre à jour le statut de paiement
                    inscription.PaymentStatus = "Payée";
                    inscription.DatePaiement = DateTime.Now;
                    inscription.FactureNumero = invoiceNumber;
                    inscription.DateFacture = DateTime.Now;

                    _context.Inscriptions.Update(inscription);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Facture générée: {FactureNumero} pour inscription {InscriptionId}",
                        invoiceNumber,
                        inscription.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération de la facture");
            }
        }
    }
}
