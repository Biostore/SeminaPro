using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SeminaPro.Models;
using Microsoft.EntityFrameworkCore;

namespace SeminaPro.Pages.Seminaires
{
    public class SeminairesModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? SearchTitle { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SpecialiteId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        public List<Seminaire>? Seminaires { get; set; }

        private readonly SeminaPro.Data.ApplicationDbContext _context;

        public SeminairesModel(SeminaPro.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            var query = _context.Seminaires
                .Include(s => s.Inscriptions)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTitle))
            {
                query = query.Where(s => s.Titre.Contains(SearchTitle));
            }
            if (MinPrice.HasValue)
            {
                query = query.Where(s => s.Tarif >= MinPrice.Value);
            }
            if (MaxPrice.HasValue)
            {
                query = query.Where(s => s.Tarif <= MaxPrice.Value);
            }
            if (SpecialiteId.HasValue)
            {
                query = query.Where(s => s.SpecialiteId == SpecialiteId.Value);
            }

            Seminaires = query.OrderBy(s => s.DateSeminaire).ToList();
        }
    }
}
