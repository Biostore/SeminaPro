using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Services.Interfaces;

namespace SeminaPro.Pages.Seminaires
{
    public class TelechargerFactureModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<TelechargerFactureModel> _logger;

        public TelechargerFactureModel(
            ApplicationDbContext context,
            IInvoiceService invoiceService,
            ILogger<TelechargerFactureModel> logger)
        {
            _context = context;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized();
                }

                // Récupérer l'inscription
                var inscription = await _context.Inscriptions
                    .Include(i => i.Participant)
                    .Include(i => i.Seminaire)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (inscription == null)
                {
                    return NotFound();
                }

                // Vérifier que c'est le propriétaire
                if (inscription.Participant?.Email != userEmail)
                {
                    return Forbid();
                }

                // Vérifier que le paiement est effectué
                if (inscription.PaymentStatus != "Payée")
                {
                    return BadRequest("Facture non disponible - paiement non effectué");
                }

                // Générer la facture
                string invoiceHtml = await _invoiceService.GenerateInvoiceHtmlAsync(inscription);
                byte[] invoiceContent = System.Text.Encoding.UTF8.GetBytes(invoiceHtml);

                // Déterminer le type de contenu
                string contentType = "text/html";
                string fileExtension = "html";

                // En production, convertir en PDF
                // var invoicePdf = await _invoiceService.ConvertHtmlToPdfAsync(invoiceHtml);
                // contentType = "application/pdf";
                // fileExtension = "pdf";

                string fileName = $"{inscription.FactureNumero ?? $"facture-{inscription.Id}"}.{fileExtension}";

                _logger.LogInformation("Facture téléchargée: {FileName} par {Email}", 
                    fileName, userEmail);

                return File(invoiceContent, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du téléchargement de la facture");
                return StatusCode(500);
            }
        }
    }
}
