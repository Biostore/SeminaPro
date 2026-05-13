using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeminaPro.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        public Participant? Participant { get; set; }

        [Required]
        [MaxLength(100)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsRead { get; set; } = false;

        [MaxLength(50)]
        public string Type { get; set; } = "General"; // "Inscription", "Paiement", "Séminaire", "Facture", "Rappel"

        public string? Lien { get; set; } // URL vers la ressource concernée
    }
}
