using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;
using SeminaPro.Services.Interfaces;

namespace SeminaPro.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ApplicationDbContext context,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ajoute une nouvelle notification
        /// </summary>
        public async Task<Notification> AjouterNotificationAsync(
            int participantId,
            string titre,
            string message,
            string type = "General",
            string? lien = null)
        {
            try
            {
                var notification = new Notification
                {
                    ParticipantId = participantId,
                    Titre = titre,
                    Message = message,
                    Type = type,
                    Lien = lien,
                    DateCreation = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    $"Notification créée pour le participant {participantId}: {titre}");

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Erreur lors de la création de la notification: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère toutes les notifications non lues d'un participant
        /// </summary>
        public async Task<List<Notification>> ObtenirNotificationsNonLuesAsync(int participantId)
        {
            try
            {
                return await _context.Notifications
                    .Where(n => n.ParticipantId == participantId && !n.IsRead)
                    .OrderByDescending(n => n.DateCreation)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Erreur lors de la récupération des notifications non lues: {ex.Message}");
                return new List<Notification>();
            }
        }

        /// <summary>
        /// Récupère les X dernières notifications d'un participant
        /// </summary>
        public async Task<List<Notification>> ObtenirDerniereNotificationsAsync(int participantId, int nombre = 10)
        {
            try
            {
                return await _context.Notifications
                    .Where(n => n.ParticipantId == participantId)
                    .OrderByDescending(n => n.DateCreation)
                    .Take(nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Erreur lors de la récupération des notifications: {ex.Message}");
                return new List<Notification>();
            }
        }

        /// <summary>
        /// Marque une notification comme lue
        /// </summary>
        public async Task<bool> MarquerCommeLueAsync(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId);

                if (notification == null)
                {
                    _logger.LogWarning($"Notification {notificationId} non trouvée");
                    return false;
                }

                notification.IsRead = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Notification {notificationId} marquée comme lue");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Erreur lors de la mise à jour de la notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Marque toutes les notifications d'un participant comme lues
        /// </summary>
        public async Task<bool> MarquerToutesCommeLuesAsync(int participantId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.ParticipantId == participantId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    $"{notifications.Count} notifications marquées comme lues pour le participant {participantId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Erreur lors de la mise à jour des notifications: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Supprime une notification
        /// </summary>
        public async Task<bool> SupprimerNotificationAsync(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId);

                if (notification == null)
                {
                    _logger.LogWarning($"Notification {notificationId} non trouvée");
                    return false;
                }

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Notification {notificationId} supprimée");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Erreur lors de la suppression de la notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Supprime toutes les notifications d'un participant
        /// </summary>
        public async Task<bool> SupprimerToutesNotificationsAsync(int participantId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.ParticipantId == participantId)
                    .ToListAsync();

                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    $"{notifications.Count} notifications supprimées pour le participant {participantId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Erreur lors de la suppression des notifications: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Compte les notifications non lues d'un participant
        /// </summary>
        public async Task<int> CompterNotificationsNonLuesAsync(int participantId)
        {
            try
            {
                return await _context.Notifications
                    .CountAsync(n => n.ParticipantId == participantId && !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Erreur lors du comptage des notifications: {ex.Message}");
                return 0;
            }
        }
    }
}
