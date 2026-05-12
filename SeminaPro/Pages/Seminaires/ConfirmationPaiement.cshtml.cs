using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Seminaires
{
    public class ConfirmationPaiementModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConfirmationPaiementModel> _logger;

        public Inscription? Inscription { get; set; }
        public Participant? Participant { get; set; }
        public Seminaire? Seminaire { get; set; }

        public ConfirmationPaiementModel(
            ApplicationDbContext context,
            ILogger<ConfirmationPaiementModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToPage("/Account/Login");
                }

                // Charger l'inscription avec ses relations
                Inscription = await _context.Inscriptions
                    .Include(i => i.Participant)
                    .Include(i => i.Seminaire)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (Inscription == null)
                {
                    _logger.LogWarning("Inscription non trouvée: {Id}", id);
                    return NotFound();
                }

                // Vérifier que c'est l'utilisateur connecté
                if (Inscription.Participant?.Email != userEmail)
                {
                    _logger.LogWarning(
                        "Tentative d'accès non autorisé à l'inscription {Id} par {Email}",
                        id,
                        userEmail);
                    return Forbid();
                }

                // Vérifier que le paiement a réussi
                if (Inscription.PaymentStatus != "Payée")
                {
                    _logger.LogWarning(
                        "Tentative d'accès à la confirmation pour un paiement non complété: {Id}",
                        id);
                    return BadRequest();
                }

                Participant = Inscription.Participant;
                Seminaire = Inscription.Seminaire;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la confirmation de paiement");
                return StatusCode(500);
            }
        }
    }
}
