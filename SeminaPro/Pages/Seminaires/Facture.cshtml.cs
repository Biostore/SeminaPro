using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Seminaires
{
    public class FactureModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FactureModel> _logger;

        // =========================
        // PROPRIÉTÉS
        // =========================

        public Inscription? Inscription { get; set; }

        public Participant? Participant { get; set; }

        public Seminaire? Seminaire { get; set; }

        // =========================
        // CONSTRUCTEUR
        // =========================

        public FactureModel(
            ApplicationDbContext context,
            ILogger<FactureModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =========================
        // GET
        // =========================

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // Vérifier session utilisateur
                var userEmail = HttpContext.Session.GetString("UserEmail");

                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToPage("/Account/Login");
                }

                // Charger inscription + relations
                Inscription = await _context.Inscriptions
                    .Include(i => i.Participant)
                    .Include(i => i.Seminaire)
                        .ThenInclude(s => s.Specialite)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (Inscription == null)
                {
                    _logger.LogWarning("Inscription introuvable : {Id}", id);
                    return NotFound();
                }

                // Vérifier propriétaire
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.Email == userEmail);

                if (participant == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                if (Inscription.ParticipantId != participant.Id)
                {
                    _logger.LogWarning(
                        "Accès interdit à la facture {Id} par {Email}",
                        id,
                        userEmail);

                    return Forbid();
                }

                Participant = Inscription.Participant;
                Seminaire = Inscription.Seminaire;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur lors du chargement de la facture");

                return StatusCode(500);
            }
        }
    }
}