using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SeminaPro.Models;
using SeminaPro.Data;
using System.IO;

namespace SeminaPro.Pages.Admin
{
    public class EditSeminaireModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditSeminaireModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Seminaire Seminaire { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Seminaire = await _context.Seminaires.FindAsync(id);

            if (Seminaire == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var seminaireInDb = await _context.Seminaires.FindAsync(Seminaire.Id);

            if (seminaireInDb == null)
                return NotFound();

            // =====================================================
            // VALIDATION: Date du séminaire ne doit pas être passée
            // =====================================================
            if (Seminaire.DateSeminaire < DateTime.Now)
            {
                ModelState.AddModelError("Seminaire.DateSeminaire",
                    "La date du séminaire ne peut pas être dans le passé.");
                return Page();
            }

            // =====================================================
            // VALIDATION: Pas deux séminaires avec mêmes coordonnées
            // =====================================================
            var existingWithSameCoordinates = _context.Seminaires
                .FirstOrDefault(s =>
                    s.Id != Seminaire.Id &&
                    s.Lieu == Seminaire.Lieu &&
                    s.DateSeminaire == Seminaire.DateSeminaire);

            if (existingWithSameCoordinates != null)
            {
                ModelState.AddModelError("",
                    "Un séminaire existe déjà avec ces mêmes coordonnées (lieu et date).");
                return Page();
            }

            // -------------------------
            // UPDATE BASIC FIELDS
            // -------------------------
            seminaireInDb.Code = Seminaire.Code;
            seminaireInDb.Titre = Seminaire.Titre;
            seminaireInDb.Lieu = Seminaire.Lieu;
            seminaireInDb.Tarif = Seminaire.Tarif;
            seminaireInDb.DateSeminaire = Seminaire.DateSeminaire;
            seminaireInDb.NombreMaximal = Seminaire.NombreMaximal;

            // -------------------------
            // IMAGE UPLOAD (IMPORTANT)
            // -------------------------
            var file = Request.Form.Files["imageFile"];

            if (file != null && file.Length > 0)
            {
                var folder = Path.Combine("wwwroot", "images", "seminaires");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // unique file name
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // ✅ SAVE PATH INTO SQLITE COLUMN
                seminaireInDb.ImageUrl = "/images/seminaires/" + fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("Seminaires");
        }
    }
}