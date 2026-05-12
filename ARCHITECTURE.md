# 🏗️ Architecture SeminaPro - Guide d'Implémentation

## 📋 Table des Matières
1. [Patrons de Conception](#patrons-de-conception)
2. [Structure des Modèles](#structure-des-modèles)
3. [Services et Interfaces](#services-et-interfaces)
4. [Gestion des Images](#gestion-des-images)
5. [Filtrage par Dates](#filtrage-par-dates)
6. [Gestion des Rôles](#gestion-des-rôles)
7. [Prochaines Étapes](#prochaines-étapes)

---

## 🎯 Patrons de Conception Implémentés

### 1. **Repository Pattern** ✅
**Où**: `ApplicationDbContext` avec EF Core DbSet
**Utilité**: Abstraction de la couche données

```csharp
// ✅ Automatique via EF Core
var seminaires = context.Seminaires.ToList();
```

### 2. **Dependency Injection (DI)** ✅
**Où**: `Program.cs` - Services enregistrés dans le conteneur IoC
**Utilité**: Injection dans les PageModels et Services

```csharp
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<ISeminaireFilterService, SeminaireFilterService>();
```

### 3. **Service Layer Pattern** ✅ (NOUVEAU)
**Où**: `Services/Interfaces/` et `Services/Implementations/`
**Utilité**: Encapsuler la logique métier

```csharp
// Utilisation
public class MyPage : PageModel
{
    public MyPage(ISeminaireFilterService filterService)
    {
        _filterService = filterService;
    }

    public void OnGet()
    {
        var filtered = _filterService.GetAvailableForInscription(seminaires);
    }
}
```

### 4. **Validator Pattern** ✅ (NOUVEAU)
**Où**: `Services/Validators/SeminaireValidator.cs`
**Utilité**: Centraliser les règles métier

```csharp
var (isValid, message) = SeminaireValidator.CanInscribe(seminaire);
if (!isValid) return BadRequest(message);
```

### 5. **Enum-based State Management** ✅ (NOUVEAU)
**Où**: `Models/Enums/`
**Utilité**: Gérer les statuts avec typage fort

```csharp
public enum SeminaireStatut { Brouillon, Publie, EnCours, ... }
public enum UserType { Admin, Organisateur, Participant, ... }
```

### 6. **Entity Hierarchy Pattern (TPH)** ✅
**Où**: `Participant` → `Universitaire` & `Industriel`
**Utilité**: Héritage sans table supplémentaire

---

## 📊 Structure des Modèles

### Hiérarchie des Entités

```
📦 Models/
├── Enums/
│   ├── SeminaireStatut.cs      ✅ NOUVEAU
│   ├── UserType.cs             ✅ NOUVEAU
│   ├── RoleType.cs             ✅ NOUVEAU
│   └── MediaType.cs            ✅ NOUVEAU
│
├── Participant.cs              (+ ImageUrl)
├── Universitaire.cs            (hérite de Participant)
├── Industriel.cs               (hérite de Participant)
├── Seminaire.cs                (+ dates, conditions, statuts)
├── Specialite.cs               (+ ImageUrl)
├── Inscription.cs              ✅
├── MediaFile.cs                ✅ NOUVEAU
└── ErrorViewModel.cs
```

### Relations de Base de Données

```
                    ┌─────────────────┐
                    │   Specialite    │
                    │  + ImageUrl ✅  │
                    └────────┬────────┘
                             │
            ┌────────────────┼────────────────┐
            │                │                │
         1:N              1:N              1:N
            │                │                │
    ┌───────▼──────┐  ┌──────▼──────┐  ┌─────▼────────┐
    │  Seminaire   │  │ Participant │  │  MediaFile   │
    │ + Dates ✅   │  │+ ImageUrl   │  │   ✅ NEW    │
    │ + Statut ✅  │  │             │  │              │
    └───────┬──────┘  └──────┬──────┘  └──────────────┘
            │                │
            │             1:N│
            │                │
            │    ┌───────────┴───────────┐
            │    │   Universitaire       │
            │    │   Industriel          │
            │    │  (TPH Inheritance)    │
            │    └───────────┬───────────┘
            │                │
            └────────┬───────┘
                     │
                  N:N│
                     │
            ┌────────▼────────┐
            │  Inscription    │
            │   ✅ EXISTS     │
            └─────────────────┘
```

### Propriétés Calculées (NotMapped)

```csharp
// Seminaire
public int NombreInscrits => Inscriptions?.Count ?? 0;
public bool EstComplet => NombreInscrits >= NombreMaximal;
public bool EstDisponible => /* conditions */;
public decimal TauxRemplissage => NombreInscrits / NombreMaximal * 100;
public bool EstEnCours => DateTime.UtcNow >= DateDebut && ...;
public bool EstTermine => DateTime.UtcNow > DateFin;
public int JoursRestantsInscription => (DateInscriptionFermeture - DateTime.UtcNow).TotalDays;
public string StatusBadge => /* emoji badge */;

// MediaFile
public string FileExtension => Path.GetExtension(FileName).ToLower();
public bool IsImage => /* vérifie extension */;
public bool IsDocument => /* vérifie extension */;
public string FileSizeFormatted => /* B, KB, MB, GB */;
```

---

## 🔧 Services et Interfaces

### IFileUploadService

**Responsabilité**: Gérer l'upload et la suppression d'images

```csharp
// Utilisation
var (success, url, message) = await fileUploadService
    .UploadImageAsync(file, "seminaires", MediaType.SeminaireImage);

if (success)
    seminaire.ImageUrl = url;
```

**Fonctionnalités**:
- ✅ Upload avec validation MIME type
- ✅ Vérification des octets magiques (magic bytes)
- ✅ Gestion de la taille maximale (5 MB)
- ✅ Extensions autorisées: JPG, PNG, GIF, WebP
- ✅ Noms uniques avec GUID
- ✅ Suppression sécurisée

### ISeminaireFilterService

**Responsabilité**: Filtrer les séminaires par critères (dates, statuts, etc.)

```csharp
// Utilisation simple
var disponibles = filterService.GetAvailableForInscription(seminaires);
var enCours = filterService.GetOngoing(seminaires);
var aVenir = filterService.GetUpcoming(seminaires, days: 30);

// Utilisation avancée
var criteria = new SeminaireFilterCriteria
{
    DateDebut = DateTime.Now,
    DateFin = DateTime.Now.AddMonths(3),
    Statut = SeminaireStatut.Publie,
    SpecialiteId = 1,
    TarifMin = 100,
    TarifMax = 500,
    SearchTerm = "Web"
};

var results = filterService.FilterAdvanced(seminaires, criteria);
```

---

## 🖼️ Gestion des Images

### Structure de Dossiers

```
wwwroot/
├── uploads/
│   ├── profiles/          → Images de profil participant
│   ├── seminaires/        → Images de séminaires
│   ├── specialties/       → Logos de spécialités
│   └── documents/         → Documents de séminaires
└── images/
    ├── default-profile.png
    ├── default-seminaire.png
    └── default-speciality.png
```

### Types de Médias

```csharp
public enum MediaType
{
    ProfileImage = 0,           // Image de profil utilisateur
    SeminaireImage = 1,         // Image du séminaire
    SeminaireDocument = 2,      // Document de séminaire (PDF, Word, etc.)
    Certificate = 3,           // Certificat d'attendance
    SpecialityImage = 4,        // Image de spécialité (logo/icône)
    Other = 5                   // Autre document
}
```

### Validation des Images

```
✅ Formats: JPG, PNG, GIF, WebP
✅ Taille max: 5 MB
✅ Vérification MIME type
✅ Vérification octets magiques (magic bytes)
✅ Noms uniques avec GUID
```

**Exemple de validation des octets magiques**:
```
JPG:  FF D8 FF
PNG:  89 50 4E 47
GIF:  47 49 46
WEBP: 52 49 46 46 ... 57 45 42 50
```

---

## 📅 Filtrage par Dates

### Dates Importantes dans Seminaire

```csharp
// Dates du séminaire
public DateTime DateDebut { get; set; }        // Début du séminaire
public DateTime DateFin { get; set; }          // Fin du séminaire

// Dates d'inscription
public DateTime DateInscriptionOuverture { get; set; }  // Quand les inscriptions ouvrent
public DateTime DateInscriptionFermeture { get; set; }  // Quand les inscriptions ferment

// Audit
public DateTime DateCreation { get; set; }    // Quand le séminaire a été créé
```

### Cas d'Usage - Filtrage dans l'Admin

```csharp
// Page Admin - Afficher tous les séminaires de janvier
var seminaireJanvier = filterService.FilterByDateRange(
    seminaires,
    startDate: new DateTime(2025, 1, 1),
    endDate: new DateTime(2025, 1, 31)
);

// Afficher les séminaires dont les inscriptions ouvrent dans les 7 jours
var openingSoon = seminaires.Where(s =>
    s.DateInscriptionOuverture <= DateTime.UtcNow.AddDays(7)
);

// Afficher les séminaires dont les inscriptions sont fermées
var closedInscriptions = filterService.GetWithClosedInscription(seminaires);

// Afficher les séminaires en cours maintenant
var ongoingSeminaires = filterService.GetOngoing(seminaires);

// Afficher les séminaires à venir dans les 30 jours
var upcoming = filterService.GetUpcoming(seminaires, days: 30);
```

---

## 👥 Gestion des Rôles

### Types d'Utilisateurs (UserType)

```csharp
public enum UserType
{
    Admin = 0,          // Accès complet au système
    Organisateur = 1,   // Crée et gère les séminaires
    Participant = 2,    // S'inscrit aux séminaires
    Moderateur = 3,     // Modère les contenus et utilisateurs
    Invite = 4          // Accès en lecture seule
}
```

### Rôles RBAC (RoleType)

```csharp
public enum RoleType
{
    Admin = 0,          // Gestion complète
    Organisateur = 1,   // Création/édition séminaires
    Participant = 2,    // Inscription séminaires
    Moderateur = 3,     // Modération contenus
    Invite = 4          // Lecture seule
}
```

### Implémentation Technique (À FAIRE - Phase 2)

#### Approche Session Actuelle (Temporaire)

```csharp
// Dans Login.cshtml.cs
if (user is Admin)
    HttpContext.Session.SetString("UserRole", "Admin");
else if (user is Organisateur)
    HttpContext.Session.SetString("UserRole", "Organisateur");
```

#### Approche Recommandée (ASP.NET Core Identity - Phase 2)

```csharp
// ✅ À implémenter prochainement
public class ApplicationUser : IdentityUser
{
    public UserType UserType { get; set; }
    public ICollection<IdentityUserRole<string>> UserRoles { get; set; }
}

// Dans PageModel
[Authorize(Roles = "Admin")]
public class AdminPage : PageModel { }

// Vérification dans le code
if (User.IsInRole("Admin")) { }
if (User.HasClaim("Permission", "Edit")) { }
```

---

## 🎓 Validation Métier

### Exemple: Validation d'Inscription

```csharp
var seminaire = db.Seminaires.Find(id);
var (canInscribe, message) = SeminaireValidator.CanInscribe(seminaire);

if (!canInscribe)
{
    ModelState.AddModelError("", message);
    return Page();
}

// Continuer avec l'inscription...
```

### Exemple: Validation de Publication

```csharp
var (canPublish, message) = SeminaireValidator.CanPublish(seminaire);

if (!canPublish)
{
    return BadRequest(message);
}

seminaire.Statut = SeminaireStatut.Publie;
db.SaveChanges();
```

---

## 🔜 Prochaines Étapes - Phase 2

### 🔴 CRITIQUE (Semaine 3-4)

- [ ] Implémenter **ASP.NET Core Identity**
- [ ] Ajouter **Policy-based Authorization**
- [ ] Créer des **DTOs avec AutoMapper**
- [ ] Implémenter le **Specification Pattern**

### 🟡 IMPORTANTE (Semaine 5-6)

- [ ] Ajouter **Unit of Work Pattern**
- [ ] Créer une **API REST** (optionnel)
- [ ] Implémenter le **Caching** (Redis)
- [ ] Ajouter des **tests unitaires**

### 🟢 SOUHAITABLE (Semaine 7+)

- [ ] Implémenter le **CQRS**
- [ ] Ajouter des **Event Sourcing**
- [ ] Créer des **microservices**
- [ ] Implémenter un **API Gateway**

---

## 📝 Checklist d'Implémentation

### ✅ Phase 1 - COMPLÉTÉE

- [x] Créer les Enums (SeminaireStatut, UserType, RoleType, MediaType)
- [x] Améliorer le modèle Seminaire (dates, conditions, statuts)
- [x] Ajouter ImageUrl à Specialite
- [x] Créer le modèle MediaFile
- [x] Implémenter SeminaireValidator
- [x] Créer IFileUploadService & FileUploadService
- [x] Créer ISeminaireFilterService & SeminaireFilterService
- [x] Enregistrer les services dans Program.cs
- [x] Mettre à jour DbContext avec MediaFile

### 🔜 Phase 2 - À FAIRE

- [ ] Créer des migrations EF Core pour les modifications
- [ ] Implémenter les DTOs
- [ ] Ajouter AutoMapper
- [ ] Implémenter ASP.NET Core Identity
- [ ] Créer des policies d'autorisation
- [ ] Tester les validateurs
- [ ] Tester le service de filtrage
- [ ] Intégrer le service d'upload dans les pages

---

## 📚 Ressources et Liens

- [Design Patterns in C#](https://refactoring.guru/design-patterns)
- [ASP.NET Core Identity](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

---

**Dernière mise à jour**: 2025-01-15
**Statut**: Architecture Phase 1 ✅ Complétée
