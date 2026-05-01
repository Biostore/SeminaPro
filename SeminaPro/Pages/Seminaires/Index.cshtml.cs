using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SeminaPro.Models;

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

        public void OnGet()
        {
            // Placeholder - à implémenter avec la DB
            Seminaires = new List<Seminaire>();
        }
    }
}
