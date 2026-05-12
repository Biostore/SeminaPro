# Documentation : Système de Paiement et Facturation

## Vue d'ensemble

Un système complet de paiement et de facturation a été intégré à votre application SeminaPro pour permettre aux participants de payer leurs inscriptions et télécharger leurs factures.

## Flux d'Inscription avec Paiement

```
1. Participant accède à la page d'inscription du séminaire
2. Clique sur "S'inscrire"
3. Est redirigé vers la page de PAIEMENT
4. Sélectionne une méthode de paiement (Carte, PayPal, Virement)
5. Remplit les détails du paiement
6. Valide le paiement
7. Reçoit une confirmation et une facture téléchargeable
```

## Fichiers Créés

### Services

1. **IPaymentService.cs** - Interface pour les services de paiement
2. **StripePaymentService.cs** - Implémentation Stripe (à configurer)
3. **IInvoiceService.cs** - Interface pour la génération de factures
4. **InvoiceService.cs** - Service de génération et gestion des factures

### Pages Razor

1. **Paiement.cshtml** / **Paiement.cshtml.cs** - Page de paiement avec formulaire
2. **TelechargerFacture.cshtml.cs** - Endpoint de téléchargement de facture

### Modèles

Le modèle `Inscription` a été enrichi avec :
- `PaymentMethodId` - ID de la méthode de paiement
- `PaymentStatus` - Statut du paiement (En Attente, Payée, Échouée)
- `DatePaiement` - Date du paiement
- `MontantPaye` - Montant payé
- `TransactionId` - ID de la transaction
- `FactureNumero` - Numéro de facture généré
- `DateFacture` - Date de génération de la facture

## Configuration Requise

### 1. Mise à jour de la Base de Données

```powershell
# Appliquer la migration
dotnet ef database update
```

### 2. Configuration Stripe (Optionnel pour Production)

Ajoutez à `appsettings.json`:

```json
{
  "Stripe": {
    "PublicKey": "pk_test_xxxxx",
    "SecretKey": "sk_test_xxxxx"
  },
  "Company": {
    "Name": "SeminaPro",
    "Email": "contact@seminapro.fr",
    "Phone": "+33 1 23 45 67 89"
  }
}
```

### 3. Installation des Packages NuGet (Recommandé)

Pour une implémentation Stripe complète :
```bash
dotnet add package Stripe.net
```

Pour la conversion HTML to PDF :
```bash
dotnet add package SelectPdf
# ou
dotnet add package iTextSharp
```

## Utilisation

### Pour les Participants

1. **S'inscrire à un séminaire**
   - Naviguer vers la liste des séminaires
   - Cliquer sur "S'inscrire"
   - Remplir le formulaire de paiement
   - Valider

2. **Télécharger la facture**
   - Aller au tableau de bord
   - Cliquer sur "Télécharger la facture" pour chaque inscription payée

### Pour les Administrateurs

#### Vue des Inscriptions Payantes

Vous pouvez voir le statut de paiement dans la page Admin des inscriptions :
- `PaymentStatus` - Statut du paiement
- `DatePaiement` - Quand le paiement a été reçu
- `FactureNumero` - Numéro de facture émis

#### Exporter les Factures

Les factures générées automatiquement contiennent :
- Informations de la compagnie
- Détails du participant
- Détails du séminaire
- Montant TTC
- Date de facturation

## Pages et Routes

### Routes de Paiement

```
GET  /Seminaires/Paiement/{id}          - Afficher le formulaire
POST /Seminaires/Paiement/{id}          - Traiter le paiement
POST /Seminaires/Paiement/ConfirmPayment - Confirmer après paiement
```

### Routes de Facture

```
GET /Seminaires/TelechargerFacture/{id} - Télécharger facture (HTML/PDF)
```

## Architecture du Paiement

### Page de Paiement (Paiement.cshtml)

Formulaire moderne avec :
- **3 méthodes de paiement** : Carte, PayPal, Virement
- **Détails de carte** : Numéro, Expiration, CVV (masqués automatiquement)
- **Résumé de commande** : Affiche les détails du séminaire
- **Validations côté client** : Formatage automatique des champs
- **Design responsive** : Adapté pour mobile et desktop

### Service de Paiement

Le `StripePaymentService` gère :
- Création des intentions de paiement
- Vérification des paiements
- Gestion des webhooks (Stripe)
- Annulation de paiements

### Service de Facture

Le `InvoiceService` gère :
- Génération automatique de numéros de facture (FAC-2025-000001)
- Création du contenu HTML de la facture
- Conversion en PDF (à implémenter)
- Enregistrement en base de données

## Format de la Facture

La facture générée contient :
```
┌─────────────────────────────────┐
│  En-tête (Logo, Facture #)      │
├─────────────────────────────────┤
│  Participant                    │
│  Séminaire                      │
│  Montant                        │
├─────────────────────────────────┤
│  Total TTC                      │
├─────────────────────────────────┤
│  Pied de page                   │
└─────────────────────────────────┘
```

## Prochaines Étapes (Production)

### 1. Intégration Stripe Complète

```csharp
// Installer le SDK Stripe
// Configurer les clés API
// Implémenter Stripe.js côté client
// Gérer les webhooks Stripe
```

### 2. Conversion HTML to PDF

Remplacez `ConvertHtmlToPdfAsync` pour utiliser :
- SelectPdf
- iTextSharp
- Puppeteer (Chrome headless)
- PdfSharpXML

### 3. Email de Confirmation

Ajouter l'envoi automatique d'email :
```csharp
await _emailService.SendInvoiceAsync(
    inscription.Participant.Email,
    facturePdf,
    invoiceNumber);
```

### 4. Gestion des Remboursements

Implémenter :
- Annulation de paiement
- Remboursement partiel/total
- Historique des transactions

### 5. Rapports et Analytics

- Revenu par séminaire
- Taux de paiement
- Méthodes de paiement les plus utilisées

## Sécurité

### Implémenté

✅ Vérification du propriétaire avant téléchargement de facture
✅ Validation de session utilisateur
✅ Chiffrement de la transmission (HTTPS)
✅ Validation des données d'entrée

### À Ajouter (Production)

- ⚠️ Stockage sécurisé des données de carte (PCI DSS)
- ⚠️ Validation signature Webhook
- ⚠️ Rate limiting sur les endpoints de paiement
- ⚠️ Audit trail complet
- ⚠️ Chiffrement des montants en base de données

## Débogage

### Logs

Les actions de paiement sont loggées :
```
Payment intent créé pour inscription 5: 299.99€
Facture FAC-2025-000005 enregistrée pour inscription 5
Facture téléchargée: FAC-2025-000005.html par user@example.com
```

### Tests

Numéros de carte de test Stripe :
```
Succès:     4242 4242 4242 4242
Décliné:    4000 0000 0000 0002
Expiration: 4000 0000 0000 0069
CVV:        4000 0000 0000 0127
```

## Support & Améliorations

Pour toute question ou amélioration, veuillez consulter :
- Documentation Stripe : https://stripe.com/docs
- Microsoft Learn - Paiements : https://learn.microsoft.com/
- GitHub SeminaPro : https://github.com/Biostore/SeminaPro
