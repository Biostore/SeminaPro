using SeminaPro.Models;

namespace SeminaPro.Services.Interfaces
{
    public interface IPaymentService
    {
        // Créer une intention de paiement
        Task<PaymentIntentDto> CreatePaymentIntentAsync(
            int inscriptionId,
            Participant participant,
            Seminaire seminaire);

        // Vérifier le paiement
        Task<bool> VerifyPaymentAsync(string paymentIntentId);

        // Traiter le paiement webhook
        Task<bool> ProcessWebhookAsync(string payload, string signature);

        // Annuler un paiement
        Task<bool> CancelPaymentAsync(string paymentIntentId);
    }

    public class PaymentIntentDto
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
    }
}
