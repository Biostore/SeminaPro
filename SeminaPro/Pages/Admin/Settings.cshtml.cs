using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SeminaPro.Pages.Admin
{
    public class SettingsModel : PageModel
    {
        [BindProperty]
        public string PlatformName { get; set; }
        [BindProperty]
        public string ContactEmail { get; set; }

        [BindProperty]
        public bool EnableNotifications { get; set; }
        [BindProperty]
        public bool UseDarkTheme { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }
        [BindProperty]
        public string ConfirmPassword { get; set; }

        public void OnGet()
        {
            // Load settings from config/store (placeholder)
            PlatformName = "SeminaPro";
            ContactEmail = "info@seminapro.com";
        }

        public IActionResult OnPost()
        {
            // Save general settings
            TempData["Message"] = "Paramètres enregistrés.";
            return RedirectToPage();
        }

        public IActionResult OnPostPreferences()
        {
            // Save preferences
            TempData["Message"] = "Préférences enregistrées.";
            return RedirectToPage();
        }

        public IActionResult OnPostSecurity()
        {
            // Update password - placeholder
            TempData["Message"] = "Sécurité mise à jour.";
            return RedirectToPage();
        }
    }
}
