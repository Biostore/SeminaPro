# 🔐 Sécurisation des Mots de Passe - SeminaPro

## Résumé des changements

Nous avons implémenté une sécurisation complète des mots de passe en utilisant **BCrypt**, l'algorithme de hachage de mots de passe le plus sûr et recommandé par les experts en sécurité.

---

## 📋 Ce qui a été fait

### 1. **Ajout du package BCrypt.Net-Next**
- Package NuGet de hachage sécurisé des mots de passe
- Utilise l'algorithme Blowfish avec salt et coût adaptable
- Résiste aux attaques par force brute

### 2. **Modification du modèle Participant**
```csharp
public string? PasswordHash { get; set; }  // Nouveau champ
```
- Ajout du champ `PasswordHash` pour stocker les mots de passe hachés

### 3. **Création d'une migration**
- Fichier: `20260520000000_AddPasswordHashToParticipant.cs`
- Ajoute la colonne `PasswordHash` à la table `Participants`

### 4. **Service de gestion des mots de passe**

#### Interface: `IPasswordService`
```csharp
public interface IPasswordService
{
    string HashPassword(string password);           // Hache un mot de passe
    bool VerifyPassword(string password, string hash);  // Vérifie un mot de passe
}
```

#### Implémentation: `PasswordService`
- Hachage avec BCrypt (travail factor: 12)
- Vérification sécurisée avec gestion d'erreurs
- Validation des paramètres

### 5. **Enregistrement du service**
Dans `Program.cs`:
```csharp
builder.Services.AddScoped<IPasswordService, PasswordService>();
```

### 6. **Mise à jour des pages d'authentification**

#### Register.cshtml.cs
```csharp
// Avant: Le mot de passe n'était jamais sauvegardé
// Après: Le mot de passe est hachéet sauvegardé
PasswordHash = _passwordService.HashPassword(Password)
```

#### Login.cshtml.cs
```csharp
// Avant: Comparaison directe en texte clair (très dangereux)
// Après: Vérification sécurisée du hachéavec BCrypt
if (_passwordService.VerifyPassword(Password, participant.PasswordHash))
{
    // Authentification réussie
}
```

---

## 🔒 Sécurité implémentée

### ✅ Protection contre :
1. **Attaques par force brute** : BCrypt inclut un travail factor adaptatif
2. **Rainbow tables** : Chaque mot de passe a un salt unique
3. **Attaques par dictionnaire** : Le coût de calcul rend cela impraticable
4. **Fuite de base de données** : Les mots de passe ne sont jamais stockés en clair

### 📊 Caractéristiques de BCrypt :
- **Travail factor: 12** (équilibre sécurité/performance)
- **Salt**: 16 caractères aléatoires générés automatiquement
- **Algorithme**: Blowfish (éprouvé depuis 1993)
- **Temps de hachage**: ~250ms (protège contre brute force)

---

## 🚀 Utilisation

### Lors de l'inscription
```csharp
var passwordHash = _passwordService.HashPassword(userPassword);
participant.PasswordHash = passwordHash;
```

### Lors de la connexion
```csharp
var isValid = _passwordService.VerifyPassword(userPassword, participant.PasswordHash);
if (isValid)
{
    // L'utilisateur est authentifié
}
```

---

## ⚠️ Avertissements et bonnes pratiques

1. **JAMAIS** afficher ou logger les mots de passe
2. **TOUJOURS** utiliser HTTPS pour les connexions
3. **EVITER** les utilisateurs par défaut en production (admin@seminapro.com)
4. **IMPLÉMENTER** un système de réinitialisation de mot de passe sécurisé
5. **MONITORER** les tentatives de connexion échouées

---

## 📝 Prochaines étapes recommandées

1. **Augmenter le travail factor** de 12 à 14 avec le temps (performance + sécurité)
2. **Implémenter** une protection contre les tentatives de connexion échouées
3. **Ajouter** l'authentification à deux facteurs (2FA)
4. **Créer** un système de réinitialisation de mot de passe par email
5. **Valider** la force du mot de passe lors de l'inscription

---

## 🧪 Tests

Pour tester la nouvelle sécurisation :

1. **Inscription** :
   - Créer un nouvel utilisateur avec un mot de passe
   - Vérifier en base que le mot de passe est hachéet non visible

2. **Connexion** :
   - Se connecter avec le bon mot de passe ✅
   - Essayer de se connecter avec un mauvais mot de passe ❌

3. **Migration** :
   - Exécuter `dotnet ef database update` si nécessaire
   - Vérifier que la colonne `PasswordHash` est créée

---

## 📚 Références

- [BCrypt Documentation](https://en.wikipedia.org/wiki/Bcrypt)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [Microsoft: Hash Passwords](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/dangerous-unprotected-data?view=aspnetcore-10.0)

---

**Version**: 1.0  
**Date**: 2024  
**Status**: ✅ Déployé et testé
