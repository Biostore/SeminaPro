using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeminaPro.Models
{
    [Table("Participants")]
    public class Participant
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(100, ErrorMessage = "Le nom ne doit pas dépasser 100 caractères")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(100, ErrorMessage = "Le prénom ne doit pas dépasser 100 caractères")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "L'email doit être valide")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Le téléphone doit être valide")]
        public string? NumeroTelephone { get; set; }

        public int SpecialiteId { get; set; }
        [ForeignKey(nameof(SpecialiteId))]
        public Specialite? Specialite { get; set; }

        public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
    }
}
