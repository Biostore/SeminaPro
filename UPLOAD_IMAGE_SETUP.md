# 📸 Fonctionnalité Upload d'Image - Guide de Configuration

## ✅ État Actuel
Tous les fichiers nécessaires ont été créés et configurés:

### 📁 Fichiers Existants:
1. ✅ `SeminaPro/Pages/Dashboard/MonProfil.cshtml` - Interface utilisateur avec section upload
2. ✅ `SeminaPro/Pages/Dashboard/MonProfil.cshtml.cs` - Code-behind avec handler `OnPostUploadImage`
3. ✅ `SeminaPro/Pages/Dashboard/MesSeminaires.cshtml` - Page des séminaires
4. ✅ `SeminaPro/Pages/Dashboard/MesSeminaires.cshtml.cs` - Code-behind des séminaires

## 🔧 Étapes pour Activer la Fonctionnalité

### 1. **Vider le Cache du Navigateur**
   - **Chrome/Edge**: 
     - Appuyez sur `Ctrl + Shift + Delete`
     - Sélectionnez "Tout le temps"
     - Cochez "Images et fichiers en cache"
     - Cliquez "Supprimer les données"

   - **Firefox**:
     - Appuyez sur `Ctrl + Shift + Delete`
     - Sélectionnez la plage "Tout"
     - Assurez-vous que "Cache" est coché
     - Cliquez "Effacer maintenant"

### 2. **Arrêter l'Application Visual Studio**
   - Dans Visual Studio, appuyez sur `Shift + F5` ou cliquez sur le bouton "Arrêter le débogage"
   - Attendez que l'application s'arrête complètement

### 3. **Nettoyer et Reconstruire le Projet**
   ```powershell
   # Dans le terminal de Visual Studio ou PowerShell:
   cd C:\Users\Yosra\source\repos\SeminaPro
   dotnet clean
   dotnet build
   ```

### 4. **Redémarrer l'Application**
   - Appuyez sur `F5` pour redémarrer avec débogage
   - Ou `Ctrl + F5` pour redémarrer sans débogage

### 5. **Accéder à la Page Mon Profil**
   - Connectez-vous en tant que participant
   - Cliquez sur "Mon Profil" dans le sidebar
   - Vous devriez voir la section "Modifier votre Photo de Profil"

## 📝 Détails de la Fonctionnalité

### Emplacements des Éléments:
- **Section Upload**: Directement sous le header du profil
- **Aperçu**: À gauche (image actuelle du profil)
- **Formulaire**: À droite (zone de drop ou clic)

### Fonctionnalités:
✅ Prévisualisation en temps réel de l'image sélectionnée
✅ Drag & Drop pour glisser-déposer les images
✅ Clic pour ouvrir le sélecteur de fichier
✅ Validation des formats (JPG, PNG, GIF, WEBP)
✅ Validation de la taille (max 5 MB)
✅ Bouton "Télécharger l'image"
✅ Bouton "Réinitialiser"
✅ Messages de succès/erreur

### Stockage:
- **Dossier**: `/wwwroot/uploads/`
- **Format**: `/uploads/{guid}.{extension}`
- **Base de Données**: `Participant.ImageUrl`

## 🐛 Dépannage

Si vous ne voyez toujours pas la section:

### Option 1: Vérification dans Visual Studio
1. Ouvrez `Pages/Dashboard/MonProfil.cshtml`
2. Recherchez (Ctrl+F) "image-upload-section"
3. Vous devriez trouver plusieurs résultats (styles et HTML)

### Option 2: Forcer un Hard Refresh
- Windows/Linux: `Ctrl + Shift + R`
- Mac: `Cmd + Shift + R`

### Option 3: Vérification de la Compilation
1. Dans Visual Studio, appuyez sur `Ctrl + Shift + B` (Rebuild Solution)
2. Assurez-vous que la compilation a réussi

### Option 4: Vérifier les Fichiers
```powershell
# Vérifier que les fichiers existent:
Get-ChildItem -Path "C:\Users\Yosra\source\repos\SeminaPro\SeminaPro\Pages\Dashboard\MonProfil*"

# Vérifier que la section existe:
Select-String -Path "C:\Users\Yosra\source\repos\SeminaPro\SeminaPro\Pages\Dashboard\MonProfil.cshtml" -Pattern "image-upload-section"
```

## 📦 Architecture

```
MonProfil.cshtml
├── Styles CSS
│   ├── .image-upload-section
│   ├── .upload-area
│   ├── .file-input-label
│   └── .btn-upload*
├── HTML
│   ├── Upload Area
│   │   ├── Preview Image
│   │   └── Upload Form
│   │       ├── File Input
│   │       ├── File Info
│   │       └── Upload Actions
│   └── JavaScript
│       ├── previewImage()
│       ├── resetImageUpload()
│       └── Drag & Drop Handlers

MonProfil.cshtml.cs
├── OnGet() - Charger le profil
├── OnPost() - Mettre à jour les infos
└── OnPostUploadImage() - Traiter l'upload
    ├── Validation du fichier
    ├── Validation du type (MIME)
    ├── Validation de la taille
    ├── Génération du nom unique
    ├── Suppression de l'ancienne image
    ├── Sauvegarde du fichier
    └── Mise à jour de la BD
```

## ✅ Vérification Finale

Après les étapes ci-dessus, vous devriez voir:

1. **Une section "Modifier votre Photo de Profil"** sous le header du profil
2. **Une zone avec l'aperçu** de l'image actuelle à gauche
3. **Une zone de drop** "Cliquez ou glissez votre image ici" à droite
4. **Les boutons** "Télécharger l'image" et "Réinitialiser"
5. **Les informations** sur les formats et la taille acceptés

---

**Si vous avez toujours besoin d'aide**, vérifiez:
- Que l'application est relancée après les modifications
- Que le cache du navigateur a été vidé
- Que la compilation a réussi sans erreurs
