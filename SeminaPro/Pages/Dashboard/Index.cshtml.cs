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
        public Participant? CurrentParticipant { get; set; }

        public List<Inscription> MesInscriptions { get; set; } = new();
        public List<Seminaire> MesSeminaires { get; set; } = new();

        public int TotalInscriptions { get; set; }
        public int InscriptionsAVenir { get; set; }
        public int InscriptionsPasses { get; set; }
        public int TotalSeminairesDisponibles { get; set; }

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            UserEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(UserEmail))
                return RedirectToPage("/Account/Login");

            try
            {
                // =====================================================
                // 1. PARTICIPANT
                // =====================================================
                CurrentParticipant = _context.Participants
                    .FirstOrDefault(p => p.Email == UserEmail);

                if (CurrentParticipant == null)
                {
                    _logger.LogWarning("Participant introuvable pour email: {Email}", UserEmail);
                    return Page();
                }

                // =====================================================
                // 2. INSCRIPTIONS
                // =====================================================
                MesInscriptions = _context.Inscriptions
                    .Include(i => i.Seminaire)
                    .Where(i => i.ParticipantId == CurrentParticipant.Id)
                    .Where(i => i.Seminaire != null)
                    .OrderByDescending(i => i.DateInscription)
                    .ToList();

                // =====================================================
                // 3. STATS
                // =====================================================
                TotalInscriptions = MesInscriptions.Count;

                InscriptionsAVenir = MesInscriptions
                    .Count(i => i.Seminaire!.DateSeminaire > DateTime.Now);

                InscriptionsPasses = MesInscriptions
                    .Count(i => i.Seminaire!.DateSeminaire <= DateTime.Now);

                // =====================================================
                // 4. MES SEMINAIRES (DISTINCT + SAFE)
                // =====================================================
                MesSeminaires = MesInscriptions
                    .Where(i => i.Seminaire != null)
                    .Select(i => i.Seminaire!)
                    .Distinct()
                    .OrderByDescending(s => s.DateSeminaire)
                    .Take(5)
                    .ToList();

                // =====================================================
                // 5. TOTAL SEMINAIRES DISPONIBLES
                // =====================================================
                TotalSeminairesDisponibles = _context.Seminaires.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur Dashboard Load");

                MesInscriptions = new List<Inscription>();
                MesSeminaires = new List<Seminaire>();
            }

            return Page();
        }

        // =========================================================
        // CANCEL INSCRIPTION
        // =========================================================
        public IActionResult OnPostCancelInscription(int id)
        {
            var inscription = _context.Inscriptions
                .FirstOrDefault(i => i.Id == id);

            if (inscription != null)
            {
                _context.Inscriptions.Remove(inscription);
                _context.SaveChanges();

                TempData["Message"] = "Inscription annulée avec succès";
            }

            return RedirectToPage();
        }
    }
}