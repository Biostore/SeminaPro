using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Admin
{
    public class InscriptionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InscriptionsModel> _logger;

        public List<Inscription>? Inscriptions { get; set; }
        public List<Participant>? Participants { get; set; }
        public List<Seminaire>? Seminaires { get; set; }

        [BindProperty]
        public Inscription Inscription { get; set; } = new();

        public InscriptionsModel(ApplicationDbContext context, ILogger<InscriptionsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet(int? id)
        {
            CheckAdmin();
            LoadData();

            if (id.HasValue)
            {
                Inscription = _context.Inscriptions
                    .Include(i => i.Participant)
                    .Include(i => i.Seminaire)
                    .FirstOrDefault(i => i.Id == id.Value);
                if (Inscription == null)
                    return NotFound();
            }
            else
            {
                Inscription.DateInscription = DateTime.Now;
            }

            return Page();
        }

        public IActionResult OnPostSave()
        {
            CheckAdmin();
            LoadData();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Vérifier si l'inscription existe déjà
                if (Inscription.Id == 0)
                {
                    var existingInscription = _context.Inscriptions
                        .FirstOrDefault(i => i.ParticipantId == Inscription.ParticipantId 
                                          && i.SeminaireId == Inscription.SeminaireId);

                    if (existingInscription != null)
                    {
                        TempData["Error"] = "Ce participant est déjà inscrit à ce séminaire";
                        return Page();
                    }

                    Inscription.DateInscription = DateTime.Now;
                    _context.Inscriptions.Add(Inscription);
                    _logger.LogInformation($"Inscription créée - Participant: {Inscription.ParticipantId}, Séminaire: {Inscription.SeminaireId}");
                }
                else
                {
                    _context.Inscriptions.Update(Inscription);
                    _logger.LogInformation($"Inscription {Inscription.Id} modifiée");
                }

                _context.SaveChanges();
                TempData["Message"] = Inscription.Id == 0 ? "Inscription créée avec succès" : "Inscription modifiée avec succès";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde de l'inscription");
                TempData["Error"] = "Erreur lors de la sauvegarde";
                return Page();
            }
        }

        public IActionResult OnPostDelete(int id)
        {
            CheckAdmin();

            try
            {
                var inscription = _context.Inscriptions.FirstOrDefault(i => i.Id == id);
                if (inscription == null)
                    return NotFound();

                _context.Inscriptions.Remove(inscription);
                _context.SaveChanges();

                TempData["Message"] = "Inscription supprimée avec succès";
                _logger.LogInformation($"Inscription {id} supprimée");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'inscription");
                TempData["Error"] = "Erreur lors de la suppression";
            }

            return RedirectToPage();
        }

        private void LoadData()
        {
            Inscriptions = _context.Inscriptions
                .Include(i => i.Participant)
                .Include(i => i.Seminaire)
                .OrderByDescending(i => i.DateInscription)
                .ToList();

            Participants = _context.Participants
                .Include(p => p.Specialite)
                .OrderBy(p => p.Nom)
                .ToList();

            Seminaires = _context.Seminaires
                .OrderByDescending(s => s.DateSeminaire)
                .ToList();
        }

        private void CheckAdmin()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                throw new UnauthorizedAccessException("Accès réservé aux administrateurs");
            }
        }
    }
}
