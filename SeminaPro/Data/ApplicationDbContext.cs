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

        // DBSETS
        public DbSet<Seminaire> Seminaires { get; set; } = null!;
        public DbSet<Participant> Participants { get; set; } = null!;
        public DbSet<Specialite> Specialites { get; set; } = null!;
        public DbSet<Inscription> Inscriptions { get; set; } = null!;
        public DbSet<Universitaire> Universitaires { get; set; } = null!;
        public DbSet<Industriel> Industriels { get; set; } = null!;
        public DbSet<MediaFile> MediaFiles { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public object Admins { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // SEMINAIRE -> SPECIALITE
            // =========================
            modelBuilder.Entity<Seminaire>()
                .HasOne(s => s.Specialite)
                .WithMany(sp => sp.Seminaires)
                .HasForeignKey(s => s.SpecialiteId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // PARTICIPANT -> SPECIALITE
            // =========================
            modelBuilder.Entity<Participant>()
                .HasOne(p => p.Specialite)
                .WithMany(sp => sp.Participants)
                .HasForeignKey(p => p.SpecialiteId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // NOTIFICATION RELATIONS
            // =========================
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Participant)
                .WithMany()
                .HasForeignKey(n => n.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // INSCRIPTION RELATIONS
            // =========================
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

            // =========================
            // TPH (HÉRITAGE)
            // =========================
            modelBuilder.Entity<Participant>()
                .HasDiscriminator<string>("ParticipantType")
                .HasValue<Participant>("Participant")
                .HasValue<Universitaire>("Universitaire")
                .HasValue<Industriel>("Industriel");

            // =========================
            // INDEX UNIQUES
            // =========================
            modelBuilder.Entity<Participant>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<Seminaire>()
                .HasIndex(s => s.Code)
                .IsUnique();

            modelBuilder.Entity<Specialite>()
                .HasIndex(s => s.Libelle)
                .IsUnique();

            // =========================
            // MEDIAFILE RELATIONS
            // =========================
            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.Participant)
                .WithMany()
                .HasForeignKey(m => m.ParticipantId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.Seminaire)
                .WithMany(s => s.MediaFiles)
                .HasForeignKey(m => m.SeminaireId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MediaFile>()
                .HasOne(m => m.Specialite)
                .WithMany()
                .HasForeignKey(m => m.SpecialiteId)
                .OnDelete(DeleteBehavior.SetNull);

            // =========================
            // IGNORE PROPRIÉTÉS NON DB
            // =========================
            modelBuilder.Entity<Specialite>()
                .Ignore(s => s.Abbreviation);

            // ❌ IMPORTANT : SUPPRIMÉ
            // modelBuilder.Entity<Universitaire>().ToTable("Participants");
            // modelBuilder.Entity<Industriel>().ToTable("Participants");
        }
    }
}