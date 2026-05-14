using BCrypt.Net;
using SeminaPro.Services.Interfaces;

namespace SeminaPro.Services.Implementations
{
    /// <summary>
    /// Implémentation du service de gestion des mots de passe avec BCrypt
    /// </summary>
    public class PasswordService : IPasswordService
    {
        /// <summary>
        /// Hache un mot de passe en utilisant BCrypt avec un travail factor de 12
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Le mot de passe ne peut pas être vide", nameof(password));
            }

            // Utiliser un facteur de coût de 12 (bon équilibre entre sécurité et performance)
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Vérifie si un mot de passe correspond au hachage stocké
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Le mot de passe ne peut pas être vide", nameof(password));
            }

            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentException("Le hachén'peut pas être vide", nameof(hash));
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (InvalidOperationException)
            {
                // BCrypt lève une exception si le hachéest invalide
                return false;
            }
        }
    }
}
