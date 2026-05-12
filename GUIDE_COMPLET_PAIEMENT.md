# Guide Complet du Système de Paiement - Procédure Complète

## 📋 Vue d'Ensemble du Flux de Paiement

```
1. Utilisateur clique "S'inscrire"
        ↓
2. Redirection vers PAGE DE PAIEMENT
        ↓
3. Sélection de la méthode de paiement (Carte/PayPal/Virement)
        ↓
4. Remplissage du formulaire de paiement
        ↓
5. Clic sur "Procéder au Paiement"
        ↓
6. Validation côté CLIENT (JavaScript)
        ↓
7. Traitement côté SERVEUR (C#)
        ↓
8. Création de l'inscription
        ↓
9. Traitement du paiement
        ↓
10. Génération de la facture
        ↓
11. Redirection vers PAGE DE CONFIRMATION
        ↓
12. Affichage du récapitulatif + lien de téléchargement
```

## 🔐 Validations Implémentées

### Côté Client (JavaScript)

✅ Validation du numéro de carte (13-19 chiffres)
✅ Validation Luhn du numéro de carte
✅ Formatage automatique (ajout d'espaces toutes les 4 chiffres)
✅ Validation de la date d'expiration (MM/YY)
✅ Vérification que la carte n'est pas expirée
✅ Validation du CVV (3-4 chiffres)
✅ Validation du nom sur la carte (minimum 3 caractères)
✅ Conditions d'utilisation obligatoires

### Côté Serveur (C#)

✅ Vérification de session utilisateur
✅ Validation de la méthode de paiement
✅ Réapplication des validations du client
✅ Algorithme Luhn pour le numéro de carte
✅ Vérification de l'expiration de la carte
✅ Contrôle des places disponibles au séminaire
✅ Vérification des doublons d'inscription

## 📄 Pages et Endpoints

### 1. **Page de Paiement** (`/Seminaires/Paiement/{id}`)

**GET**
- Affiche le formulaire de paiement
- Vérifie les places disponibles
- Vérification d'authentification

**POST**
- Reçoit les données du paiement
- Valide les données
- Crée l'inscription
- Traite le paiement
- Génère la facture
- Redirige vers confirmation

**Formulaire inclus :**
```
- Méthode de paiement (radio buttons)
- Détails de carte (si carte sélectionnée)
- Conditions d'utilisation (checkbox)
- Résumé de la commande (sidebar)
- Bouton "Procéder au Paiement"
```

### 2. **Page de Confirmation** (`/Seminaires/ConfirmationPaiement/{id}`)

- Affiche le succès du paiement
- Affiche les détails de l'inscription
- Affiche le numéro de facture
- Lien de téléchargement de la facture
- Boutons de navigation

### 3. **Téléchargement de Facture** (`/Seminaires/TelechargerFacture/{id}`)

- Endpoint sécurisé
- Vérification d'authentification
- Vérification de propriété (l'utilisateur est bien le propriétaire)
- Retourne le fichier (HTML ou PDF)
- Enregistrement dans les logs

## 💾 Modèles de Données

### Inscription

```csharp
public class Inscription
{
    public int Id { get; set; }
    public DateTime DateInscription { get; set; }
    public bool AffichageConfirmation { get; set; }

    // Paiement
    public string? PaymentMethodId { get; set; }           // ID de l'intention de paiement
    public string PaymentStatus { get; set; }              // En Attente, Payée, Échouée
    public string? SelectedPaymentMethod { get; set; }     // card, paypal, transfer
    public DateTime? DatePaiement { get; set; }            // Date du paiement
    public decimal MontantPaye { get; set; }               // Montant payé
    public string? TransactionId { get; set; }             // ID de la transaction
    public string? FactureNumero { get; set; }             // Numéro de facture
    public DateTime? DateFacture { get; set; }             // Date de facturation

    // Relations
    public int ParticipantId { get; set; }
    public Participant? Participant { get; set; }
    public int SeminaireId { get; set; }
    public Seminaire? Seminaire { get; set; }
}
```

## 🔄 Flux de Données

### Request POST (Paiement)

```
{
  "id": 1,                                    // Séminaire ID
  "selectedPaymentMethod": "card",             // Méthode
  "cardName": "JEAN DUPONT",                  // Nom sur la carte
  "cardNumber": "4242 4242 4242 4242",       // Numéro de carte
  "cardExpiry": "12/25",                     // Date expiration
  "cardCvc": "123"                           // Cryptogramme
}
```

### Response After Payment

**Succès (301 Redirect)**
```
Location: /Seminaires/ConfirmationPaiement/{inscriptionId}
```

**Erreur (400 Bad Request)**
```
ValidationErrorMessage: "Numéro de carte invalide"
```

## 📊 État du Paiement

```
┌─────────────────────────────────────────────┐
│       LIFECYCLE DE L'INSCRIPTION            │
├─────────────────────────────────────────────┤
│ 1. En Attente                               │
│    - Inscription créée                      │
│    - En attente de paiement                 │
│    - Utilisateur remplit formulaire         │
│                                             │
│ 2. Payée (Succès)                          │
│    - Paiement traité                        │
│    - Facture générée                        │
│    - Confirmation envoyée                   │
│    - Accès au séminaire confirmé            │
│                                             │
│ 3. Échouée (Erreur)                        │
│    - Paiement refusé                        │
│    - Message d'erreur affiché               │
│    - Formulaire prêt pour nouvel essai      │
│                                             │
│ 4. Annulée (Optionnel)                     │
│    - Remboursement initié                   │
│    - Inscription invalidée                  │
│                                             │
└─────────────────────────────────────────────┘
```

## 🧪 Test du Système

### Numéros de Carte de Test (En développement)

```
Succès:           4242 4242 4242 4242
Décliné:          4000 0000 0000 0002
Expiration:       4000 0000 0000 0069
Erreur CVV:       4000 0000 0000 0127
Succès 3D:        4000 0025 0000 3155
```

### Formulaire de Test

```
Nom:              JEAN DUPONT
Numéro:           4242 4242 4242 4242
Expiration:       12/25 (ou ultérieure)
CVV:              Trois chiffres quelconques (ex: 123)
```

## 📝 Logging

Tous les événements de paiement sont enregistrés :

```
INFO: Inscription créée pour participant 5 au séminaire 1 - ID: 10
INFO: Paiement réussi pour inscription 10
INFO: Facture générée: FAC-2025-000010 pour inscription 10
INFO: Facture téléchargée: FAC-2025-000010.html par user@example.com
```

## 🚀 Prochaines Étapes

### Phase 1 - Production Immédiate

- [ ] Installer le SDK Stripe officiel
- [ ] Configurer les clés API Stripe
- [ ] Implémenter Stripe.js côté client
- [ ] Tester avec environnement de test Stripe
- [ ] Ajouter gestion des webhooks Stripe

### Phase 2 - Amélioration

- [ ] Intégration PayPal officielle
- [ ] Conversion HTML → PDF (SelectPdf)
- [ ] Envoi d'email avec facture
- [ ] Support du multi-devise
- [ ] Remboursements partiels/totaux

### Phase 3 - Analytics

- [ ] Dashboard de paiement (Admin)
- [ ] Rapports de revenus
- [ ] Graphiques de paiement
- [ ] Taux de conversion

## 🔗 Routes de Paiement

```
GET  /Seminaires/Paiement/{id}              ✅ Formulaire
POST /Seminaires/Paiement/{id}              ✅ Traitement
GET  /Seminaires/ConfirmationPaiement/{id}  ✅ Confirmation
GET  /Seminaires/TelechargerFacture/{id}    ✅ Téléchargement
```

## 📚 Architecture Services

### IPaymentService
```csharp
Task<PaymentIntentDto> CreatePaymentIntentAsync()
Task<bool> VerifyPaymentAsync()
Task<bool> ProcessWebhookAsync()
Task<bool> CancelPaymentAsync()
```

### IInvoiceService
```csharp
string GenerateInvoiceNumber()
Task<string> GenerateInvoiceHtmlAsync()
Task<byte[]> ConvertHtmlToPdfAsync()
Task<bool> SaveInvoiceAsync()
```

## 🛡️ Sécurité

✅ **Session**
- Vérification de UserEmail dans la session
- Timeout de session (1 heure)

✅ **Authentification**
- Redirection vers login si non connecté
- Vérification de propriété avant accès

✅ **Validation**
- Côté client ET serveur
- Algorithme Luhn pour cartes
- Vérification d'expiration

✅ **Données Sensibles**
- Pas de stockage de numéro complet
- Utilisation de PaymentMethodId
- Logs sécurisés

## 📞 Support

Pour intégrer Stripe complètement :
- Documentation: https://stripe.com/docs
- Guide: https://stripe.com/docs/payments/accept-card-payments

Pour les convertisseurs PDF :
- SelectPdf: https://selectpdf.com/
- iTextSharp: https://github.com/itext/itext7-dotnet
- PdfSharp: http://www.pdfsharp.net/
