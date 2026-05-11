using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Models;

namespace SeminaPro.Pages.Seminaires
{
    public class SeminairesModel : PageModel
    {
        private readonly SeminaPro.Data.ApplicationDbContext _context;

        public SeminairesModel(SeminaPro.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // FILTERS
        // =========================

        [BindProperty(SupportsGet = true)]
        public string? SearchTitle { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SpecialiteId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        // ✅ NEW DATE FILTER
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        // =========================
        // DATA
        // =========================

        public List<Seminaire>? Seminaires { get; set; }

        // =========================
        // GET
        // =========================

        public void OnGet()
        {
            var query = _context.Seminaires
                .Include(s => s.Inscriptions)
                .AsQueryable();

            // =========================
            // TITLE FILTER
            // =========================
            if (!string.IsNullOrWhiteSpace(SearchTitle))
            {
                query = query.Where(s =>
                    s.Titre.Contains(SearchTitle));
            }

            // =========================
            // PRICE FILTER
            // =========================
            if (MinPrice.HasValue)
            {
                query = query.Where(s =>
                    s.Tarif >= MinPrice.Value);
            }

            if (MaxPrice.HasValue)
            {
                query = query.Where(s =>
                    s.Tarif <= MaxPrice.Value);
            }

            // =========================
            // SPECIALITE FILTER
            // =========================
            if (SpecialiteId.HasValue)
            {
                query = query.Where(s =>
                    s.SpecialiteId == SpecialiteId.Value);
            }

            // =========================
            // DATE FILTER (NEW)
            // =========================

            if (StartDate.HasValue)
            {
                query = query.Where(s =>
                    s.DateSeminaire >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(s =>
                    s.DateSeminaire <= EndDate.Value);
            }

            // =========================
            // SORT
            // =========================
            Seminaires = query
                .OrderBy(s => s.DateSeminaire)
                .ToList();
        }
    }
}