using SeminaPro.Services.Interfaces;
using SeminaPro.Data;

namespace SeminaPro.Migrations
{
    /// <summary>
    /// Script de migration des données pour initialiser les utilisateurs par défaut
    /// avec des mots de passe hachés
    /// </summary>
    public static class PasswordMigrationHelper
    {
        /// <summary>
        /// Hache et met à jour les mots de passe en clair des utilisateurs par défaut
        /// À exécuter une seule fois après la migration
        /// </summary>
        public static void MigrateDefaultUserPasswords(
            ApplicationDbContext context,
            IPasswordService passwordService)
        {
            // Définir les utilisateurs par défaut avec leurs mots de passe
            var defaultUsers = new[]
            {
                new { Email = "admin@seminapro.com", Password = "admin123" },
                new { Email = "user@seminapro.com", Password = "user123" }
            };

            foreach (var user in defaultUsers)
            {
                var participant = context.Participants.FirstOrDefault(p => p.Email == user.Email);

                if (participant != null && string.IsNullOrEmpty(participant.PasswordHash))
                {
                    // Hacher le mot de passe et le stocker
                    participant.PasswordHash = passwordService.HashPassword(user.Password);
                    context.Participants.Update(participant);
                }
            }

            context.SaveChanges();
        }
    }
}
