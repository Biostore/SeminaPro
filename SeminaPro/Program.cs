using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;

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

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Créer la base de données et les tables au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

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

        // Ajouter des séminaires de test
        var specialites = db.Specialites.ToList();
        if (specialites.Any())
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
                    SpecialiteId = specialites[0].Id
                },
                new SeminaPro.Models.Seminaire 
                { 
                    Code = "SEM002", 
                    Titre = "Leadership Moderne", 
                    Lieu = "Lyon",
                    Tarif = 399.99m,
                    DateSeminaire = DateTime.Now.AddDays(15),
                    NombreMaximal = 25,
                    SpecialiteId = specialites[1].Id
                },
                new SeminaPro.Models.Seminaire 
                { 
                    Code = "SEM003", 
                    Titre = "Digital Marketing 2025", 
                    Lieu = "Marseille",
                    Tarif = 349.99m,
                    DateSeminaire = DateTime.Now.AddDays(20),
                    NombreMaximal = 40,
                    SpecialiteId = specialites[2].Id
                },
                new SeminaPro.Models.Seminaire 
                { 
                    Code = "SEM004", 
                    Titre = "Gestion des Talents", 
                    Lieu = "Toulouse",
                    Tarif = 279.99m,
                    DateSeminaire = DateTime.Now.AddDays(25),
                    NombreMaximal = 35,
                    SpecialiteId = specialites[3].Id
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
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
