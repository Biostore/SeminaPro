# Système de Notifications - SeminaPro

## 📋 Vue d'ensemble

Le système de notifications permet aux participants de recevoir des mises à jour importantes sur leurs inscriptions, paiements, et séminaires.

## 🗂️ Structure

```
Models/
├── Notification.cs              # Modèle de données

Services/
├── Interfaces/
│   └── INotificationService.cs  # Interface du service
└── Implementations/
    └── NotificationService.cs   # Implémentation du service

Controllers/
└── NotificationsController.cs   # API REST

Pages/
├── Dashboard/
│   ├── Notifications.cshtml     # Page de tous les notifications
│   └── Notifications.cshtml.cs
└── Shared/
    ├── NotificationDropdown.cshtml         # Composant dropdown
    └── NotificationDropdownModel.cs

Helpers/
└── NotificationHelper.cs        # Utilitaires

wwwroot/css/
└── notifications.css            # Styles responsive
```

## 🚀 Utilisation

### 1. **Dans un PageModel** - Créer une notification

```csharp
public class MonPageModel : PageModel
{
    private readonly INotificationService _notificationService;

    public MonPageModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        int participantId = 1;

        // Méthode simple
        await _notificationService.AjouterNotificationAsync(
            participantId,
            titre: "Mon Titre",
            message: "Mon message...",
            type: "Inscription",
            lien: "/path/to/page"
        );

        return Page();
    }
}
```

### 2. **Utiliser le NotificationHelper**

```csharp
using SeminaPro.Helpers;

// Après une inscription réussie
await NotificationHelper.CreateInscriptionNotificationAsync(
    _notificationService,
    participantId,
    seminaireTitre: "C# Avancé"
);

// Après un paiement
await NotificationHelper.CreatePaiementNotificationAsync(
    _notificationService,
    participantId,
    seminaireTitre: "C# Avancé",
    montant: 199.99m
);

// Facture générée
await NotificationHelper.CreateFactureNotificationAsync(
    _notificationService,
    participantId,
    seminaireTitre: "C# Avancé",
    inscriptionId: 5
);

// Rappel avant séminaire
await NotificationHelper.CreateRappelNotificationAsync(
    _notificationService,
    participantId,
    seminaireTitre: "C# Avancé",
    dateDebut: DateTime.Now.AddDays(3)
);

// Notification générique
await NotificationHelper.CreateGenericNotificationAsync(
    _notificationService,
    participantId,
    titre: "Mise à Jour Disponible",
    message: "Consultez la nouvelle version...",
    lien: "/news"
);

// Broadcast à tous les participants
var participantIds = new List<int> { 1, 2, 3, 4, 5 };
await NotificationHelper.CreateBroadcastNotificationAsync(
    _notificationService,
    participantIds,
    titre: "Nouveau Séminaire",
    message: "Un nouveau séminaire est disponible!",
    type: "Séminaire",
    lien: "/Seminaires/Index"
);
```

## 📡 API REST

### Endpoints disponibles

```
GET  /api/notifications/non-lues/{participantId}
     Récupère les notifications non lues

GET  /api/notifications/dernieres/{participantId}?nombre=10
     Récupère les dernières notifications

GET  /api/notifications/count-non-lues/{participantId}
     Compte les notifications non lues

PUT  /api/notifications/{notificationId}/marquer-lue
     Marque une notification comme lue

PUT  /api/notifications/marquer-tous-lues/{participantId}
     Marque toutes les notifications comme lues

DELETE /api/notifications/{notificationId}
       Supprime une notification

DELETE /api/notifications/supprimer-tous/{participantId}
       Supprime toutes les notifications
```

### Exemples de requêtes

```javascript
// Récupérer les notifications non lues
fetch('/api/notifications/non-lues/1')
  .then(r => r.json())
  .then(notifications => console.log(notifications));

// Marquer comme lue
fetch('/api/notifications/5/marquer-lue', {
  method: 'PUT'
})
  .then(r => r.json())
  .then(data => console.log(data));

// Supprimer
fetch('/api/notifications/5', {
  method: 'DELETE'
})
  .then(r => r.json())
  .then(data => console.log(data));
```

## 🎨 Composants UI

### 1. **Dropdown dans la navigation**

Pour ajouter le dropdown des notifications dans votre layout, utilisez le composant partial :

```html
<!-- Dans _Layout.cshtml ou similaire -->
@await Html.PartialAsync("NotificationDropdown", 
    new SeminaPro.Pages.Shared.NotificationDropdownModel 
    { 
        Notifications = Model.RecentNotifications,
        UnreadCount = Model.UnreadCount
    }
)
```

### 2. **Page complète des notifications**

Accessible à : `/Dashboard/Notifications`

Affiche :
- Liste de toutes les notifications
- Indicateur de lecture/non-lu
- Boutons d'action (marquer lue, accéder, supprimer)
- Bouton "Marquer tous comme lus"

### 3. **Types de notifications supportés**

```
- "Inscription"   - Confirmation d'inscription
- "Paiement"      - Confirmation de paiement
- "Séminaire"     - Informations sur les séminaires
- "Facture"       - Factures disponibles
- "Rappel"        - Rappels avant événements
- "General"       - Notifications génériques
```

## 📊 Schéma Base de Données

```sql
CREATE TABLE Notifications (
    Id              INT PRIMARY KEY AUTOINCREMENT,
    ParticipantId   INT NOT NULL,
    Titre           NVARCHAR(100) NOT NULL,
    Message         NVARCHAR(500) NOT NULL,
    DateCreation    DATETIME NOT NULL,
    IsRead          BOOLEAN DEFAULT 0,
    Type            NVARCHAR(50) DEFAULT 'General',
    Lien            NVARCHAR(500),
    FOREIGN KEY (ParticipantId) REFERENCES Participants(Id) 
        ON DELETE CASCADE
);
```

## ⚙️ Configuration dans Program.cs

Le service est déjà enregistré dans `Program.cs` :

```csharp
builder.Services.AddScoped<INotificationService, NotificationService>();
```

## 🔄 Workflow d'exemple

1. Utilisateur remplit formulaire d'inscription
2. Paiement réussi
3. **3 notifications créées automatiquement** :
   - ✅ Inscription Confirmée
   - ✅ Paiement Validé
   - ✅ Facture Disponible
4. Utilisateur voit l'icône 🔔 avec badge "3"
5. En cliquant sur 🔔, le dropdown montre les 3 notifications
6. Utilisateur peut marquer comme lues, supprimer ou accéder au lien

## 🛠️ Maintenance

### Nettoyer les anciennes notifications

```csharp
// Supprimer les notifications de plus de 30 jours
var anciennes = await _context.Notifications
    .Where(n => n.DateCreation < DateTime.UtcNow.AddDays(-30))
    .ToListAsync();

_context.Notifications.RemoveRange(anciennes);
await _context.SaveChangesAsync();
```

### Statistiques

```csharp
// Nombre total de notifications non lues (all users)
var totalUnread = await _context.Notifications
    .CountAsync(n => !n.IsRead);

// Notifications par type
var byType = await _context.Notifications
    .GroupBy(n => n.Type)
    .Select(g => new { Type = g.Key, Count = g.Count() })
    .ToListAsync();
```

## 📝 Notes

- Les notifications sont supprimées en cascade si le participant est supprimé
- Le système supporte les liens personnalisés vers n'importe quelle page
- Les notifications supportent la pagination et le scroll automatique
- L'API peut être utilisée pour créer des intégrations externes

---

**Version**: 1.0  
**Dernière mise à jour**: 2025-01-08
