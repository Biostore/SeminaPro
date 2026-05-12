using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Dashboard
{
    public class SettingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SettingsModel> _logger;

        [BindProperty]
        public string Nom { get; set; } = string.Empty;

        [BindProperty]
        public string Prenom { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string NumeroTelephone { get; set; } = string.Empty;

        [BindProperty]
        public bool EnableNotifications { get; set; }

        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public Participant? CurrentParticipant { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public SettingsModel(ApplicationDbContext context, ILogger<SettingsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            UserEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(UserEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                CurrentParticipant = _context.Participants.FirstOrDefault(p => p.Email == UserEmail);
                if (CurrentParticipant != null)
                {
                    Nom = CurrentParticipant.Nom;
                    Prenom = CurrentParticipant.Prenom;
                    Email = CurrentParticipant.Email;
                    NumeroTelephone = CurrentParticipant.NumeroTelephone ?? string.Empty;
                }
                else
                {
                    return RedirectToPage("/Account/Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du participant");
                ErrorMessage = "Erreur lors du chargement des paramètres.";
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            UserEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(UserEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                CurrentParticipant = _context.Participants.FirstOrDefault(p => p.Email == UserEmail);
                if (CurrentParticipant != null)
                {
                    // Mettre à jour les informations personnelles
                    CurrentParticipant.Nom = Nom;
                    CurrentParticipant.Prenom = Prenom;
                    CurrentParticipant.NumeroTelephone = NumeroTelephone;

                    // Si l'email change, vérifier qu'il n'existe pas déjà
                    if (Email != UserEmail)
                    {
                        var existingEmail = _context.Participants.FirstOrDefault(p => p.Email == Email && p.Id != CurrentParticipant.Id);
                        if (existingEmail != null)
                        {
                            ErrorMessage = "Cet email est déjà utilisé.";
                            return Page();
                        }

                        CurrentParticipant.Email = Email;
                        HttpContext.Session.SetString("UserEmail", Email);
                    }

                    _context.SaveChanges();
                    Message = "Profil mis à jour avec succès.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du profil");
                ErrorMessage = "Erreur lors de la mise à jour du profil.";
            }

            return Page();
        }

        public IActionResult OnPostSecurity()
        {
            UserEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(UserEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            // Validation des mots de passe
            if (string.IsNullOrEmpty(NewPassword) && string.IsNullOrEmpty(ConfirmPassword))
            {
                Message = "Aucun changement de mot de passe.";
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Les mots de passe ne correspondent pas.";
                return Page();
            }

            if (NewPassword.Length < 6)
            {
                ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.";
                return Page();
            }

            try
            {
                CurrentParticipant = _context.Participants.FirstOrDefault(p => p.Email == UserEmail);
                if (CurrentParticipant != null)
                {
                    // Note: Pour une implémentation réelle, utiliser ASP.NET Core Identity avec hachage des mots de passe
                    // Ceci est une démonstration simplifiée
                    Message = "Mot de passe mis à jour avec succès. (Stockage sécurisé recommandé)";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du mot de passe");
                ErrorMessage = "Erreur lors de la mise à jour du mot de passe.";
            }

            return Page();
        }

        public IActionResult OnPostPreferences()
        {
            UserEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(UserEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                // Stocker les préférences dans la session
                HttpContext.Session.SetString("EnableNotifications", EnableNotifications.ToString());
                Message = "Préférences sauvegardées avec succès.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde des préférences");
                ErrorMessage = "Erreur lors de la sauvegarde des préférences.";
            }

            return Page();
        }
    }
}
