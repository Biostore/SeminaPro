using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;
using SeminaPro.Services.Interfaces;

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

        [BindProperty]
        public string? SelectedPaymentMethod { get; set; } = "card";

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

                // Vérifier si déjà inscrit
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
                _logger.LogError(ex, "Erreur lors de la récupération des données");
                return StatusCode(500);
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
                _context.SaveChanges();

                // Créer une intention de paiement
                PaymentIntent = await _paymentService.CreatePaymentIntentAsync(
                    inscription.Id,
                    Participant,
                    Seminaire);

                // Enregistrer l'intention de paiement
                inscription.PaymentMethodId = PaymentIntent.PaymentIntentId;
                _context.Inscriptions.Update(inscription);
                _context.SaveChanges();

                // Retourner à la même page avec les infos de paiement
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du paiement");
                TempData["Error"] = "Une erreur est survenue";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmPaymentAsync(int id, string paymentIntentId)
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized();
                }

                // Récupérer l'inscription
                var inscription = _context.Inscriptions
                    .Include(i => i.Participant)
                    .Include(i => i.Seminaire)
                    .FirstOrDefault(i => i.PaymentMethodId == paymentIntentId);

                if (inscription == null)
                {
                    return NotFound();
                }

                // Vérifier le paiement
                bool paymentVerified = await _paymentService.VerifyPaymentAsync(paymentIntentId);

                if (paymentVerified)
                {
                    // Mettre à jour l'inscription
                    inscription.PaymentStatus = "Payée";
                    inscription.DatePaiement = DateTime.Now;
                    inscription.TransactionId = paymentIntentId;

                    // Générer la facture
                    string invoiceNumber = _invoiceService.GenerateInvoiceNumber(inscription.Id);
                    string invoiceHtml = await _invoiceService.GenerateInvoiceHtmlAsync(inscription);
                    byte[] invoicePdf = await _invoiceService.ConvertHtmlToPdfAsync(invoiceHtml);

                    // Enregistrer la facture
                    await _invoiceService.SaveInvoiceAsync(
                        inscription.Id,
                        invoiceNumber,
                        invoicePdf);

                    inscription.FactureNumero = invoiceNumber;
                    _context.Inscriptions.Update(inscription);
                    _context.SaveChanges();

                    TempData["Message"] = "Paiement réussi! Votre facture a été générée.";
                    return RedirectToPage("/Dashboard/Index");
                }
                else
                {
                    // Paiement échoué
                    inscription.PaymentStatus = "Échouée";
                    _context.Inscriptions.Update(inscription);
                    _context.SaveChanges();

                    TempData["Error"] = "Le paiement a échoué. Veuillez réessayer.";
                    return RedirectToPage("/Seminaires/Paiement", new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la confirmation du paiement");
                return StatusCode(500);
            }
        }
    }
}
