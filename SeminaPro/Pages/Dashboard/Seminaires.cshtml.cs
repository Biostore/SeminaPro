using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeminaPro.Pages.Dashboard
{
    public class SeminairesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SeminairesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Seminaire> Seminaires { get; set; } = new();
        public List<Inscription> Inscriptions { get; set; } = new();

        public IActionResult OnGet()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            // Get all seminars with their inscriptions
            Seminaires = _context.Seminaires
                .Include(s => s.Inscriptions)
                .OrderBy(s => s.DateSeminaire)
                .ToList();

            // Get current participant
            var participant = _context.Participants
                .FirstOrDefault(p => p.Email == userEmail);

            if (participant != null)
            {
                Inscriptions = _context.Inscriptions
                    .Where(i => i.ParticipantId == participant.Id)
                    .ToList();
            }

            return Page();
        }
    }
}
