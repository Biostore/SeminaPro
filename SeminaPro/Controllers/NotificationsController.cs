using Microsoft.AspNetCore.Mvc;
using SeminaPro.Services.Interfaces;

namespace SeminaPro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Récupère les notifications non lues
        /// </summary>
        [HttpGet("non-lues/{participantId}")]
        public async Task<IActionResult> ObtenirNotificationsNonLues(int participantId)
        {
            try
            {
                var notifications = await _notificationService.ObtenirNotificationsNonLuesAsync(participantId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur: {ex.Message}");
                return StatusCode(500, "Erreur lors de la récupération des notifications");
            }
        }

        /// <summary>
        /// Récupère les dernières notifications
        /// </summary>
        [HttpGet("dernieres/{participantId}")]
        public async Task<IActionResult> ObtenirDerniereNotifications(int participantId, [FromQuery] int nombre = 10)
        {
            try
            {
                var notifications = await _notificationService.ObtenirDerniereNotificationsAsync(participantId, nombre);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur: {ex.Message}");
                return StatusCode(500, "Erreur lors de la récupération des notifications");
            }
        }

        /// <summary>
        /// Compte les notifications non lues
        /// </summary>
        [HttpGet("count-non-lues/{participantId}")]
        public async Task<IActionResult> CompterNotificationsNonLues(int participantId)
        {
            try
            {
                var count = await _notificationService.CompterNotificationsNonLuesAsync(participantId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur: {ex.Message}");
                return StatusCode(500, "Erreur lors du comptage des notifications");
            }
        }

        /// <summary>
        /// Marque une notification comme lue
        /// </summary>
        [HttpPut("{notificationId}/marquer-lue")]
        public async Task<IActionResult> MarquerCommeLue(int notificationId)
        {
            try
            {
                var result = await _notificationService.MarquerCommeLueAsync(notificationId);
                if (!result)
                    return NotFound("Notification non trouvée");

                return Ok(new { message = "Notification marquée comme lue" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur: {ex.Message}");
                return StatusCode(500, "Erreur lors de la mise à jour");
            }
        }

        /// <summary>
        /// Marque toutes les notifications comme lues
        /// </summary>
        [HttpPut("marquer-tous-lues/{participantId}")]
        public async Task<IActionResult> MarquerToutesCommeLues(int participantId)
        {
            try
            {
                var result = await _notificationService.MarquerToutesCommeLuesAsync(participantId);
                if (!result)
                    return BadRequest("Erreur lors de la mise à jour");

                return Ok(new { message = "Toutes les notifications marquées comme lues" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur: {ex.Message}");
                return StatusCode(500, "Erreur lors de la mise à jour");
            }
        }

        /// <summary>
        /// Supprime une notification
        /// </summary>
        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> SupprimerNotification(int notificationId)
        {
            try
            {
                var result = await _notificationService.SupprimerNotificationAsync(notificationId);
                if (!result)
                    return NotFound("Notification non trouvée");

                return Ok(new { message = "Notification supprimée" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur: {ex.Message}");
                return StatusCode(500, "Erreur lors de la suppression");
            }
        }

        /// <summary>
        /// Supprime toutes les notifications
        /// </summary>
        [HttpDelete("supprimer-tous/{participantId}")]
        public async Task<IActionResult> SupprimerToutesNotifications(int participantId)
        {
            try
            {
                var result = await _notificationService.SupprimerToutesNotificationsAsync(participantId);
                if (!result)
                    return BadRequest("Erreur lors de la suppression");

                return Ok(new { message = "Toutes les notifications supprimées" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur: {ex.Message}");
                return StatusCode(500, "Erreur lors de la suppression");
            }
        }
    }
}
