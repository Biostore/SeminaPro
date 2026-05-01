using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SeminaPro.Pages.Admin
{
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public string Prenom { get; set; }
        [BindProperty]
        public string Nom { get; set; }
        [BindProperty]
        public string Email { get; set; }

        public void OnGet()
        {
            // Load profile from session/user store - placeholder
            Prenom = "Admin";
            Nom = "Semina";
            Email = HttpContext.Session.GetString("UserEmail") ?? "admin@seminapro.com";
        }

        public IActionResult OnPost()
        {
            // Update profile - placeholder
            TempData["Message"] = "Profil mis à jour.";
            return RedirectToPage();
        }
    }
}
