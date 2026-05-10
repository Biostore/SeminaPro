using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SeminaPro.Models;
using SeminaPro.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SeminaPro.Pages.Admin
{
    public class CalendrierModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public List<Seminaire> Seminaires { get; set; } = new();

        public CalendrierModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            Seminaires = _context.Seminaires
                .Include(s => s.Inscriptions)
                .OrderBy(s => s.DateSeminaire)
                .ToList();
        }

        public IActionResult OnPostDelete(int id)
        {
            try
            {
                var seminaire = _context.Seminaires.Find(id);
                if (seminaire == null)
                {
                    return new JsonResult(new { success = false, message = "Séminaire non trouvé" }) { StatusCode = 404 };
                }

                // Supprimer les inscriptions associées
                var inscriptions = _context.Inscriptions.Where(i => i.SeminaireId == id).ToList();
                foreach (var inscription in inscriptions)
                {
                    _context.Inscriptions.Remove(inscription);
                }

                // Supprimer le séminaire
                _context.Seminaires.Remove(seminaire);
                _context.SaveChanges();

                return new JsonResult(new { success = true, message = "Séminaire supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
            }
        }
    }
}
