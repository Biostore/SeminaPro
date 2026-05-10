using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeminaPro.Models
{
    [Table("Participants")]
    public class Participant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? NumeroTelephone { get; set; }

        // ✅ IMAGE CORRECTE (c'est celle que tu utilises partout)
        public string? ImageUrl { get; set; } = "/images/default-profile.png";

        [Required]
        public int SpecialiteId { get; set; }

        public Specialite? Specialite { get; set; }

        public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();

        [NotMapped]
        public string NomComplet => $"{Prenom} {Nom}";
    }
}