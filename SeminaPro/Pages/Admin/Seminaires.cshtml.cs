using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Admin
{
    public class SeminairesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeminairesModel> _logger;

        // LISTE
        public List<Seminaire> Seminaires { get; set; } = new();

        public SeminairesModel(
            ApplicationDbContext context,
            ILogger<SeminairesModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================
        // GET
        // =====================================

        public IActionResult OnGet()
        {
            try
            {
                CheckAdmin();

                LoadSeminaires();

                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur chargement séminaires");

                TempData["Error"] =
                    ex.InnerException?.Message ?? ex.Message;

                Seminaires = new List<Seminaire>();

                return Page();
            }
        }

        // =====================================
        // DELETE
        // =====================================

        public IActionResult OnPostDelete(int id)
        {
            try
            {
                CheckAdmin();

                var seminaire = _context.Seminaires
                    .Include(s => s.Inscriptions)
                    .FirstOrDefault(s => s.Id == id);

                if (seminaire == null)
                {
                    TempData["Error"] =
                        "Séminaire introuvable";

                    return RedirectToPage();
                }

                // SUPPRIMER INSCRIPTIONS
                if (seminaire.Inscriptions != null
                    && seminaire.Inscriptions.Any())
                {
                    _context.Inscriptions
                        .RemoveRange(seminaire.Inscriptions);
                }

                // DELETE IMAGE PHYSIQUE
                if (!string.IsNullOrEmpty(
                    seminaire.ImageUrl))
                {
                    var imagePath =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            seminaire.ImageUrl
                                .TrimStart('/')
                                .Replace("/", "\\"));

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // DELETE SEMINAIRE
                _context.Seminaires.Remove(seminaire);

                _context.SaveChanges();

                TempData["Message"] =
                    "Séminaire supprimé avec succès";

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur suppression séminaire");

                TempData["Error"] =
                    ex.InnerException?.Message ?? ex.Message;

                return RedirectToPage();
            }
        }

        // =====================================
        // LOAD
        // =====================================

        private void LoadSeminaires()
        {
            Seminaires = _context.Seminaires
                .Include(s => s.Inscriptions)
                .OrderByDescending(s => s.DateSeminaire)
                .ToList();
        }

        // =====================================
        // CHECK ADMIN
        // =====================================

        private void CheckAdmin()
        {
            var userRole =
                HttpContext.Session.GetString("UserRole");

            if (userRole != "Admin")
            {
                throw new UnauthorizedAccessException(
                    "Accès réservé aux administrateurs");
            }
        }
    }
}