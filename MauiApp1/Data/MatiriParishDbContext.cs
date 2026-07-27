using Microsoft.EntityFrameworkCore;
using MatiriParish.Registry.Models;

namespace MatiriParish.Registry.Data
{
    public class MatiriParishDbContext : DbContext
    {
        public MatiriParishDbContext(DbContextOptions<MatiriParishDbContext> options) : base(options) { }

        public DbSet<Person> Persons => Set<Person>();
        public DbSet<BaptismRecord> Baptisms => Set<BaptismRecord>();
        public DbSet<ConfirmationRecord> Confirmations => Set<ConfirmationRecord>();
        public DbSet<MarriageRecord> Marriages => Set<MarriageRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Marriage relationships (Groom and Bride)
            modelBuilder.Entity<MarriageRecord>()
                .HasOne(m => m.Groom)
                .WithOne(p => p.MarriageAsGroom)
                .HasForeignKey<MarriageRecord>(m => m.GroomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarriageRecord>()
                .HasOne(m => m.Bride)
                .WithOne(p => p.MarriageAsBride)
                .HasForeignKey<MarriageRecord>(m => m.BrideId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Parent relationships
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
        }
    }
}