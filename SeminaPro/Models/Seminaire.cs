using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeminaPro.Models
{
    public class Seminaire
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le code du séminaire est requis")]
        [StringLength(50, ErrorMessage = "Le code ne doit pas dépasser 50 caractères")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le titre est requis")]
        [StringLength(200, ErrorMessage = "Le titre ne doit pas dépasser 200 caractères")]
        public string Titre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le lieu est requis")]
        [StringLength(100)]
        public string Lieu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le tarif est requis")]
        [Range(0, 100000, ErrorMessage = "Le tarif doit être entre 0 et 100000")]
        public decimal Tarif { get; set; }

        [Required(ErrorMessage = "La date du séminaire est requise")]
        public DateTime DateSeminaire { get; set; }

        [Range(1, 1000, ErrorMessage = "Le nombre maximal doit être entre 1 et 1000")]
        public int NombreMaximal { get; set; }

        public int SpecialiteId { get; set; }
        [ForeignKey(nameof(SpecialiteId))]
        public Specialite? Specialite { get; set; }

        public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
    }
}
