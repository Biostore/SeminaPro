using SeminaPro.Models;

namespace SeminaPro.Pages.Shared
{
    public class NotificationDropdownModel
    {
        public List<Notification> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
    }
}
