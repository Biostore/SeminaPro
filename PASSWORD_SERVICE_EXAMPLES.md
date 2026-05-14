# 💡 Exemples d'utilisation - Service de mots de passe

## Utilisation basique

### Injection de dépendance
```csharp
public class MyPageModel : PageModel
{
    private readonly IPasswordService _passwordService;

    public MyPageModel(IPasswordService passwordService)
    {
        _passwordService = passwordService;
    }
}
```

---

## Cas d'usage 1: Enregistrer un nouvel utilisateur

```csharp
[HttpPost]
public async Task<IActionResult> OnPostRegisterAsync(string email, string password)
{
    // Valider le mot de passe
    if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
    {
        ModelState.AddModelError("", "Le mot de passe doit contenir au moins 6 caractères");
        return Page();
    }

    // Hacher le mot de passe
    string passwordHash = _passwordService.HashPassword(password);

    // Créer l'utilisateur avec le hash
    var user = new Participant
    {
        Email = email,
        PasswordHash = passwordHash,
        // ... autres propriétés
    };

    _context.Participants.Add(user);
    await _context.SaveChangesAsync();

    return RedirectToPage("/Account/Login");
}
```

---

## Cas d'usage 2: Vérifier un mot de passe lors de la connexion

```csharp
[HttpPost]
public IActionResult OnPostLogin(string email, string password)
{
    // Trouver l'utilisateur
    var user = _context.Participants.FirstOrDefault(p => p.Email == email);

    if (user == null)
    {
        ViewData["ErrorMessage"] = "Email ou mot de passe incorrect";
        return Page();
    }

    // Vérifier le mot de passe
    if (!_passwordService.VerifyPassword(password, user.PasswordHash))
    {
        ViewData["ErrorMessage"] = "Email ou mot de passe incorrect";
        return Page();
    }

    // Mot de passe correct - créer la session
    HttpContext.Session.SetString("UserId", user.Id.ToString());
    HttpContext.Session.SetString("UserEmail", user.Email);

    return RedirectToPage("/Dashboard/Index");
}
```

---

## Cas d'usage 3: Changer le mot de passe

```csharp
[HttpPost]
public async Task<IActionResult> OnPostChangePasswordAsync(
    string currentPassword, 
    string newPassword, 
    string confirmPassword)
{
    // Récupérer l'utilisateur connecté
    var userEmail = HttpContext.Session.GetString("UserEmail");
    var user = _context.Participants.FirstOrDefault(p => p.Email == userEmail);

    if (user == null)
        return Unauthorized();

    // Vérifier le mot de passe actuel
    if (!_passwordService.VerifyPassword(currentPassword, user.PasswordHash))
    {
        ModelState.AddModelError("", "Le mot de passe actuel est incorrect");
        return Page();
    }

    // Vérifier que le nouveau mot de passe correspond à la confirmation
    if (newPassword != confirmPassword)
    {
        ModelState.AddModelError("", "Les mots de passe ne correspondent pas");
        return Page();
    }

    // Hacher et stocker le nouveau mot de passe
    user.PasswordHash = _passwordService.HashPassword(newPassword);
    _context.Participants.Update(user);
    await _context.SaveChangesAsync();

    ViewData["SuccessMessage"] = "Mot de passe changé avec succès";
    return Page();
}
```

---

## Cas d'usage 4: Réinitialiser le mot de passe par email (futur)

```csharp
[HttpPost]
public async Task<IActionResult> OnPostResetPasswordAsync(string email)
{
    var user = _context.Participants.FirstOrDefault(p => p.Email == email);

    if (user == null)
    {
        // Ne pas révéler si l'email existe (sécurité)
        ViewData["Message"] = "Si l'email existe, un lien de réinitialisation a été envoyé";
        return Page();
    }

    // Générer un token de réinitialisation (à implémenter)
    string resetToken = GenerateSecureToken();

    // Sauvegarder le token dans une table (à créer)
    var resetRequest = new PasswordResetRequest
    {
        UserId = user.Id,
        Token = _passwordService.HashPassword(resetToken),
        ExpiresAt = DateTime.UtcNow.AddHours(1)
    };

    _context.PasswordResetRequests.Add(resetRequest);
    await _context.SaveChangesAsync();

    // Envoyer l'email avec le lien (à implémenter)
    await SendPasswordResetEmailAsync(user.Email, resetToken);

    ViewData["Message"] = "Un lien de réinitialisation a été envoyé à votre email";
    return Page();
}

[HttpPost]
public async Task<IActionResult> OnPostConfirmResetAsync(string token, string newPassword)
{
    // Trouver la demande de réinitialisation valide
    var resetRequest = _context.PasswordResetRequests
        .Where(r => r.ExpiresAt > DateTime.UtcNow)
        .FirstOrDefault();

    if (resetRequest == null || !_passwordService.VerifyPassword(token, resetRequest.Token))
    {
        ModelState.AddModelError("", "Le lien de réinitialisation est invalide ou expiré");
        return Page();
    }

    // Récupérer l'utilisateur et mettre à jour le mot de passe
    var user = await _context.Participants.FindAsync(resetRequest.UserId);
    user.PasswordHash = _passwordService.HashPassword(newPassword);

    _context.Participants.Update(user);
    _context.PasswordResetRequests.Remove(resetRequest);
    await _context.SaveChangesAsync();

    ViewData["SuccessMessage"] = "Mot de passe réinitialisé avec succès";
    return RedirectToPage("/Account/Login");
}
```

