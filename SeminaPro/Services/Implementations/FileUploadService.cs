using SeminaPro.Models.Enums;
using SeminaPro.Services.Interfaces;

namespace SeminaPro.Services.Implementations
{
    /// <summary>
    /// Service pour gérer l'upload et la suppression d'images
    /// </summary>
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        // Configuration
        private const long MAX_FILE_SIZE = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] ALLOWED_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] ALLOWED_MIME_TYPES = { "image/jpeg", "image/png", "image/gif", "image/webp" };

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<(bool Success, string Url, string Message)> UploadImageAsync(
            IFormFile file,
            string folder,
            MediaType mediaType)
        {
            try
            {
                // Validation
                if (file == null || file.Length == 0)
                    return (false, "", "Aucun fichier fourni");

                if (!IsValidImage(file))
                    return (false, "", "Le fichier n'est pas une image valide. Formats autorisés: JPG, PNG, GIF, WebP");

                if (file.Length > MAX_FILE_SIZE)
                    return (false, "", $"La taille du fichier ne doit pas dépasser {MAX_FILE_SIZE / (1024 * 1024)} MB");

                // Créer le dossier s'il n'existe pas
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // Générer un nom de fichier unique
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                // Sauvegarder le fichier
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Construire l'URL
                var fileUrl = $"/uploads/{folder}/{uniqueFileName}";

                _logger.LogInformation(
                    $"Image uploadée avec succès: {fileUrl} (Type: {mediaType}, Taille: {file.Length} bytes)");

                return (true, fileUrl, "Image uploadée avec succès");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'upload de l'image pour {mediaType}");
                return (false, "", "Une erreur s'est produite lors de l'upload");
            }
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Vérifier l'extension
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!ALLOWED_EXTENSIONS.Contains(extension))
                return false;

            // Vérifier le type MIME
            if (!ALLOWED_MIME_TYPES.Contains(file.ContentType.ToLower()))
                return false;

            // Vérifier les octets magiques (magic bytes)
            using (var stream = file.OpenReadStream())
            {
                byte[] buffer = new byte[8];
                stream.Read(buffer, 0, 8);

                // Vérifier les signatures de fichier
                if (!IsMagicBytesValid(buffer, extension))
                    return false;
            }

            return true;
        }

        public async Task<bool> DeleteImageAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUrl) || fileUrl.StartsWith("http"))
                    return false; // Ne pas supprimer les URLs externes

                var filePath = Path.Combine(_environment.WebRootPath, fileUrl.TrimStart('/'));

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning($"Fichier non trouvé: {filePath}");
                    return false;
                }

                File.Delete(filePath);
                _logger.LogInformation($"Image supprimée: {fileUrl}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la suppression de l'image: {fileUrl}");
                return false;
            }
        }

        public long GetMaxFileSize() => MAX_FILE_SIZE;

        public string[] GetAllowedExtensions() => ALLOWED_EXTENSIONS;

        /// <summary>
        /// Vérifie les octets magiques du fichier
        /// </summary>
        private bool IsMagicBytesValid(byte[] buffer, string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
                ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
                ".gif" => (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46), // GIF
                ".webp" => buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50, // WEBP
                _ => false
            };
        }
    }
}
