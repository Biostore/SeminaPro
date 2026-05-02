using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardModel> _logger;

        public int TotalSeminaires { get; set; }
        public int TotalParticipants { get; set; }
        public int TotalInscriptions { get; set; }
        public int TotalSpecialites { get; set; }
        public List<Seminaire>? DerniersSeminaires { get; set; }
        public List<Participant>? DerniersParticipants { get; set; }

        public DashboardModel(ApplicationDbContext context, ILogger<DashboardModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void OnGet()
        {
            // Vérifier l'authentification
            var userRole = HttpContext.Session.GetString("UserRole");
            HttpContext.Items["UserRole"] = userRole;

            if (userRole != "Admin")
            {
                return;
            }

            // Compter les éléments
            TotalSeminaires = _context.Seminaires.Count();
            TotalParticipants = _context.Participants.Count();
            TotalInscriptions = _context.Inscriptions.Count();
            TotalSpecialites = _context.Specialites.Count();

            // Récupérer les derniers éléments
            DerniersSeminaires = _context.Seminaires
                .OrderByDescending(s => s.DateSeminaire)
                .Take(5)
                .ToList();

            // Charger les inscriptions
            foreach (var seminaire in DerniersSeminaires)
            {
                seminaire.Inscriptions = _context.Inscriptions
                    .Where(i => i.SeminaireId == seminaire.Id)
                    .ToList();
            }

            DerniersParticipants = _context.Participants
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToList();
        }
    }
}
