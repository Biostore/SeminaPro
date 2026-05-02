# Pages d'Administration - Documentation

## Pages créées

Trois nouvelles pages d'administration ont été créées pour gérer les entités principales de la plateforme :

### 1. Gestion des Participants
**URL :** `http://localhost:5093/Admin/Participants`

**Fonctionnalités :**
- ✅ Afficher la liste de tous les participants
- ✅ Ajouter un nouveau participant (Nom, Prénom, Email, Téléphone, Spécialité)
- ✅ Modifier un participant existant
- ✅ Supprimer un participant (supprime aussi les inscriptions associées)
- ✅ Voir le nombre d'inscriptions par participant
- ✅ Voir la spécialité de chaque participant

**Colonnes :**
| Colonne | Description |
|---------|-------------|
| Nom | Nom du participant |
| Prénom | Prénom du participant |
| Email | Email du participant |
| Téléphone | Numéro de téléphone (optionnel) |
| Spécialité | Domaine d'expertise (badge) |
| Inscriptions | Nombre de séminaires auxquels le participant est inscrit |
| Actions | Boutons Modifier/Supprimer |

---

### 2. Gestion des Inscriptions
**URL :** `http://localhost:5093/Admin/Inscriptions`

**Fonctionnalités :**
- ✅ Afficher la liste de toutes les inscriptions
- ✅ Ajouter une nouvelle inscription (Participant + Séminaire)
- ✅ Modifier une inscription existante
- ✅ Supprimer une inscription
- ✅ Gérer l'affichage de confirmation
- ✅ Éviter les doublons (même participant ne peut pas s'inscrire 2x au même séminaire)

**Colonnes :**
| Colonne | Description |
|---------|-------------|
| Participant | Nom et prénom du participant |
| Email | Email du participant |
| Séminaire | Titre du séminaire |
| Date Séminaire | Date du séminaire (badge) |
| Date Inscription | Date et heure de l'inscription |
| Confirmation | Statut d'affichage (Oui/Non) |
| Actions | Boutons Modifier/Supprimer |

---

### 3. Gestion des Spécialités
**URL :** `http://localhost:5093/Admin/Specialites`

**Fonctionnalités :**
- ✅ Afficher la liste de toutes les spécialités
- ✅ Ajouter une nouvelle spécialité (Libellé, Abréviation, Description)
- ✅ Modifier une spécialité existante
- ✅ Supprimer une spécialité (seulement si aucun participant/séminaire associé)
- ✅ Voir le nombre de participants par spécialité
- ✅ Voir le nombre de séminaires par spécialité

**Colonnes :**
| Colonne | Description |
|---------|-------------|
| Libellé | Nom de la spécialité |
| Abréviation | Code court (max 10 caractères) |
| Description | Description courte (max 500 caractères) |
| Participants | Nombre de participants dans cette spécialité |
| Séminaires | Nombre de séminaires pour cette spécialité |
| Actions | Boutons Modifier/Supprimer (désactivé si utilisée) |

---

## Accès aux pages

### Depuis le Dashboard Admin
Les 4 cartes de statistiques en haut du dashboard sont cliquables et vous redirigent vers :
- **Séminaires** → /Admin/Seminaires
- **Participants** → /Admin/Participants
- **Inscriptions** → /Admin/Inscriptions
- **Spécialités** → /Admin/Specialites

### Depuis le menu "Actions Rapides"
Accès rapide à toutes les pages d'administration :
1. Créer Séminaire
2. Séminaires
3. Participants
4. Inscriptions
5. Spécialités
6. Paramètres

---

## Fonctionnalités de sécurité

✅ **Vérification d'authentification**
- Seul un utilisateur connecté avec le rôle "Admin" peut accéder à ces pages
- Les autres utilisateurs sont rejetés avec `UnauthorizedAccessException`

✅ **Validation des données**
- Tous les champs obligatoires sont validés
- Les emails sont vérifiés pour être valides
- Les doublons sont empêchés (spécialité, inscription)

✅ **Cascade de suppression**
- Supprimer un participant supprime automatiquement ses inscriptions
- La suppression d'une spécialité est empêchée si elle est utilisée

✅ **Logging**
- Toutes les opérations CRUD sont enregistrées dans les logs
- Utile pour l'audit et le débogage

---

## Modèles de données

### Participant
```csharp
public class Participant
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Email { get; set; }
    public string? NumeroTelephone { get; set; }
    public int SpecialiteId { get; set; }
    public Specialite? Specialite { get; set; }
    public ICollection<Inscription> Inscriptions { get; set; }
}
```

### Inscription
```csharp
public class Inscription
{
    public int Id { get; set; }
    public DateTime DateInscription { get; set; }
    public bool AffichageConfirmation { get; set; }
    public int ParticipantId { get; set; }
    public Participant? Participant { get; set; }
    public int SeminaireId { get; set; }
    public Seminaire? Seminaire { get; set; }
}
```

### Specialite
```csharp
public class Specialite
{
    public int Id { get; set; }
    public string Libelle { get; set; }
    public string? Abrevaition { get; set; }  // Note: typo dans le modèle original
    public string? Description { get; set; }
    public ICollection<Seminaire> Seminaires { get; set; }
    public ICollection<Participant> Participants { get; set; }
}
```

---

## Comment utiliser

### Ajouter un nouveau participant
1. Aller à /Admin/Participants
2. Remplir le formulaire à gauche
3. Sélectionner une spécialité
4. Cliquer sur "Enregistrer"

### Inscrire un participant à un séminaire
1. Aller à /Admin/Inscriptions
2. Sélectionner le participant dans le dropdown
3. Sélectionner le séminaire dans le dropdown
4. Cliquer sur "Enregistrer"
5. ⚠️ Impossible si le participant est déjà inscrit

### Créer une spécialité
1. Aller à /Admin/Specialites
2. Remplir le formulaire avec:
   - Libellé (obligatoire)
   - Abréviation (optionnel)
   - Description (optionnel)
3. Cliquer sur "Enregistrer"

---

## Erreurs courantes

| Erreur | Cause | Solution |
|--------|-------|----------|
| HTTP 404 | Vous n'êtes pas connecté comme Admin | Vérifiez votre session / Connectez-vous comme Admin |
| "Impossible de supprimer" | La spécialité est utilisée | Supprimez d'abord les participants/séminaires |
| "Ce participant est déjà inscrit" | Doublon d'inscription | Modifiez l'inscription existante au lieu de la dupliquer |

---

## Fichiers créés

- `SeminaPro/Pages/Admin/Participants.cshtml`
- `SeminaPro/Pages/Admin/Participants.cshtml.cs`
- `SeminaPro/Pages/Admin/Inscriptions.cshtml`
- `SeminaPro/Pages/Admin/Inscriptions.cshtml.cs`
- `SeminaPro/Pages/Admin/Specialites.cshtml`
- `SeminaPro/Pages/Admin/Specialites.cshtml.cs`

---

**Dernière mise à jour :** 2025 - .NET 10 / Razor Pages / Entity Framework Core
