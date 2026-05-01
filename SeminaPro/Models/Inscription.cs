using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeminaPro.Models
{
    public class Inscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime DateInscription { get; set; }

        public bool AffichageConfirmation { get; set; }

        public int ParticipantId { get; set; }
        [ForeignKey(nameof(ParticipantId))]
        public Participant? Participant { get; set; }

        public int SeminaireId { get; set; }
        [ForeignKey(nameof(SeminaireId))]
        public Seminaire? Seminaire { get; set; }
    }
}
