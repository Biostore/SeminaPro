using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeminaPro.Models
{
    /// <summary>
    /// Modèle pour représenter une spécialité avec logo/icône
    /// </summary>
    public class Specialite
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le libellé est requis")]
        [StringLength(100)]
        public string Libelle { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(10)]
        public string? Abbreviation { get; set; }

        /// <summary>
        /// URL du logo ou icône de la spécialité
        /// </summary>
        [StringLength(300)]
        public string? ImageUrl { get; set; } = "/images/default-speciality.png";

        // Alias (optionnel, rétrocompatibilité)
        [NotMapped]
        public string? Abrevaition
        {
            get => Abbreviation;
            set => Abbreviation = value;
        }

        // Relations
        public ICollection<Seminaire> Seminaires { get; set; } = new List<Seminaire>();
        public ICollection<Participant> Participants { get; set; } = new List<Participant>();

        public override string ToString()
        {
            return $"{Id} - {Libelle}";
        }
    }
}