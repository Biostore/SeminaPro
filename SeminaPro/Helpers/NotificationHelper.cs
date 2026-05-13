using SeminaPro.Services.Interfaces;

namespace SeminaPro.Helpers
{
    /// <summary>
    /// Helper pour créer des notifications à différents moments
    /// </summary>
    public static class NotificationHelper
    {
        /// <summary>
        /// Crée une notification d'inscription réussie
        /// </summary>
        public static async Task CreateInscriptionNotificationAsync(
            INotificationService notificationService,
            int participantId,
            string seminaireTitre)
        {
            await notificationService.AjouterNotificationAsync(
                participantId,
                titre: "Inscription Confirmée",
                message: $"Vous êtes maintenant inscrit au séminaire: {seminaireTitre}",
                type: "Inscription",
                lien: "/Dashboard/MesSeminaires"
            );
        }

        /// <summary>
        /// Crée une notification de paiement réussi
        /// </summary>
        public static async Task CreatePaiementNotificationAsync(
            INotificationService notificationService,
            int participantId,
            string seminaireTitre,
            decimal montant)
        {
            await notificationService.AjouterNotificationAsync(
                participantId,
                titre: "Paiement Validé",
                message: $"Votre paiement de {montant:C} a été confirmé pour {seminaireTitre}",
                type: "Paiement",
                lien: "/Dashboard/MesSeminaires"
            );
        }

        /// <summary>
        /// Crée une notification de facture générée
        /// </summary>
        public static async Task CreateFactureNotificationAsync(
            INotificationService notificationService,
            int participantId,
            string seminaireTitre,
            int inscriptionId)
        {
            await notificationService.AjouterNotificationAsync(
                participantId,
                titre: "Facture Disponible",
                message: $"Votre facture pour {seminaireTitre} est prête",
                type: "Facture",
                lien: $"/Seminaires/TelechargerFacture?id={inscriptionId}"
            );
        }

        /// <summary>
        /// Crée une notification de rappel avant le séminaire
        /// </summary>
        public static async Task CreateRappelNotificationAsync(
            INotificationService notificationService,
            int participantId,
            string seminaireTitre,
            DateTime dateDebut)
        {
            var jour = dateDebut.ToString("dd/MM/yyyy");
            await notificationService.AjouterNotificationAsync(
                participantId,
                titre: "Rappel Séminaire",
                message: $"Rappel: Le séminaire {seminaireTitre} débute le {jour}",
                type: "Rappel",
                lien: "/Dashboard/MesSeminaires"
            );
        }

        /// <summary>
        /// Crée une notification générique
        /// </summary>
        public static async Task CreateGenericNotificationAsync(
            INotificationService notificationService,
            int participantId,
            string titre,
            string message,
            string? lien = null)
        {
            await notificationService.AjouterNotificationAsync(
                participantId,
                titre: titre,
                message: message,
                type: "General",
                lien: lien
            );
        }

        /// <summary>
        /// Crée des notifications pour tous les participants d'un séminaire
        /// </summary>
        public static async Task CreateBroadcastNotificationAsync(
            INotificationService notificationService,
            List<int> participantIds,
            string titre,
            string message,
            string type = "General",
            string? lien = null)
        {
            var tasks = participantIds.Select(id =>
                notificationService.AjouterNotificationAsync(id, titre, message, type, lien)
            );

            await Task.WhenAll(tasks);
        }
    }
}
