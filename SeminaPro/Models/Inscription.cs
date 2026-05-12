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

        // Informations de paiement
        public string? PaymentMethodId { get; set; }
        public string PaymentStatus { get; set; } = "En Attente"; // En Attente, Payée, Échouée
        public DateTime? DatePaiement { get; set; }
        public decimal MontantPaye { get; set; }
        public string? TransactionId { get; set; }
        public string? FactureNumero { get; set; }
        public DateTime? DateFacture { get; set; }

        public int ParticipantId { get; set; }
        [ForeignKey(nameof(ParticipantId))]
        public Participant? Participant { get; set; }

        public int SeminaireId { get; set; }
        [ForeignKey(nameof(SeminaireId))]
        public Seminaire? Seminaire { get; set; }
    }
}
