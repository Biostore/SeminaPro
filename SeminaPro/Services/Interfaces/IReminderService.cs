namespace SeminaPro.Services.Interfaces
{
    public interface IReminderService
    {
        /// <summary>
        /// Envoie des rappels pour les séminaires à venir (dans 24h)
        /// </summary>
        Task SendSeminarRemindersAsync();

        /// <summary>
        /// Envoie une notification de confirmation de paiement
        /// </summary>
        Task SendPaymentConfirmationAsync(int inscriptionId);

        /// <summary>
        /// Envoie une notification quand un séminaire est annulé
        /// </summary>
        Task SendSeminarCancellationAsync(int seminaireId);

        /// <summary>
        /// Envoie une notification quand un séminaire est modifié
        /// </summary>
        Task SendSeminarUpdateAsync(int seminaireId, string updateMessage);

        /// <summary>
        /// Envoie une notification quand une place se libère
        /// </summary>
        Task SendWaitlistNotificationAsync(int seminaireId);

        /// <summary>
        /// Envoie une notification de bienvenue
        /// </summary>
        Task SendWelcomeNotificationAsync(int participantId);

        /// <summary>
        /// Nettoie les anciennes notifications (plus de 30 jours)
        /// </summary>
        Task CleanupOldNotificationsAsync();
    }
}
