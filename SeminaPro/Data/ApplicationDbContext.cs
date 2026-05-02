using Microsoft.EntityFrameworkCore;
using SeminaPro.Models;

namespace SeminaPro.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Seminaire> Seminaires { get; set; } = null!;
        public DbSet<Participant> Participants { get; set; } = null!;
        public DbSet<Specialite> Specialites { get; set; } = null!;
        public DbSet<Inscription> Inscriptions { get; set; } = null!;
        public DbSet<Universitaire> Universitaires { get; set; } = null!;
        public DbSet<Industriel> Industriels { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la relation Seminaire -> Specialite
            modelBuilder.Entity<Seminaire>()
                .HasOne(s => s.Specialite)
                .WithMany(sp => sp.Seminaires)
                .HasForeignKey(s => s.SpecialiteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration de la relation Participant -> Specialite
            modelBuilder.Entity<Participant>()
                .HasOne(p => p.Specialite)
                .WithMany(sp => sp.Participants)
                .HasForeignKey(p => p.SpecialiteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration de la relation Inscription
            modelBuilder.Entity<Inscription>()
                .HasOne(i => i.Participant)
                .WithMany(p => p.Inscriptions)
                .HasForeignKey(i => i.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inscription>()
                .HasOne(i => i.Seminaire)
                .WithMany(s => s.Inscriptions)
                .HasForeignKey(i => i.SeminaireId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration de l'héritage TPH avec une seule table
            modelBuilder.Entity<Participant>()
                .HasDiscriminator<string>("ParticipantType")
                .HasValue<Participant>("Participant")
                .HasValue<Universitaire>("Universitaire")
                .HasValue<Industriel>("Industriel");

            // Indices
            modelBuilder.Entity<Participant>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<Seminaire>()
                .HasIndex(s => s.Code)
                .IsUnique();

            modelBuilder.Entity<Specialite>()
                .HasIndex(s => s.Libelle)
                .IsUnique();

            // Ignorer la colonne Abbreviation qui n'existe pas dans la BD
            modelBuilder.Entity<Specialite>()
                .Ignore(s => s.Abbreviation);

            // Ignorer les propriétés de navigation non mappées
            modelBuilder.Entity<Universitaire>().ToTable("Participants");
            modelBuilder.Entity<Industriel>().ToTable("Participants");
        }
    }
}
