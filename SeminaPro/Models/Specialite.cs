using System.ComponentModel.DataAnnotations;

namespace SeminaPro.Models
{
    public class Specialite
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le libellé est requis")]
        [StringLength(100, ErrorMessage = "Le libellé ne doit pas dépasser 100 caractères")]
        public string Libelle { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "L'abréviation ne doit pas dépasser 10 caractères")]
        public string? Abbreviation { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<Seminaire> Seminaires { get; set; } = new List<Seminaire>();
        public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    }
}
