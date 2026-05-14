using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SeminaPro.Data;
using SeminaPro.Models;
using SeminaPro.Services.Interfaces;

namespace SeminaPro.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IPasswordService _passwordService;

        // Utilisateurs par défaut (à remplacer par une vraie authentification)
        private static readonly Dictionary<string, (string password, string role)> DefaultUsers = new()
        {
            { "admin@seminapro.com", ("admin123", "Admin") },
            { "user@seminapro.com", ("user123", "User") }
        };

        public LoginModel(ILogger<LoginModel> logger, ApplicationDbContext context, IPasswordService passwordService)
        {
            _logger = logger;
            _context = context;
            _passwordService = passwordService;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }

        public void OnGet()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                RedirectToPage("/Dashboard/Index");
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Chercher d'abord dans les utilisateurs par défaut
            if (DefaultUsers.TryGetValue(Email, out var defaultUser) && defaultUser.password == Password)
            {
                // Créer une session utilisateur
                HttpContext.Session.SetString("UserId", Email);
                HttpContext.Session.SetString("UserRole", defaultUser.role);
                HttpContext.Session.SetString("UserEmail", Email);

                if (defaultUser.role == "Admin")
                {
                    return RedirectToPage("/Admin/Dashboard");
                }
                else
                {
                    return RedirectToPage("/Dashboard/Index");
                }
            }

            // Chercher dans la base de données pour les utilisateurs inscrits
            try
            {
                var participant = _context.Participants.FirstOrDefault(p => p.Email == Email);
                if (participant != null && !string.IsNullOrEmpty(participant.PasswordHash))
                {
                    // Vérifier le mot de passe hachéavec BCrypt
                    if (_passwordService.VerifyPassword(Password, participant.PasswordHash))
                    {
                        // Authentifier l'utilisateur inscrit
                        HttpContext.Session.SetString("UserId", Email);
                        HttpContext.Session.SetString("UserRole", "User");
                        HttpContext.Session.SetString("UserEmail", Email);

                        return RedirectToPage("/Dashboard/Index");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du participant");
            }

            ViewData["Message"] = "Email ou mot de passe incorrect.";
            return Page();
        }
    }
}
