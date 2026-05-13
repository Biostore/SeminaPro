using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Seminaires
{
    public class InscriptionModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InscriptionModel> _logger;

        // =====================================================
        // PROPRIÉTÉS
        // =====================================================

        public bool DejaInscrit { get; set; }

        public Seminaire? Seminaire { get; set; }

        // =====================================================
        // CONSTRUCTEUR
        // =====================================================

        public InscriptionModel(
            ApplicationDbContext context,
            ILogger<InscriptionModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================
        // GET
        // =====================================================

        public IActionResult OnGet(int? id, int? seminaireId)
        {
            // Accepter soit le paramètre de route (id) soit le query string (seminaireId)
            int actualId = id ?? seminaireId ?? 0;

            if (actualId == 0)
            {
                return BadRequest("ID du séminaire requis");
            }

            // Vérifier connexion
            var userEmail =
                HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            // Récupérer séminaire
            Seminaire = _context.Seminaires
                .FirstOrDefault(s => s.Id == actualId);

            if (Seminaire == null)
            {
                return NotFound();
            }

            // Récupérer participant
            var participant = _context.Participants
                .FirstOrDefault(p => p.Email == userEmail);

            if (participant == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Vérifier si déjà inscrit
            DejaInscrit = _context.Inscriptions
                .Any(i =>
                    i.ParticipantId == participant.Id &&
                    i.SeminaireId == id);

            return Page();
        }

        // =====================================================
        // POST
        // =====================================================

        public IActionResult OnPost(int id)
        {
            // Vérifier connexion
            var userEmail =
                HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            // Récupérer participant
            var participant = _context.Participants
                .FirstOrDefault(p => p.Email == userEmail);

            if (participant == null)
            {
                TempData["Message"] =
                    "Participant introuvable";

                return RedirectToPage("/Account/Login");
            }

            // Récupérer séminaire
            Seminaire = _context.Seminaires
                .FirstOrDefault(s => s.Id == id);

            if (Seminaire == null)
            {
                return NotFound();
            }

            // =================================================
            // DÉJÀ INSCRIT
            // =================================================

            var existingInscription =
                _context.Inscriptions
                .FirstOrDefault(i =>
                    i.ParticipantId == participant.Id &&
                    i.SeminaireId == Seminaire.Id);

            if (existingInscription != null)
            {
                DejaInscrit = true;

                TempData["Message"] =
                    "Vous êtes déjà inscrit à ce séminaire";

                return Page();
            }

            // =================================================
            // VÉRIFIER PLACES
            // =================================================

            var inscriptionCount =
                _context.Inscriptions
                .Count(i => i.SeminaireId == Seminaire.Id);

            if (inscriptionCount >= Seminaire.NombreMaximal)
            {
                TempData["Message"] =
                    "Le séminaire est complet";

                return RedirectToPage("/Seminaires/Index");
            }

            // =================================================
            // REDIRECTION VERS PAIEMENT
            // =================================================

            // Rediriger vers la page de paiement au lieu de créer l'inscription directement
            return RedirectToPage("/Seminaires/Paiement", new { id });
        }
    }
}
