using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Admin
{
    public class SpecialitesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SpecialitesModel> _logger;

        public List<Specialite>? Specialites { get; set; }

        [BindProperty]
        public Specialite Specialite { get; set; } = new();

        public SpecialitesModel(ApplicationDbContext context, ILogger<SpecialitesModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet(int? id)
        {
            CheckAdmin();

            if (id.HasValue)
            {
                Specialite = _context.Specialites
                    .Include(s => s.Participants)
                    .Include(s => s.Seminaires)
                    .FirstOrDefault(s => s.Id == id.Value);
                if (Specialite == null)
                    return NotFound();
            }

            LoadSpecialites();
            return Page();
        }

        public IActionResult OnPostSave()
        {
            CheckAdmin();

            if (!ModelState.IsValid)
            {
                LoadSpecialites();
                return Page();
            }

            try
            {
                // Vérifier les doublons
                var existing = _context.Specialites
                    .FirstOrDefault(s => s.Libelle == Specialite.Libelle && s.Id != Specialite.Id);

                if (existing != null)
                {
                    TempData["Error"] = "Une spécialité avec ce libellé existe déjà";
                    LoadSpecialites();
                    return Page();
                }

                if (Specialite.Id == 0)
                {
                    _context.Specialites.Add(Specialite);
                    _logger.LogInformation($"Spécialité {Specialite.Libelle} créée");
                }
                else
                {
                    _context.Specialites.Update(Specialite);
                    _logger.LogInformation($"Spécialité {Specialite.Libelle} modifiée");
                }

                _context.SaveChanges();
                TempData["Message"] = Specialite.Id == 0 ? "Spécialité créée avec succès" : "Spécialité modifiée avec succès";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde de la spécialité");
                TempData["Error"] = "Erreur lors de la sauvegarde";
                LoadSpecialites();
                return Page();
            }
        }

        public IActionResult OnPostDelete(int id)
        {
            CheckAdmin();

            try
            {
                var specialite = _context.Specialites
                    .Include(s => s.Participants)
                    .Include(s => s.Seminaires)
                    .FirstOrDefault(s => s.Id == id);

                if (specialite == null)
                    return NotFound();

                // Vérifier si des participants ou séminaires utilisent cette spécialité
                if (specialite.Participants.Count > 0 || specialite.Seminaires.Count > 0)
                {
                    TempData["Error"] = "Impossible de supprimer une spécialité utilisée par des participants ou séminaires";
                    return RedirectToPage();
                }

                _context.Specialites.Remove(specialite);
                _context.SaveChanges();

                TempData["Message"] = "Spécialité supprimée avec succès";
                _logger.LogInformation($"Spécialité {specialite.Libelle} supprimée");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la spécialité");
                TempData["Error"] = "Erreur lors de la suppression";
            }

            return RedirectToPage();
        }

        private void LoadSpecialites()
        {
            Specialites = _context.Specialites
                .Include(s => s.Participants)
                .Include(s => s.Seminaires)
                .OrderBy(s => s.Libelle)
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
