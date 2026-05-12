using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SeminaPro.Pages.Dashboard
{
    public class ProfilParticipantModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProfilParticipantModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Participant? CurrentParticipant { get; set; }
        public string? UserEmail { get; set; }
        public int TotalInscriptions { get; set; }
        public int InscriptionsVenires { get; set; }
        public int InscriptionsTerminees { get; set; }

        public IActionResult OnGet()
        {
            UserEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(UserEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            CurrentParticipant = _context.Participants
                .Include(p => p.Inscriptions)
                .ThenInclude(i => i.Seminaire)
                .FirstOrDefault(p => p.Email == UserEmail);

            if (CurrentParticipant == null)
            {
                return RedirectToPage("/Account/Login");
            }

            TotalInscriptions = CurrentParticipant.Inscriptions.Count;
            InscriptionsVenires = CurrentParticipant.Inscriptions
                .Count(i => i.Seminaire.DateSeminaire > DateTime.Now);
            InscriptionsTerminees = CurrentParticipant.Inscriptions
                .Count(i => i.Seminaire.DateSeminaire <= DateTime.Now);

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync(string firstName, string lastName, string phone)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var participant = await _context.Participants.FirstOrDefaultAsync(p => p.Email == userEmail);

            if (participant != null)
            {
                participant.Prenom = firstName;
                participant.Nom = lastName;
                participant.NumeroTelephone = phone;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profil mis à jour avec succès!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Les mots de passe ne correspondent pas");
                return RedirectToPage();
            }

            TempData["ErrorMessage"] = "Changement de mot de passe non disponible";
            return RedirectToPage();
        }
    }
}
