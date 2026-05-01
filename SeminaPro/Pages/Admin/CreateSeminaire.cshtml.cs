using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Admin
{
    public class CreateSeminaireModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateSeminaireModel> _logger;

        [BindProperty]
        public Seminaire Seminaire { get; set; } = new();

        public List<Specialite>? Specialites { get; set; }

        public CreateSeminaireModel(ApplicationDbContext context, ILogger<CreateSeminaireModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void OnGet()
        {
            CheckAdmin();
            Specialites = _context.Specialites.ToList();    
        }

        public IActionResult OnPost()
        {
            var correlationId = HttpContext.TraceIdentifier;

            CheckAdmin();

            if (!ModelState.IsValid)
            {
                Specialites = _context.Specialites.ToList();
                return Page();
            }

            try
            {
                _context.Seminaires.Add(Seminaire);
                _context.SaveChanges();

                _logger.LogInformation(
                    "Séminaire ajouté avec succès. CorrelationId: {CorrelationId}, Code: {Code}", 
                    correlationId, 
                    Seminaire.Code);

                TempData["Message"] = "Séminaire ajouté avec succès";
                return RedirectToPage("Seminaires");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Erreur lors de l'ajout du séminaire. CorrelationId: {CorrelationId}", 
                    correlationId);
                ModelState.AddModelError("", "Une erreur est survenue");
                Specialites = _context.Specialites.ToList();
                return Page();
            }
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
