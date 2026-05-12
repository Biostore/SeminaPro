using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SeminaPro.Models.Enums;

namespace SeminaPro.Models
{
    /// <summary>
    /// Modèle pour gérer les fichiers médias (images, documents, etc.)
    /// Pattern : Un fichier = Une entité indépendante réutilisable
    /// </summary>
    public class MediaFile
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom du fichier est requis")]
        [StringLength(255, ErrorMessage = "Le nom ne doit pas dépasser 255 caractères")]
        public string FileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'URL du fichier est requise")]
        [Url(ErrorMessage = "L'URL n'est pas valide")]
        public string FileUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le type de média est requis")]
        public MediaType MediaType { get; set; }

        [Required(ErrorMessage = "Le type MIME est requis")]
        [StringLength(100)]
        public string MimeType { get; set; } = string.Empty;

        [Range(0, long.MaxValue, ErrorMessage = "La taille du fichier n'est pas valide")]
        public long FileSize { get; set; }

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? UploadedBy { get; set; }

        /// <summary>
        /// Clés étrangères pour les relations polymorphes
        /// Seule UNE de ces clés devrait être remplie à la fois
        /// </summary>

        public int? ParticipantId { get; set; }
        [ForeignKey(nameof(ParticipantId))]
        public Participant? Participant { get; set; }

        public int? SeminaireId { get; set; }
        [ForeignKey(nameof(SeminaireId))]
        public Seminaire? Seminaire { get; set; }

        public int? SpecialiteId { get; set; }
        [ForeignKey(nameof(SpecialiteId))]
        public Specialite? Specialite { get; set; }

        [NotMapped]
        public string FileExtension => Path.GetExtension(FileName).ToLower();

        [NotMapped]
        public bool IsImage => FileExtension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".svg" => true,
            _ => false
        };

        [NotMapped]
        public bool IsDocument => FileExtension switch
        {
            ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" => true,
            _ => false
        };

        [NotMapped]
        public string FileSizeFormatted => FormatFileSize(FileSize);

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
