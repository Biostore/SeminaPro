using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Dashboard
{
    public class MonProfilModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MonProfilModel> _logger;
        private readonly IWebHostEnvironment _env;

        public MonProfilModel(
            ApplicationDbContext context,
            ILogger<MonProfilModel> logger,
            IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        [BindProperty]
        public Participant? CurrentParticipant { get; set; }

        [BindProperty]
        public IFormFile? ProfileImage { get; set; }

        public string? UserEmail { get; set; }

        public List<Seminaire> MesSeminaires { get; set; } = new();

        // =========================
        // GET PROFIL
        // =========================
        public IActionResult OnGet()
        {
            UserEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(UserEmail))
                return RedirectToPage("/Account/Login");

            try
            {
                CurrentParticipant = _context.Participants
                    .FirstOrDefault(p => p.Email == UserEmail);

                if (CurrentParticipant == null)
                    return RedirectToPage("/Account/Login");

                MesSeminaires = _context.Inscriptions
                    .Where(i => i.ParticipantId == CurrentParticipant.Id)
                    .Include(i => i.Seminaire)
                    .Select(i => i.Seminaire!)
                    .ToList();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur chargement profil");
                return Page();
            }
        }

        // =========================
        // UPDATE PROFIL
        // =========================
        public IActionResult OnPost()
        {
            UserEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(UserEmail))
                return RedirectToPage("/Account/Login");

            try
            {
                var participant = _context.Participants
                    .FirstOrDefault(p => p.Email == UserEmail);

                if (participant == null)
                    return RedirectToPage("/Account/Login");

                participant.Prenom = CurrentParticipant?.Prenom ?? participant.Prenom;
                participant.Nom = CurrentParticipant?.Nom ?? participant.Nom;
                participant.NumeroTelephone = CurrentParticipant?.NumeroTelephone ?? participant.NumeroTelephone;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Profil mis à jour avec succès";

                // reload
                CurrentParticipant = participant;

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur update profil");
                TempData["ErrorMessage"] = "Erreur lors de la mise à jour";
                return Page();
            }
        }

        // =========================
        // UPLOAD IMAGE
        // =========================
        public async Task<IActionResult> OnPostUploadImageAsync()
        {
            UserEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(UserEmail))
                return RedirectToPage("/Account/Login");

            if (ProfileImage == null || ProfileImage.Length == 0)
            {
                TempData["ErrorMessage"] = "Veuillez sélectionner une image";
                return RedirectToPage();
            }

            try
            {
                var participant = _context.Participants
                    .FirstOrDefault(p => p.Email == UserEmail);

                if (participant == null)
                    return RedirectToPage("/Account/Login");

                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/profiles");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                participant.ImageUrl = "/uploads/profiles/" + fileName;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Image mise à jour avec succès";

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur upload image");
                TempData["ErrorMessage"] = "Erreur lors de l'upload";
                return Page();
            }
        }
    }
}