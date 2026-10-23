using Microsoft.EntityFrameworkCore;
using MatiriParish.Api.Models;

namespace MatiriParish.Api.Data
{
    public class MatiriParishDbContext : DbContext
    {
        public MatiriParishDbContext(DbContextOptions<MatiriParishDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; } = null!;
        public DbSet<BaptismRecord> Baptisms { get; set; } = null!;
        public DbSet<ConfirmationRecord> Confirmations { get; set; } = null!;
        public DbSet<MarriageRecord> Marriages { get; set; } = null!;
        public DbSet<FirstHolyCommunionRecord> FirstHolyCommunions { get; set; } = null!;
        public DbSet<DeathRegistryRecord> DeathRegistries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MarriageRecord>()
                .HasOne(m => m.Groom)
                .WithMany()
                .HasForeignKey(m => m.GroomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarriageRecord>()
                .HasOne(m => m.Bride)
                .WithMany()
                .HasForeignKey(m => m.BrideId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Person>()
                .HasIndex(p => new { p.FirstName, p.LastName, p.DateOfBirth });

            // Add indexes to speed up lookups by foreign keys used in searches
            modelBuilder.Entity<BaptismRecord>()
                .HasIndex(b => b.PersonId);

            modelBuilder.Entity<ConfirmationRecord>()
                .HasIndex(c => c.PersonId);

            modelBuilder.Entity<MarriageRecord>()
                .HasIndex(m => m.GroomId);

            modelBuilder.Entity<MarriageRecord>()
                .HasIndex(m => m.BrideId);

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Father)
                .WithMany()
                .HasForeignKey(p => p.FatherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Mother)
                .WithMany()
                .HasForeignKey(p => p.MotherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FirstHolyCommunionRecord>()
                .HasOne(f => f.Person)
                .WithMany()
                .HasForeignKey(f => f.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeathRegistryRecord>()
                .HasOne(d => d.Person)
                .WithMany()
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
