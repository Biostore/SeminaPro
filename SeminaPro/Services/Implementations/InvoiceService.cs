using SeminaPro.Data;
using SeminaPro.Models;
using SeminaPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SeminaPro.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvoiceService> _logger;
        private readonly IConfiguration _configuration;

        public InvoiceService(
            ApplicationDbContext context,
            ILogger<InvoiceService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public string GenerateInvoiceNumber(int inscriptionId)
        {
            // Format: FAC-2025-000001
            var year = DateTime.Now.Year;
            var invoiceNumber = $"FAC-{year}-{inscriptionId:D6}";
            return invoiceNumber;
        }

        public async Task<string> GenerateInvoiceHtmlAsync(Inscription inscription)
        {
            try
            {
                if (inscription.Participant == null || inscription.Seminaire == null)
                {
                    return string.Empty;
                }

                var invoiceNumber = GenerateInvoiceNumber(inscription.Id);
                var invoiceDate = inscription.DateFacture ?? DateTime.Now;
                var companyName = _configuration["Company:Name"] ?? "SeminaPro";
                var companyEmail = _configuration["Company:Email"] ?? "contact@seminapro.fr";
                var companyPhone = _configuration["Company:Phone"] ?? "+33 1 23 45 67 89";

                var html = $@"
<!DOCTYPE html>
<html lang='fr'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Facture {invoiceNumber}</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Arial', sans-serif;
            background-color: #f5f5f5;
            padding: 20px;
        }}
        .container {{
            max-width: 900px;
            margin: 0 auto;
            background-color: white;
            padding: 40px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }}
        .header {{
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            border-bottom: 3px solid #0f6bff;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }}
        .company-info h2 {{
            color: #0f6bff;
            font-size: 28px;
            margin-bottom: 10px;
        }}
        .company-info p {{
            color: #666;
            font-size: 12px;
            margin: 3px 0;
        }}
        .invoice-info {{
            text-align: right;
        }}
        .invoice-info h3 {{
            color: #0f6bff;
            font-size: 20px;
            margin-bottom: 10px;
        }}
        .invoice-info p {{
            color: #666;
            font-size: 13px;
            margin: 5px 0;
        }}
        .invoice-info .number {{
            font-weight: bold;
            color: #333;
            font-size: 16px;
        }}
        .section {{
            margin-bottom: 30px;
        }}
        .section-title {{
            background-color: #f9f9f9;
            padding: 10px 15px;
            border-left: 4px solid #0f6bff;
            font-weight: bold;
            color: #333;
            margin-bottom: 15px;
        }}
        .section-content {{
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
        }}
        .info-block {{
            padding: 10px 0;
        }}
        .label {{
            color: #999;
            font-size: 12px;
            font-weight: bold;
            text-transform: uppercase;
            margin-bottom: 5px;
        }}
        .value {{
            color: #333;
            font-size: 14px;
            font-weight: 500;
        }}
        .items-table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        .items-table thead {{
            background-color: #f9f9f9;
            border-bottom: 2px solid #0f6bff;
        }}
        .items-table th {{
            padding: 12px;
            text-align: left;
            color: #666;
            font-weight: bold;
            font-size: 12px;
            text-transform: uppercase;
        }}
        .items-table td {{
            padding: 15px 12px;
            border-bottom: 1px solid #eee;
            color: #333;
        }}
        .items-table tbody tr:last-child td {{
            border-bottom: none;
        }}
        .total-row {{
            background-color: #f9f9f9;
            font-weight: bold;
        }}
        .total-section {{
            margin-top: 30px;
            text-align: right;
            padding-top: 20px;
            border-top: 2px solid #eee;
        }}
        .total-amount {{
            font-size: 24px;
            color: #0f6bff;
            font-weight: bold;
            margin-top: 10px;
        }}
        .footer {{
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #eee;
            text-align: center;
            color: #999;
            font-size: 11px;
        }}
        .status {{
            display: inline-block;
            background-color: #22c55e;
            color: white;
            padding: 5px 10px;
            border-radius: 3px;
            font-size: 12px;
            font-weight: bold;
            margin-top: 10px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <!-- Header -->
        <div class='header'>
            <div class='company-info'>
                <h2>{companyName}</h2>
                <p>Email: {companyEmail}</p>
                <p>Téléphone: {companyPhone}</p>
            </div>
            <div class='invoice-info'>
                <h3>Facture</h3>
                <p class='number'>{invoiceNumber}</p>
                <p><strong>Date:</strong> {invoiceDate:dd/MM/yyyy}</p>
                <p><strong>Statut:</strong><span class='status'>Payée</span></p>
            </div>
        </div>

        <!-- Client Info -->
        <div class='section'>
            <div class='section-title'>Informations du Participant</div>
            <div class='section-content'>
                <div>
                    <div class='info-block'>
                        <div class='label'>Nom</div>
                        <div class='value'>{inscription.Participant.Nom} {inscription.Participant.Prenom}</div>
                    </div>
                    <div class='info-block'>
                        <div class='label'>Email</div>
                        <div class='value'>{inscription.Participant.Email}</div>
                    </div>
                    <div class='info-block'>
                        <div class='label'>Téléphone</div>
                        <div class='value'>{inscription.Participant.NumeroTelephone}</div>
                    </div>
                </div>
                <div>
                    <div class='info-block'>
                        <div class='label'>Type Participant</div>
                        <div class='value'>{inscription.Participant.GetType().Name}</div>
                    </div>
                    <div class='info-block'>
                        <div class='label'>Date d'Inscription</div>
                        <div class='value'>{inscription.DateInscription:dd/MM/yyyy HH:mm}</div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Items -->
        <div class='section'>
            <div class='section-title'>Détails du Séminaire</div>
            <table class='items-table'>
                <thead>
                    <tr>
                        <th>Description</th>
                        <th style='width: 150px;'>Prix Unitaire</th>
                        <th style='width: 100px; text-align: center;'>Quantité</th>
                        <th style='width: 150px; text-align: right;'>Total</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>
                            <strong>{inscription.Seminaire.Titre}</strong><br/>
                            Code: {inscription.Seminaire.Code}<br/>
                            Date: {inscription.Seminaire.DateSeminaire:dd/MM/yyyy HH:mm}<br/>
                            Lieu: {inscription.Seminaire.Lieu}
                        </td>
                        <td>{inscription.Seminaire.Tarif:N2} €</td>
                        <td style='text-align: center;'>1</td>
                        <td style='text-align: right; font-weight: bold;'>{inscription.Seminaire.Tarif:N2} €</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Total -->
        <div class='total-section'>
            <div style='margin-bottom: 10px;'>
                <span style='font-weight: bold;'>Total HT:</span>
                <span style='float: right;'>{inscription.Seminaire.Tarif:N2} €</span>
            </div>
            <div style='margin-bottom: 10px;'>
                <span style='font-weight: bold;'>TVA (0%):</span>
                <span style='float: right;'>0,00 €</span>
            </div>
            <div class='total-amount'>
                <span>Total TTC:</span>
                <span style='float: right;'>{inscription.Seminaire.Tarif:N2} €</span>
            </div>
        </div>

        <!-- Footer -->
        <div class='footer'>
            <p>Cette facture a été générée automatiquement par {companyName}</p>
            <p>Merci pour votre participation!</p>
        </div>
    </div>
</body>
</html>";

                return html;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du HTML de facture");
                return string.Empty;
            }
        }

        public async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent)
        {
            try
            {
                // En production, utilisez SelectPdf, iTextSharp, ou Puppeteer
                // Pour l'instant, retourner une version HTML encodée en UTF-8

                // Vous pouvez remplacer cette implémentation par une vraie conversion PDF
                // Exemple avec iTextSharp:
                // var document = new Document();
                // var writer = PdfWriter.GetInstance(document, stream);
                // // ... ajouter contenu ...

                var htmlBytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);
                _logger.LogInformation("Contenu HTML converti: {Size} bytes", htmlBytes.Length);
                return htmlBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la conversion HTML en PDF");
                return Array.Empty<byte>();
            }
        }

        public async Task<bool> SaveInvoiceAsync(
            int inscriptionId,
            string invoiceNumber,
            byte[] pdfContent)
        {
            try
            {
                var inscription = await _context.Inscriptions.FindAsync(inscriptionId);
                if (inscription == null)
                {
                    return false;
                }

                inscription.FactureNumero = invoiceNumber;
                inscription.DateFacture = DateTime.Now;

                _context.Inscriptions.Update(inscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Facture {Number} enregistrée pour inscription {Id}", 
                    invoiceNumber, inscriptionId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de la facture");
                return false;
            }
        }

        /// <summary>
        /// Récupère une facture par son numéro
        /// </summary>
        public async Task<Inscription?> GetInvoiceAsync(string invoiceNumber)
        {
            try
            {
                var inscription = await _context.Inscriptions
                    .Include(i => i.Participant)
                    .Include(i => i.Seminaire)
                    .FirstOrDefaultAsync(i => i.FactureNumero == invoiceNumber);

                if (inscription == null)
                {
                    _logger.LogWarning("Facture non trouvée: {InvoiceNumber}", invoiceNumber);
                }

                return inscription;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la facture");
                return null;
            }
        }

        /// <summary>
        /// Récupère toutes les factures d'un participant
        /// </summary>
        public async Task<List<Inscription>> GetParticipantInvoicesAsync(int participantId)
        {
            try
            {
                var invoices = await _context.Inscriptions
                    .Where(i => i.ParticipantId == participantId && i.FactureNumero != null)
                    .Include(i => i.Seminaire)
                    .OrderByDescending(i => i.DateFacture)
                    .ToListAsync();

                _logger.LogInformation(
                    "Récupération de {Count} factures pour participant {Id}",
                    invoices.Count,
                    participantId);

                return invoices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des factures du participant");
                return new List<Inscription>();
            }
        }

        /// <summary>
        /// Annule une facture (génère un avoir)
        /// </summary>
        public async Task<bool> CancelInvoiceAsync(int inscriptionId, string? reason = null)
        {
            try
            {
                var inscription = await _context.Inscriptions.FindAsync(inscriptionId);
                if (inscription == null)
                {
                    _logger.LogWarning("Inscription non trouvée pour l'annulation: {Id}", inscriptionId);
                    return false;
                }

                // Créer un avoir
                var creditNoteNumber = $"AVOIR-{inscription.FactureNumero}";
                inscription.FactureNumero = creditNoteNumber;
                inscription.PaymentStatus = "Annulée";

                _context.Inscriptions.Update(inscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Facture annulée - Créé avoir: {CreditNote} - Raison: {Reason}",
                    creditNoteNumber,
                    reason ?? "Non spécifiée");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'annulation de la facture");
                return false;
            }
        }

        /// <summary>
        /// Relance une facture impayée
        /// </summary>
        public async Task<bool> RemindInvoiceAsync(int inscriptionId)
        {
            try
            {
                var inscription = await _context.Inscriptions
                    .Include(i => i.Participant)
                    .Include(i => i.Seminaire)
                    .FirstOrDefaultAsync(i => i.Id == inscriptionId);

                if (inscription == null)
                {
                    _logger.LogWarning("Inscription non trouvée pour relance: {Id}", inscriptionId);
                    return false;
                }

                if (inscription.PaymentStatus == "Payée")
                {
                    _logger.LogWarning("La facture est déjà payée - pas de relance: {Id}", inscriptionId);
                    return false;
                }

                // En production: envoyer email de relance
                _logger.LogInformation(
                    "Relance facture pour: {Invoice} - Email: {Email}",
                    inscription.FactureNumero,
                    inscription.Participant?.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la relance de facture");
                return false;
            }
        }
    }
}
