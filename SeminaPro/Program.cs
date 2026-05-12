using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Services.Interfaces;
using SeminaPro.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Configuration du logging avec ID de corrélation
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configuration de la chaîne de connexion
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=seminapro.db";

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.Name = ".SeminaPro.Session";
    options.Cookie.HttpOnly = true;
});

// ==========================================
// ENREGISTRER LES SERVICES PERSONNALISÉS
// ==========================================
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Créer la base de données et les tables au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Appliquer les migrations en attente
    try
    {
        // En development : supprimer et recréer si les migrations échouent
        var isDevelopment = app.Environment.IsDevelopment();

        try
        {
            db.Database.Migrate();
            Console.WriteLine("Migrations appliquées avec succès");
        }
        catch (Exception migrationEx)
        {
            if (isDevelopment)
            {
                Console.WriteLine($"Erreur lors de l'application des migrations: {migrationEx.Message}");
                Console.WriteLine("En mode développement: suppression et recréation de la base de données...");

                try
                {
                    db.Database.EnsureDeleted();
                    db.Database.Migrate();
                    Console.WriteLine("Base de données recréée et migrations appliquées avec succès");
                }
                catch (Exception recreateEx)
                {
                    Console.WriteLine($"Erreur lors de la recréation de la base: {recreateEx.Message}");
                    throw;
                }
            }
            else
            {
                throw;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur critique lors de la migration de la base de données: {ex.Message}");
    }

    // Ajouter des données de test si la BD est vide
    if (!db.Specialites.Any())
    {
        // Ajouter les spécialités
            db.Specialites.AddRange(
                    new SeminaPro.Models.Specialite { Libelle = "Informatique", Abbreviation = "INFO", Description = "Spécialité en informatique et développement" },
                    new SeminaPro.Models.Specialite { Libelle = "Gestion", Abbreviation = "GEST", Description = "Spécialité en gestion et management" },
                    new SeminaPro.Models.Specialite { Libelle = "Marketing", Abbreviation = "MKT", Description = "Spécialité en marketing et communication" },
                    new SeminaPro.Models.Specialite { Libelle = "Ressources Humaines", Abbreviation = "RH", Description = "Spécialité en ressources humaines" }
                );
        db.SaveChanges();

        // Ajouter des participants de test
        var specialites = db.Specialites.ToList();
        if (specialites.Any())
        {
            db.Participants.AddRange(
                new SeminaPro.Models.Universitaire
                {
                    Nom = "Dupont",
                    Prenom = "Jean",
                    Email = "jean.dupont@test.com",
                    NumeroTelephone = "+33612345678",
                    SpecialiteId = specialites[0].Id,
                    Niveau = "Master 2",
                    NomUniversite = "Université Paris-Saclay"
                },
                new SeminaPro.Models.Industriel
                {
                    Nom = "Martin",
                    Prenom = "Sophie",
                    Email = "sophie.martin@test.com",
                    NumeroTelephone = "+33687654321",
                    SpecialiteId = specialites[1].Id,
                    Fonction = "Manager",
                    NomEntreprise = "TechCorp France"
                },
                new SeminaPro.Models.Participant
                {
                    Nom = "Bernard",
                    Prenom = "Marie",
                    Email = "marie.bernard@test.com",
                    NumeroTelephone = "+33698765432",
                    SpecialiteId = specialites[2].Id
                }
            );
            db.SaveChanges();
        }

        // Ajouter des séminaires de test
        var specialitesList = db.Specialites.ToList();
        if (specialitesList.Any())
        {
            db.Seminaires.AddRange(
                new SeminaPro.Models.Seminaire 
                { 
                    Code = "SEM001", 
                    Titre = "Introduction à .NET 10", 
                    Lieu = "Paris",
                    Tarif = 299.99m,
                    DateSeminaire = DateTime.Now.AddDays(10),
                    NombreMaximal = 30,
                    SpecialiteId = specialitesList[0].Id
                },
                new SeminaPro.Models.Seminaire 
                { 
                    Code = "SEM002", 
                    Titre = "Leadership Moderne", 
                    Lieu = "Lyon",
                    Tarif = 399.99m,
                    DateSeminaire = DateTime.Now.AddDays(15),
                    NombreMaximal = 25,
                    SpecialiteId = specialitesList[1].Id
                },
                new SeminaPro.Models.Seminaire 
                { 
                    Code = "SEM003", 
                    Titre = "Digital Marketing 2025", 
                    Lieu = "Marseille",
                    Tarif = 349.99m,
                    DateSeminaire = DateTime.Now.AddDays(20),
                    NombreMaximal = 40,
                    SpecialiteId = specialitesList[2].Id
                },
                new SeminaPro.Models.Seminaire 
                { 
                    Code = "SEM004", 
                    Titre = "Gestion des Talents", 
                    Lieu = "Toulouse",
                    Tarif = 279.99m,
                    DateSeminaire = DateTime.Now.AddDays(25),
                    NombreMaximal = 35,
                    SpecialiteId = specialitesList[3].Id
                }
            );
            db.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

// Ajout du middleware CorrelationId
app.UseMiddleware<SeminaPro.Middleware.CorrelationIdMiddleware>();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
