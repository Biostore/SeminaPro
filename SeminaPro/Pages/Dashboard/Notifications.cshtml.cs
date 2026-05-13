using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SeminaPro.Data;
using SeminaPro.Models;
using SeminaPro.Services.Interfaces;

namespace SeminaPro.Pages.Dashboard
{
    public class NotificationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsModel> _logger;

        public List<Notification> Notifications { get; set; } = new();
        public int ParticipantId { get; set; }

        public NotificationsModel(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<NotificationsModel> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Vérifier la connexion
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            // Récupérer le participant
            var participant = _context.Participants
                .FirstOrDefault(p => p.Email == userEmail);

            if (participant == null)
            {
                return RedirectToPage("/Account/Login");
            }

            ParticipantId = participant.Id;

            // Récupérer les notifications
            Notifications = await _notificationService.ObtenirDerniereNotificationsAsync(
                participant.Id,
                nombre: 50);

            return Page();
        }
    }
}
