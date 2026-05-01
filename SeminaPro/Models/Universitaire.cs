using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeminaPro.Models
{
    [Table("Universitaires")]
    public class Universitaire : Participant
    {
        [StringLength(100)]
        public string Niveau { get; set; } = string.Empty;

        [StringLength(200)]
        public string? NomUniversite { get; set; }
    }
}
