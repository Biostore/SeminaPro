using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;
using SeminaPro.Services.Interfaces;

namespace SeminaPro.Services.Implementations
{
    /// <summary>
    /// Service pour gérer les rappels automatiques et les tâches planifiées
    /// </summary>
    public class ReminderService : IReminderService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<ReminderService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Envoie des rappels pour les séminaires commençant demain
        /// </summary>
        public async Task SendSeminarRemindersAsync()
        {
            try
            {
                var tomorrow = DateTime.Now.Date.AddDays(1);
                var tomorrowEnd = tomorrow.AddDays(1);

                // Trouver les séminaires commençant demain
                var upcomingSeminaires = await _context.Seminaires
                    .Where(s => s.DateSeminaire >= tomorrow && s.DateSeminaire < tomorrowEnd)
                    .Include(s => s.Inscriptions)
                        .ThenInclude(i => i.Participant)
                    .ToListAsync();

                _logger.LogInformation($"Trouvé {upcomingSeminaires.Count} séminaires à rappeler pour demain");

                foreach (var seminaire in upcomingSeminaires)
                {
                    // Trouver tous les participants inscrits et payés
                    var inscriptions = seminaire.Inscriptions
                        .Where(i => i.PaymentStatus == "Payée")
                        .ToList();

                    foreach (var inscription in inscriptions)
                    {
                        if (inscription.Participant != null)
                        {
                            await _notificationService.AjouterNotificationAsync(
                                inscription.ParticipantId,
                                "Rappel : Séminaire demain",
                                $"N'oubliez pas ! Le séminaire \"{seminaire.Titre}\" commence demain à {seminaire.DateSeminaire:HH:mm} à {seminaire.Lieu}.",
                                "warning",
                                $"/Seminaires/Details/{seminaire.Id}");

                            _logger.LogInformation(
                                $"Rappel envoyé pour le séminaire {seminaire.Id} au participant {inscription.ParticipantId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi des rappels de séminaire");
            }
        }

        public async Task SendPaymentConfirmationAsync(int inscriptionId)
        {
            try
            {
                var inscription = await _context.Inscriptions
                    .Include(i => i.Participant)
                    .Include(i => i.Seminaire)
                    .FirstOrDefaultAsync(i => i.Id == inscriptionId);

                if (inscription?.Participant == null || inscription.Seminaire == null)
                    return;

                await _notificationService.AjouterNotificationAsync(
                    inscription.ParticipantId,
                    "Paiement confirmé ✓",
                    $"Votre paiement pour le séminaire \"{inscription.Seminaire.Titre}\" a été confirmé. Facture: {inscription.FactureNumero}",
                    "success",
                    $"/Seminaires/TelechargerFacture/{inscription.Id}");

                _logger.LogInformation(
                    $"Notification de paiement envoyée pour l'inscription {inscriptionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la confirmation de paiement");
            }
        }

        public async Task SendSeminarCancellationAsync(int seminaireId)
        {
            try
            {
                var seminar = await _context.Seminaires
                    .Include(s => s.Inscriptions)
                        .ThenInclude(i => i.Participant)
                    .FirstOrDefaultAsync(s => s.Id == seminaireId);

                if (seminar == null)
                    return;

                foreach (var inscription in seminar.Inscriptions.Where(i => i.PaymentStatus == "Payée"))
                {
                    if (inscription.Participant != null)
                    {
                        await _notificationService.AjouterNotificationAsync(
                            inscription.ParticipantId,
                            "Séminaire annulé",
                            $"Le séminaire \"{seminar.Titre}\" prévu le {seminar.DateSeminaire:dd/MM/yyyy} a été annulé. Un remboursement sera traité.",
                            "error",
                            "/Dashboard/Index");

                        _logger.LogInformation(
                            $"Notification d'annulation envoyée au participant {inscription.ParticipantId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification d'annulation");
            }
        }

        public async Task SendSeminarUpdateAsync(int seminaireId, string updateMessage)
        {
            try
            {
                var seminar = await _context.Seminaires
                    .Include(s => s.Inscriptions)
                        .ThenInclude(i => i.Participant)
                    .FirstOrDefaultAsync(s => s.Id == seminaireId);

                if (seminar == null)
                    return;

                foreach (var inscription in seminar.Inscriptions.Where(i => i.PaymentStatus == "Payée"))
                {
                    if (inscription.Participant != null)
                    {
                        await _notificationService.AjouterNotificationAsync(
                            inscription.ParticipantId,
                            "Mise à jour du séminaire",
                            $"Le séminaire \"{seminar.Titre}\" a été modifié. Détails: {updateMessage}",
                            "info",
                            $"/Seminaires/Details/{seminar.Id}");

                        _logger.LogInformation(
                            $"Notification de mise à jour envoyée au participant {inscription.ParticipantId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification de mise à jour");
            }
        }

        public async Task SendWaitlistNotificationAsync(int seminaireId)
        {
            try
            {
                var seminar = await _context.Seminaires
                    .FirstOrDefaultAsync(s => s.Id == seminaireId);

                if (seminar == null)
                    return;

                _logger.LogInformation("Notification de waitlist à implémenter");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification de waitlist");
            }
        }

        public async Task SendWelcomeNotificationAsync(int participantId)
        {
            try
            {
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.Id == participantId);

                if (participant == null)
                    return;

                await _notificationService.AjouterNotificationAsync(
                    participantId,
                    "Bienvenue sur SeminaPro !",
                    $"Bonjour {participant.Prenom}, bienvenue sur notre plateforme. Explorez nos séminaires et inscrivez-vous dès maintenant!",
                    "success",
                    "/Seminaires/Index");

                _logger.LogInformation($"Notification de bienvenue envoyée au participant {participantId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification de bienvenue");
            }
        }

        public async Task CleanupOldNotificationsAsync()
        {
            try
            {
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);

                var oldNotifications = await _context.Notifications
                    .Where(n => n.DateCreation < thirtyDaysAgo && n.IsRead)
                    .ToListAsync();

                if (oldNotifications.Any())
                {
                    _context.Notifications.RemoveRange(oldNotifications);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        $"{oldNotifications.Count} anciennes notifications supprimées");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du nettoyage des notifications");
            }
        }
    }
}
