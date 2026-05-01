using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public string? UserEmail { get; set; }
        public List<Inscription>? MesInscriptions { get; set; }
        public int TotalInscriptions { get; set; }
        public int InscriptionsAVenir { get; set; }
        public int InscriptionsPasses { get; set; }

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
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
                // Récupérer les inscriptions avec les séminaires en eager loading
                var participant = _context.Participants.FirstOrDefault(p => p.Email == UserEmail);
                if (participant != null)
                {
                    MesInscriptions = _context.Inscriptions
                        .Where(i => i.ParticipantId == participant.Id)
                        .Include(i => i.Seminaire)
                        .OrderByDescending(i => i.DateInscription)
                        .ToList();

                    TotalInscriptions = MesInscriptions.Count;
                    InscriptionsAVenir = MesInscriptions.Count(i => i.Seminaire != null && i.Seminaire.DateSeminaire > DateTime.Now);
                    InscriptionsPasses = MesInscriptions.Count(i => i.Seminaire != null && i.Seminaire.DateSeminaire <= DateTime.Now);
                }
                else
                {
                    MesInscriptions = new List<Inscription>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des inscriptions");
                MesInscriptions = new List<Inscription>();
            }

            return Page();
        }

        public IActionResult OnPostCancelInscription(int id)
        {
            var inscription = _context.Inscriptions.FirstOrDefault(i => i.Id == id);
            if (inscription != null)
            {
                _context.Inscriptions.Remove(inscription);
                _context.SaveChanges();
                TempData["Message"] = "Inscription annulée";
            }

            return RedirectToPage();
        }
    }
}
