using SeminaPro.Models;

namespace SeminaPro.Services.Interfaces
{
    public interface IInvoiceService
    {
        // Générer un numéro de facture
        string GenerateInvoiceNumber(int inscriptionId);

        // Générer le contenu HTML de la facture
        Task<string> GenerateInvoiceHtmlAsync(Inscription inscription);

        // Convertir HTML en PDF
        Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent);

        // Enregistrer la facture en base de données
        Task<bool> SaveInvoiceAsync(
            int inscriptionId,
            string invoiceNumber,
            byte[] pdfContent);

        // Récupérer une facture par son numéro
        Task<Inscription?> GetInvoiceAsync(string invoiceNumber);

        // Récupérer toutes les factures d'un participant
        Task<List<Inscription>> GetParticipantInvoicesAsync(int participantId);

        // Annuler une facture
        Task<bool> CancelInvoiceAsync(int inscriptionId, string? reason = null);

        // Relancer une facture impayée
        Task<bool> RemindInvoiceAsync(int inscriptionId);
    }
}
