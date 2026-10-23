using Microsoft.EntityFrameworkCore;
using MatiriParish.Core.Models;

namespace MatiriParish.Core.Data
{
    public class MatiriParishDbContext : DbContext
    {
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
            // Intentionally left blank - callers should configure provider via options.
            // This keeps the core library platform-agnostic.
        }
    }
}
