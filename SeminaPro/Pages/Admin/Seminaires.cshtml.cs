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

        public List<Seminaire>? Seminaires { get; set; }

        public SeminairesModel(ApplicationDbContext context, ILogger<SeminairesModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void OnGet()
        {
            CheckAdmin();
            Seminaires = _context.Seminaires
                .OrderByDescending(s => s.DateSeminaire)
                .ToList();

            // Charger les inscriptions
            foreach (var seminaire in Seminaires)
            {
                seminaire.Inscriptions = _context.Inscriptions
                    .Where(i => i.SeminaireId == seminaire.Id)
                    .ToList();
            }
        }

        public IActionResult OnPostDelete(int id)
        {
            CheckAdmin();
            var seminaire = _context.Seminaires.FirstOrDefault(s => s.Id == id);
            if (seminaire == null)
            {
                return NotFound();
            }

            _context.Seminaires.Remove(seminaire);
            _context.SaveChanges();

            ViewData["Message"] = "Séminaire supprimé avec succès";
            return RedirectToPage();
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
