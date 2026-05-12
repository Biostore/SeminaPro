using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Dashboard
{
    public class MesSeminairesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MesSeminairesModel> _logger;

        public string? UserEmail { get; set; }
        public Participant? CurrentParticipant { get; set; }

        public List<Inscription> MesInscriptions { get; set; } = new();
        public List<Seminaire> SeminairesAVenir { get; set; } = new();
        public List<Seminaire> SeminairesPasses { get; set; } = new();

        public int TotalInscrits { get; set; }
        public decimal DepenseTotal { get; set; }

        public MesSeminairesModel(ApplicationDbContext context, ILogger<MesSeminairesModel> logger)
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
                CurrentParticipant = _context.Participants
                    .FirstOrDefault(p => p.Email == UserEmail);

                if (CurrentParticipant == null)
                {
                    _logger.LogWarning("Participant introuvable pour email: {Email}", UserEmail);
                    return RedirectToPage("/Account/Login");
                }

                // Charger les inscriptions
                MesInscriptions = _context.Inscriptions
                    .Where(i => i.ParticipantId == CurrentParticipant.Id)
                    .Include(i => i.Seminaire)
                    .ToList();

                // Séparer les séminaires à venir et passés
                var maintenant = DateTime.Now;
                SeminairesAVenir = MesInscriptions
                    .Where(i => i.Seminaire != null && i.Seminaire.DateSeminaire > maintenant)
                    .Select(i => i.Seminaire!)
                    .OrderBy(s => s.DateSeminaire)
                    .ToList();

                SeminairesPasses = MesInscriptions
                    .Where(i => i.Seminaire != null && i.Seminaire.DateSeminaire <= maintenant)
                    .Select(i => i.Seminaire!)
                    .OrderByDescending(s => s.DateSeminaire)
                    .ToList();

                // Calculer les statistiques
                TotalInscrits = MesInscriptions.Count;
                DepenseTotal = MesInscriptions
                    .Where(i => i.Seminaire != null)
                    .Sum(i => i.Seminaire!.Tarif);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des séminaires");
                return Page();
            }
        }

        public IActionResult OnPostDesinscrire(int inscriptionId)
        {
            UserEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(UserEmail))
                return RedirectToPage("/Account/Login");

            try
            {
                var inscription = _context.Inscriptions
                    .FirstOrDefault(i => i.Id == inscriptionId);

                if (inscription == null)
                {
                    TempData["ErrorMessage"] = "Inscription introuvable";
                    return RedirectToPage();
                }

                _context.Inscriptions.Remove(inscription);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Vous avez été désinscrit du séminaire avec succès";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la désinscription");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la désinscription";
                return RedirectToPage();
            }
        }
    }
}
