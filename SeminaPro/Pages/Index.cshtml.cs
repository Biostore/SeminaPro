using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        public List<Seminaire>? SeminairesRecents { get; set; }
        public List<Seminaire>? SeminairesMostPopular { get; set; }

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            try
            {
                // Charger les 6 séminaires les plus récents avec les inscriptions incluses
                SeminairesRecents = _context.Seminaires
                    .Include(s => s.Inscriptions)
                    .Where(s => s.DateSeminaire >= DateTime.Now)
                    .OrderBy(s => s.DateSeminaire)
                    .Take(6)
                    .ToList();

                // Charger les 6 séminaires les plus populaires avec les inscriptions incluses
                var allSeminairesWithInscriptions = _context.Seminaires
                    .Include(s => s.Inscriptions)
                    .Where(s => s.DateSeminaire >= DateTime.Now)
                    .ToList();

                SeminairesMostPopular = allSeminairesWithInscriptions
                    .OrderByDescending(s => s.Inscriptions.Count)
                    .Take(6)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des séminaires");
                SeminairesRecents = new List<Seminaire>();
                SeminairesMostPopular = new List<Seminaire>();
            }
        }
    }
}
