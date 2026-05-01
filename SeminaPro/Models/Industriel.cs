using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeminaPro.Models
{
    [Table("Industriels")]
    public class Industriel : Participant
    {
        [StringLength(100)]
        public string Fonction { get; set; } = string.Empty;

        [StringLength(200)]
        public string? NomEntreprise { get; set; }
    }
}
