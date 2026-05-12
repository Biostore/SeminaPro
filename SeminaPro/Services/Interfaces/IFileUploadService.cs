using SeminaPro.Models.Enums;

namespace SeminaPro.Services.Interfaces
{
    /// <summary>
    /// Interface pour gérer l'upload et la suppression d'images
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Upload une image et retourne l'URL
        /// </summary>
        Task<(bool Success, string Url, string Message)> UploadImageAsync(
            IFormFile file,
            string folder,
            MediaType mediaType);

        /// <summary>
        /// Vérifie si un fichier est une image valide
        /// </summary>
        bool IsValidImage(IFormFile file);

        /// <summary>
        /// Supprime une image du serveur
        /// </summary>
        Task<bool> DeleteImageAsync(string fileUrl);

        /// <summary>
        /// Récupère la taille maximale autorisée
        /// </summary>
        long GetMaxFileSize();

        /// <summary>
        /// Récupère les extensions autorisées
        /// </summary>
        string[] GetAllowedExtensions();
    }
}
