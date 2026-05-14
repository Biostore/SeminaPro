# 📋 Instructions de Migration - Sécurisation des Mots de Passe

## Étapes pour appliquer les changements

### 1. **Mise à jour de la base de données**

Si vous avez une base de données existante, exécutez la migration:

```powershell
cd C:\Users\Yosra\source\repos\SeminaPro8
dotnet ef database update --project SeminaPro
```

Ou si vous utilisez la Package Manager Console dans Visual Studio:

```powershell
Update-Database
```

### 2. **Créer un nouvel utilisateur de test (RECOMMANDÉ)**

Le moyen le plus sûr est de créer un nouvel utilisateur via la page d'inscription:

1. Arrêtez l'application si elle est en cours d'exécution
2. Lancez l'application
3. Allez sur la page d'inscription (`/Account/Register`)
4. Créez un nouvel utilisateur avec:
   - Email: `testuser@example.com`
   - Mot de passe: `Test@Pass123`
   - Acceptez les conditions
5. L'utilisateur est enregistréavec un mot de passe hachéet sécurisé
6. Connectez-vous pour vérifier que cela fonctionne

### 3. **Optionnel: Hacher les utilisateurs par défaut existants**

Si vous avez une base de données existante avec les utilisateurs par défaut, vous pouvez les mettre à jour.

**ATTENTION**: Cela ne devrait être fait qu'une seule fois!

Modifiez `Program.cs` temporairement:

```csharp
// Dans le bloc de migration de la base de données, après db.Database.Migrate():

try
{
    db.Database.Migrate();

    // Mettre à jour les utilisateurs par défaut avec des mots de passe hachés
    var passwordService = app.Services.GetRequiredService<IPasswordService>();
    PasswordMigrationHelper.MigrateDefaultUserPasswords(db, passwordService);

    Console.WriteLine("Utilisateurs par défaut mis à jour avec des mots de passe hachés");
}
catch (Exception ex)
{
    Console.WriteLine($"Erreur lors de la migration: {ex.Message}");
}
```

Puis exécutez l'application une fois, et retirez ce code.

---

## ✅ Vérification

### Pour vérifier que la migration a fonctionné:

1. **En base de données**:
   - Ouvrez la base de données (`seminapro.db`)
   - Vérifiez que la colonne `PasswordHash` existe dans la table `Participants`
   - Les mots de passe devraient commencer par `$2` (format BCrypt)

2. **Via l'application**:
   - Inscrivez un nouvel utilisateur
   - Vérifiez que la connexion fonctionne
   - Vérifiez que vous ne pouvez pas vous connecter avec un mauvais mot de passe

3. **Exemple de hash BCrypt valide**:
   ```
   $2a$12$R9h/cIPz0gi.URNNX3kh2OPST9/PgBkqquzi.Ss7KIUgO2t0jWMUm
   ```

---

## ⚠️ Rollback (Si nécessaire)

Si vous devez revenir en arrière:

```powershell
# Annuler la dernière migration
dotnet ef database update "20260512220903_AddNotificationSystem" --project SeminaPro
```

---

## 🔧 Dépannage

### "L'outil dotnet-ef n'est pas trouvé"
```powershell
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

### "La base de données est verrouillée"
- Arrêtez l'application
- Supprimez `seminapro.db` si c'est un développement
- Relancez l'application

### "Les changements ne sont pas appliqués"
- Nettoyez la build: `dotnet clean`
- Rebuildez: `dotnet build`
- Mettez à jour la BD: `dotnet ef database update`

---

## 📚 Fichiers modifiés

- `SeminaPro/Models/Participant.cs` - Ajout du champ PasswordHash
- `SeminaPro/Pages/Account/Register.cshtml.cs` - Hachage des mots de passe
- `SeminaPro/Pages/Account/Login.cshtml.cs` - Vérification des mots de passe hachés
- `SeminaPro/Program.cs` - Enregistrement du service IPasswordService
- `SeminaPro/Services/Interfaces/IPasswordService.cs` - Nouvelle interface
- `SeminaPro/Services/Implementations/PasswordService.cs` - Implémentation BCrypt
- `SeminaPro/Migrations/20260520000000_AddPasswordHashToParticipant.cs` - Migration
- `SeminaPro/Migrations/20260520000000_AddPasswordHashToParticipant.Designer.cs` - Designer
- `SeminaPro/Migrations/ApplicationDbContextModelSnapshot.cs` - Snapshot mis à jour
- `SeminaPro/SeminaPro.csproj` - Package BCrypt.Net-Next ajouté

---

**Status**: ✅ Prêt à déployer  
**Version**: 1.0
