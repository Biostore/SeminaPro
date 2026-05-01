using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Seminaires
{
    public class InscriptionModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InscriptionModel> _logger;

        public Seminaire? Seminaire { get; set; }

        public InscriptionModel(ApplicationDbContext context, ILogger<InscriptionModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            Seminaire = _context.Seminaires.FirstOrDefault(s => s.Id == id);
            if (Seminaire == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            var participant = _context.Participants.FirstOrDefault(p => p.Email == userEmail);
            if (participant == null)
            {
                return NotFound();
            }

            var seminaire = _context.Seminaires.FirstOrDefault(s => s.Id == id);
            if (seminaire == null)
            {
                return NotFound();
            }

            // Vérifier si déjà inscrit
            var existingInscription = _context.Inscriptions
                .FirstOrDefault(i => i.ParticipantId == participant.Id && i.SeminaireId == seminaire.Id);
            if (existingInscription != null)
            {
                TempData["Message"] = "Vous êtes déjà inscrit à ce séminaire";
                return RedirectToPage("/Dashboard/Index");
            }

            // Vérifier les places disponibles
            var inscriptionCount = _context.Inscriptions.Count(i => i.SeminaireId == seminaire.Id);
            if (inscriptionCount >= seminaire.NombreMaximal)
            {
                TempData["Message"] = "Le séminaire est complet";
                return RedirectToPage("/Seminaires/Index");
            }

            try
            {
                var inscription = new Inscription
                {
                    ParticipantId = participant.Id,
                    SeminaireId = seminaire.Id,
                    DateInscription = DateTime.Now,
                    AffichageConfirmation = true
                };

                _context.Inscriptions.Add(inscription);
                _context.SaveChanges();

                TempData["Message"] = "Inscription réussie!";
                return RedirectToPage("/Dashboard/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription");
                TempData["Message"] = "Une erreur est survenue";
                return RedirectToPage("/Seminaires/Index");
            }
        }
    }
}