---

## Gestion des erreurs

```csharp
[HttpPost]
public IActionResult OnPostLogin(string email, string password)
{
    try
    {
        var user = _context.Participants.FirstOrDefault(p => p.Email == email);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            _logger.LogWarning($"Tentative de connexion pour email inexistant: {email}");
            ViewData["ErrorMessage"] = "Email ou mot de passe incorrect";
            return Page();
        }

        if (!_passwordService.VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogWarning($"Mot de passe incorrect pour: {email}");
            ViewData["ErrorMessage"] = "Email ou mot de passe incorrect";
            return Page();
        }

        // Succès
        _logger.LogInformation($"Connexion réussie pour: {email}");
        HttpContext.Session.SetString("UserEmail", user.Email);

        return RedirectToPage("/Dashboard/Index");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de la tentative de connexion");
        ViewData["ErrorMessage"] = "Une erreur est survenue. Veuillez réessayer.";
        return Page();
    }
}
```

---

## ⚠️ Erreurs à ÉVITER

### ❌ MAUVAIS: Hacher deux fois
```csharp
// DANGEREUX!
var hash1 = _passwordService.HashPassword(password);
var hash2 = _passwordService.HashPassword(hash1); // NON!
user.PasswordHash = hash2;
```

### ❌ MAUVAIS: Comparer directement
```csharp
// DANGEREUX!
if (password == user.PasswordHash) // NON! Le hash n'est pas le mot de passe
{
    // ...
}
```

### ❌ MAUVAIS: Stocker le mot de passe en clair
```csharp
// DANGEREUX!
user.Password = password; // Jamais faire ça!
user.PasswordHash = _passwordService.HashPassword(password); // BON
```

### ❌ MAUVAIS: Logger les mots de passe
```csharp
// DANGEREUX!
_logger.LogInformation($"Tentative de connexion: {email}, {password}"); // NON!

// BON
_logger.LogInformation($"Tentative de connexion: {email}");
```

---

## ✅ Bonnes pratiques

### 1. Toujours utiliser le service
```csharp
// BON
var hash = _passwordService.HashPassword(password);

// MAUVAIS
var hash = BCrypt.Net.BCrypt.HashPassword(password); // Accès direct
```

### 2. Injecter plutôt que créer une nouvelle instance
```csharp
// BON
public MyModel(IPasswordService passwordService)
{
    _passwordService = passwordService; // Service injecté
}

// MAUVAIS
var service = new PasswordService(); // Nouvelle instance
```

### 3. Valider avant de traiter
```csharp
// BON
if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
{
    ModelState.AddModelError("", "Mot de passe faible");
    return Page();
}
var hash = _passwordService.HashPassword(password);

// MAUVAIS
var hash = _passwordService.HashPassword(password); // Pas de validation
```

### 4. Utiliser des messages d'erreur génériques
```csharp
// BON
ViewData["Error"] = "Email ou mot de passe incorrect";

// MAUVAIS
ViewData["Error"] = "Email n'existe pas"; // Révèle des informations
ViewData["Error"] = "Mot de passe incorrect"; // Révèle que l'email existe
```

---

## 📊 Résumé des méthodes

| Méthode | Paramètre | Retour | Cas d'usage |
|---------|-----------|--------|-----------|
| `HashPassword(string)` | Mot de passe clair | Hash BCrypt | Enregistrement, changement mot de passe |
| `VerifyPassword(string, string)` | Mot de passe + Hash | bool | Vérification login |

---

## 🔗 Fichiers associés

- `Services/Interfaces/IPasswordService.cs` - Interface
- `Services/Implementations/PasswordService.cs` - Implémentation
- `Pages/Account/Register.cshtml.cs` - Exemple enregistrement
- `Pages/Account/Login.cshtml.cs` - Exemple login

---

*Documentation v1.0 - BCrypt Password Security*
