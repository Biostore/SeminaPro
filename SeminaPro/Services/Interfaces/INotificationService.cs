using SeminaPro.Models;

namespace SeminaPro.Services.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// Ajoute une nouvelle notification
        /// </summary>
        Task<Notification> AjouterNotificationAsync(
            int participantId,
            string titre,
            string message,
            string type = "General",
            string? lien = null);

        /// <summary>
        /// Récupère toutes les notifications non lues d'un participant
        /// </summary>
        Task<List<Notification>> ObtenirNotificationsNonLuesAsync(int participantId);

        /// <summary>
        /// Récupère les X dernières notifications d'un participant
        /// </summary>
        Task<List<Notification>> ObtenirDerniereNotificationsAsync(int participantId, int nombre = 10);

        /// <summary>
        /// Marque une notification comme lue
        /// </summary>
        Task<bool> MarquerCommeLueAsync(int notificationId);

        /// <summary>
        /// Marque toutes les notifications d'un participant comme lues
        /// </summary>
        Task<bool> MarquerToutesCommeLuesAsync(int participantId);

        /// <summary>
        /// Supprime une notification
        /// </summary>
        Task<bool> SupprimerNotificationAsync(int notificationId);

        /// <summary>
        /// Supprime toutes les notifications d'un participant
        /// </summary>
        Task<bool> SupprimerToutesNotificationsAsync(int participantId);

        /// <summary>
        /// Compte les notifications non lues d'un participant
        /// </summary>
        Task<int> CompterNotificationsNonLuesAsync(int participantId);
    }
}
