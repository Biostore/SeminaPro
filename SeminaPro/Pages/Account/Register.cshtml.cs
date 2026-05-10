using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SeminaPro.Data;
using SeminaPro.Models;
using System.ComponentModel.DataAnnotations;

namespace SeminaPro.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        [Required(ErrorMessage = "Le prénom est obligatoire")]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Le nom est obligatoire")]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string? Phone { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Veuillez sélectionner un type de participant")]
        public string ParticipantType { get; set; } = string.Empty;

        [BindProperty]
        public string? Niveau { get; set; }

        [BindProperty]
        public string? NomUniversite { get; set; }

        [BindProperty]
        public string? Fonction { get; set; }

        [BindProperty]
        public string? NomEntreprise { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vous devez accepter les conditions d'utilisation")]
        public bool Accept { get; set; }

        public RegisterModel(ApplicationDbContext context, ILogger<RegisterModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnPost()
        {
            var correlationId = HttpContext.Items[SeminaPro.Middleware.CorrelationIdMiddleware.CorrelationIdHeader]?.ToString() ?? HttpContext.TraceIdentifier;

            // Nettoyage des espaces
            FirstName = FirstName?.Trim() ?? string.Empty;
            LastName = LastName?.Trim() ?? string.Empty;
            Email = Email?.Trim().ToLower() ?? string.Empty;
            Phone = Phone?.Trim();
            Password = Password?.Trim() ?? string.Empty;
            ConfirmPassword = ConfirmPassword?.Trim() ?? string.Empty;
            ParticipantType = ParticipantType?.Trim() ?? string.Empty;
            Niveau = Niveau?.Trim();
            Fonction = Fonction?.Trim();

            // Validation du type de participant
            if (string.IsNullOrEmpty(ParticipantType))
            {
                ModelState.AddModelError("ParticipantType", "Veuillez sélectionner un type de participant");
            }
            else if (ParticipantType != "Universitaire" && ParticipantType != "Industriel")
            {
                ModelState.AddModelError("ParticipantType", "Type de participant invalide");
            }
            else
            {
                // Validations conditionnelles
                if (ParticipantType == "Universitaire")
                {
                    if (string.IsNullOrWhiteSpace(Niveau))
                        ModelState.AddModelError("Niveau", "Le niveau d'études est obligatoire pour un universitaire");
                }
                else if (ParticipantType == "Industriel")
                {
                    if (string.IsNullOrWhiteSpace(Fonction))
                        ModelState.AddModelError("Fonction", "La fonction est obligatoire pour un industriel");
                }
            }

            // Validation du téléphone si fourni
            if (!string.IsNullOrEmpty(Phone))
            {
                var phoneValidator = new PhoneAttribute();
                if (!phoneValidator.IsValid(Phone))
                    ModelState.AddModelError("Phone", "Le numéro de téléphone n'est pas valide");
            }

            // Validation des mots de passe
            if (Password != ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Les mots de passe ne correspondent pas");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Validation échouée pour l'inscription. CorrelationId: {correlationId}");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning($"Erreur [{state.Key}]: {error.ErrorMessage}");
                    }
                }
                return Page();
            }

            if (!Accept)
            {
                ModelState.AddModelError("Accept", "Vous devez accepter les conditions d'utilisation");
                _logger.LogWarning($"Inscription refusée - conditions non acceptées. CorrelationId: {correlationId}");
                return Page();
            }

            // Vérifier si l'email existe déjà
            if (_context.Participants.Any(p => p.Email == Email))
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé");
                _logger.LogWarning($"Tentative d'inscription avec email existant: {Email}. CorrelationId: {correlationId}");
                return Page();
            }

            try
            {
                // Récupérer une spécialité par défaut
                var defaultSpecialite = _context.Specialites.FirstOrDefault();
                if (defaultSpecialite == null)
                {
                    _logger.LogError($"Aucune spécialité trouvée dans la base de données. CorrelationId: {correlationId}");
                    ModelState.AddModelError("", "Configuration système incorrecte. Veuillez contacter le support.");
                    return Page();
                }

                Participant participant;

                if (ParticipantType == "Universitaire")
                {
                    participant = new Universitaire
                    {
                        Nom = LastName,
                        Prenom = FirstName,
                        Email = Email,
                        NumeroTelephone = Phone ?? string.Empty,
                        Niveau = Niveau ?? string.Empty,
                        NomUniversite = NomUniversite ?? string.Empty,
                        SpecialiteId = defaultSpecialite.Id
                    };
                }
                else
                {
                    participant = new Industriel
                    {
                        Nom = LastName,
                        Prenom = FirstName,
                        Email = Email,
                        NumeroTelephone = Phone ?? string.Empty,
                        Fonction = Fonction ?? string.Empty,
                        NomEntreprise = NomEntreprise ?? string.Empty,
                        SpecialiteId = defaultSpecialite.Id
                    };
                }

                _context.Participants.Add(participant);
                _context.SaveChanges();

                _logger.LogInformation($"Nouvel utilisateur inscrit: {Email}. Type: {ParticipantType}. CorrelationId: {correlationId}");

                // Authentifier automatiquement
                HttpContext.Session.SetString("UserId", Email);
                HttpContext.Session.SetString("UserRole", "User");
                HttpContext.Session.SetString("UserEmail", Email);

                return RedirectToPage("/Dashboard/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'inscription de {Email}. CorrelationId: {correlationId}");
                ModelState.AddModelError("", "Une erreur est survenue lors de l'inscription. Veuillez réessayer.");
                return Page();
            }
        }
    }
}
