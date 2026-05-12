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
    }
}
