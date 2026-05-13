using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;
using SeminaPro.Services.Interfaces;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace SeminaPro.Pages.Seminaires
{
    public class PaiementModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceService _invoiceService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaiementModel> _logger;
        private readonly IConfiguration _configuration;

        public Seminaire? Seminaire { get; set; }
        public Participant? Participant { get; set; }
        public PaymentIntentDto? PaymentIntent { get; set; }
        public Inscription? CurrentInscription { get; set; }
        public string? StripePublicKey { get; set; }
        public string? RequestToken { get; set; }

        [BindProperty]
        public string? SelectedPaymentMethod { get; set; } = "card";

        [BindProperty]
        public string? CardName { get; set; }

        [BindProperty]
        public string? StripeToken { get; set; }

        public PaiementModel(
            ApplicationDbContext context,
            IPaymentService paymentService,
            IInvoiceService invoiceService,
            INotificationService notificationService,
            ILogger<PaiementModel> logger,
            IConfiguration configuration)
        {
            _context = context;
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            _notificationService = notificationService;
            _logger = logger;
            _configuration = configuration;
            _logger = logger;
            _configuration = configuration;
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

                // Charger la clé publique Stripe
                StripePublicKey = _configuration["Stripe:PublicKey"];
                RequestToken = GetRequestVerificationToken();

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

                // Traiter le paiement selon la méthode
                bool paymentSucceeded = await ProcessPaymentAsync(inscription);

                if (paymentSucceeded)
                {
                    // Paiement réussi - générer la facture
                    await GenerateAndSaveInvoiceAsync(inscription);

                    // Créer les notifications
                    await _notificationService.AjouterNotificationAsync(
                        Participant.Id,
                        titre: "Inscription Confirmée",
                        message: $"Vous êtes maintenant inscrit au séminaire: {Seminaire.Titre}",
                        type: "Inscription",
                        lien: "/Dashboard/MesSeminaires"
                    );

                    await _notificationService.AjouterNotificationAsync(
                        Participant.Id,
                        titre: "Paiement Validé",
                        message: $"Votre paiement de {Seminaire.Tarif:C} a été confirmé pour {Seminaire.Titre}",
                        type: "Paiement",
                        lien: "/Dashboard/MesSeminaires"
                    );

                    await _notificationService.AjouterNotificationAsync(
                        Participant.Id,
                        titre: "Facture Disponible",
                        message: $"Votre facture pour {Seminaire.Titre} est prête",
                        type: "Facture",
                        lien: $"/Seminaires/TelechargerFacture?id={inscription.Id}"
                    );

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
                    StripePublicKey = _configuration["Stripe:PublicKey"];
                    RequestToken = GetRequestVerificationToken();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du paiement");
                TempData["Error"] = "Une erreur est survenue lors du paiement";
                StripePublicKey = _configuration["Stripe:PublicKey"];
                RequestToken = GetRequestVerificationToken();
                return Page();
            }
        }

        /// <summary>
        /// Récupère le token de vérification des requêtes CSRF
        /// </summary>
        private string GetRequestVerificationToken()
        {
            try
            {
                var tokens = HttpContext.Items["__RequestVerificationToken"];
                return tokens?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
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
                // Valider le nom sur la carte
                if (string.IsNullOrWhiteSpace(CardName) || CardName.Length < 3)
                {
                    ModelState.AddModelError("CardName", "Le nom sur la carte doit contenir au moins 3 caractères");
                    return false;
                }

                // Le token Stripe valide le reste
                if (string.IsNullOrWhiteSpace(StripeToken))
                {
                    ModelState.AddModelError("", "Erreur de paiement: token invalide. Veuillez réessayer.");
                    return false;
                }

                return true;
            }
            else if (SelectedPaymentMethod == "paypal")
            {
                // Validation PayPal - accepter par défaut
                _logger.LogInformation("Paiement PayPal sélectionné");
                return true;
            }
            else if (SelectedPaymentMethod == "transfer")
            {
                // Validation virement bancaire - accepter par défaut
                _logger.LogInformation("Virement bancaire sélectionné");
                return true;
            }

            ModelState.AddModelError("", "Méthode de paiement non valide");
            return false;
        }

        /// <summary>
        /// Traite le paiement selon la méthode sélectionnée
        /// </summary>
        private async Task<bool> ProcessPaymentAsync(Inscription inscription)
        {
            try
            {
                _logger.LogInformation(
                    "Traitement du paiement - Méthode: {Method}, Inscription ID: {Id}",
                    SelectedPaymentMethod,
                    inscription.Id);

                // Valider les données du formulaire
                if (!ValidatePaymentData())
                {
                    _logger.LogWarning("Validation du paiement échouée");
                    return false;
                }

                if (SelectedPaymentMethod == "card")
                {
                    // Traiter la carte bancaire via Stripe
                    return await ProcessStripePaymentAsync(inscription);
                }
                else if (SelectedPaymentMethod == "paypal")
                {
                    // Traiter PayPal
                    return await ProcessPayPalPaymentAsync(inscription);
                }
                else if (SelectedPaymentMethod == "transfer")
                {
                    // Traiter virement bancaire - marquer comme en attente
                    return await ProcessBankTransferPaymentAsync(inscription);
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
        /// Traite un paiement par carte (Stripe)
        /// </summary>
        private async Task<bool> ProcessStripePaymentAsync(Inscription inscription)
        {
            try
            {
                // Simuler le traitement Stripe
                if (string.IsNullOrWhiteSpace(StripeToken))
                {
                    _logger.LogWarning("Token Stripe manquant");
                    return false;
                }

                // En production: appel réel à l'API Stripe
                await Task.Delay(300);

                // Marquer comme payée
                inscription.PaymentStatus = "Payée";
                inscription.DatePaiement = DateTime.Now;
                inscription.TransactionId = $"stripe_{Guid.NewGuid():N}";
                inscription.SelectedPaymentMethod = "card";

                _context.Inscriptions.Update(inscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Paiement Stripe réussi - Transaction: {TransactionId}",
                    inscription.TransactionId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du paiement Stripe");
                return false;
            }
        }

        /// <summary>
        /// Traite un paiement via PayPal
        /// </summary>
        private async Task<bool> ProcessPayPalPaymentAsync(Inscription inscription)
        {
            try
            {
                // En production: rediriger vers PayPal
                await Task.Delay(300);

                inscription.PaymentStatus = "En Attente";
                inscription.TransactionId = $"paypal_{Guid.NewGuid():N}";
                inscription.SelectedPaymentMethod = "paypal";

                _context.Inscriptions.Update(inscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Paiement PayPal initié - Transaction: {TransactionId}",
                    inscription.TransactionId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du paiement PayPal");
                return false;
            }
        }

        /// <summary>
        /// Traite un paiement par virement bancaire
        /// </summary>
        private async Task<bool> ProcessBankTransferPaymentAsync(Inscription inscription)
        {
            try
            {
                // Virement: marquer comme en attente de confirmation
                await Task.Delay(300);

                inscription.PaymentStatus = "En Attente";
                inscription.TransactionId = $"transfer_{Guid.NewGuid():N}";
                inscription.SelectedPaymentMethod = "transfer";

                _context.Inscriptions.Update(inscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Virement bancaire enregistré - Transaction: {TransactionId}",
                    inscription.TransactionId);

                // En production: envoyer email avec détails du virement
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du virement bancaire");
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
