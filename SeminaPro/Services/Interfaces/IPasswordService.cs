using BCrypt.Net;

namespace SeminaPro.Services.Interfaces
{
    /// <summary>
    /// Service pour gérer le hachage et la vérification sécurisée des mots de passe
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Hache un mot de passe en utilisant BCrypt
        /// </summary>
        /// <param name="password">Le mot de passe en texte clair</param>
        /// <returns>Le mot de passe hachéet salé</returns>
        string HashPassword(string password);

        /// <summary>
        /// Vérifie si un mot de passe correspond au hachage stocké
        /// </summary>
        /// <param name="password">Le mot de passe en texte clair</param>
        /// <param name="hash">Le hachéstocké en base de données</param>
        /// <returns>True si le mot de passe correspond, false sinon</returns>
        bool VerifyPassword(string password, string hash);
    }
}
