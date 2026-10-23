using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using MauiApp1.Models;

namespace MauiApp1.Data
{
    public class MatiriParishDbContext : DbContext
    {
        public DbSet<Person> Persons { get; set; } = null!;
        public DbSet<BaptismRecord> Baptisms { get; set; } = null!;
        public DbSet<ConfirmationRecord> Confirmations { get; set; } = null!;
        public DbSet<MarriageRecord> Marriages { get; set; } = null!;
        public DbSet<FirstHolyCommunionRecord> FirstHolyCommunions { get; set; } = null!;
        public DbSet<DeathRegistryRecord> DeathRegistries { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        public MatiriParishDbContext()
        {
        }

        public MatiriParishDbContext(DbContextOptions<MatiriParishDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Stores database file in platform-specific app data folder
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "matiri_parish.db");

                // Directly calling the static extension method avoids CS1061 resolution issues in MAUI
                SqliteDbContextOptionsBuilderExtensions.UseSqlite(optionsBuilder, $"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----------------------------------------------------
            // 1. RELATIONSHIPS & FOREIGN KEYS
            // ----------------------------------------------------

            // MarriageRecord -> Person (Groom)
            modelBuilder.Entity<MarriageRecord>()
                .HasOne(m => m.Groom)
                .WithMany()
                .HasForeignKey(m => m.GroomId)
                .OnDelete(DeleteBehavior.Restrict);

            // MarriageRecord -> Person (Bride)
            modelBuilder.Entity<MarriageRecord>()
                .HasOne(m => m.Bride)
                .WithMany()
                .HasForeignKey(m => m.BrideId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------
            // 2. INDEXES & QUERY OPTIMIZATIONS
            // ----------------------------------------------------

            // Fast lookup for duplicate checks
            modelBuilder.Entity<Person>()
                .HasIndex(p => new { p.FirstName, p.LastName, p.DateOfBirth });

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // ----------------------------------------------------
            // 3. SEED DEFAULT ADMIN USER
            // ----------------------------------------------------

            // Pre-seeded Admin account: Username = admin
            // Static deterministic salt & hash so EF Core migrations remain stable
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "admin",
                // Pre-calculated hash for "Admin@123" with fixed salt
                PasswordHash = "J3pQ8Y1L2mN9bV8c7X6z5W4v3U2t1S0r9Q8p7O6n5M4=",
                PasswordSalt = "dGhpcy1pcy1hLXNhbHQ=",
                Role = UserRole.Admin,
                IsMfaEnabled = false
            });

            // ----------------------------------------------------
            // 3. SELF-REFERENCING PARENT RELATIONSHIPS
            // Explicitly configure Father and Mother as two independent
            // self-referencing relationships so EF Core can determine
            // the dependent side (use explicit foreign keys).
            modelBuilder.Entity<Person>()
                .HasOne(p => p.Father)
                .WithMany() // no inverse collection on Person
                .HasForeignKey(p => p.FatherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Mother)
                .WithMany() // no inverse collection on Person
                .HasForeignKey(p => p.MotherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure FirstHolyCommunionRecord -> Person
            modelBuilder.Entity<FirstHolyCommunionRecord>()
                .HasOne(f => f.Person)
                .WithMany()
                .HasForeignKey(f => f.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Optionally configure DeathRegistryRecord -> Person (nullable)
            modelBuilder.Entity<DeathRegistryRecord>()
                .HasOne(d => d.Person)
                .WithMany()
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}