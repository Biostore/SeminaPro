<!-- 
    SNIPPET D'INTÉGRATION DU DROPDOWN DE NOTIFICATIONS

    À ajouter dans _Layout.cshtml, juste avant le menu utilisateur ou après les boutons de session

    Exemple d'intégration dans la navbar:
-->

<!-- AJOUTER CES LIGNES DANS LA NAVBAR -->
@{
    // Récupérer l'utilisateur actuel et ses notifications
    var userEmail = Context.Session.GetString("UserEmail");
    var participant = userEmail != null ? 
        DbContext.Participants.FirstOrDefault(p => p.Email == userEmail) : 
        null;
}

@if (participant != null)
{
    <!-- Charger les notifications -->
    @{
        var notificationService = HttpContext.RequestServices.GetService(typeof(SeminaPro.Services.Interfaces.INotificationService)) 
            as SeminaPro.Services.Interfaces.INotificationService;
        var recentNotifications = await notificationService.ObtenirDerniereNotificationsAsync(participant.Id, 5);
        var unreadCount = await notificationService.CompterNotificationsNonLuesAsync(participant.Id);
    }

    <!-- Partial view du dropdown -->
    @await Html.PartialAsync("NotificationDropdown", 
        new SeminaPro.Pages.Shared.NotificationDropdownModel 
        { 
            Notifications = recentNotifications,
            UnreadCount = unreadCount
        }
    )
}

<!-- AJOUTS CSS NÉCESSAIRES -->
<link rel="stylesheet" href="~/css/notifications.css" asp-append-version="true" />

<!-- ALTERNATIVE : Si vous ne pouvez pas modifier _Layout.cshtml -->
<!-- Créez un composant view component -->
@component NotificationBellComponent(participant?.Id ?? 0)
