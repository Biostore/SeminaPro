using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= BASIC =================

        [BindProperty]
        public string FirstName { get; set; }

        [BindProperty]
        public string LastName { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Phone { get; set; }

        // ================= PASSWORD =================

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        // ================= SPECIALITE =================

        [BindProperty]
        public int SpecialiteId { get; set; }

        public List<Specialite> Specialites { get; set; } = new();

        // ================= CONDITIONS =================

        [BindProperty]
        public bool Accept { get; set; }

        // ================= ALERTS =================

        public string Message { get; set; }

        public string Error { get; set; }

        // ================= GET =================

        public void OnGet()
        {
            Specialites = _context.Specialites.ToList();
        }

        // ================= POST =================

        public async Task<IActionResult> OnPostAsync()
        {
            // PASSWORD CHECK

            if (Password != ConfirmPassword)
            {
                Error = "Les mots de passe ne correspondent pas.";
                return Page();
            }

            // CONDITIONS CHECK

            if (!Accept)
            {
                Error = "Veuillez accepter les conditions.";
                return Page();
            }

            // EMAIL EXIST

            var existingUser = _context.Participants
                .FirstOrDefault(p => p.Email == Email);

            if (existingUser != null)
            {
                Error = "Cet email existe déjà.";
                return Page();
            }

            // CREATE PARTICIPANT

            var participant = new Participant
            {
                Nom = LastName,
                Prenom = FirstName,
                Email = Email,
                NumeroTelephone = Phone,
                SpecialiteId = SpecialiteId
            };

            // SAVE DB

            _context.Participants.Add(participant);

            await _context.SaveChangesAsync();

            Message = "Inscription réussie avec succès !";

            return Page();
        }
    }
}