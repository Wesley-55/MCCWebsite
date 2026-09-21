using MCCConstruction.Models;
using Microsoft.EntityFrameworkCore;

namespace MCCConstruction.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<ContactMessage> ContactMessages { get; set; }

        public DbSet<QuoteRequest> QuoteRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ContactMessage>()
                .Property(x => x.Status)
                .HasMaxLength(50);

            modelBuilder.Entity<ContactMessage>()
                .Property(x => x.DateSent)
                .HasColumnType("datetime2");

            modelBuilder.Entity<QuoteRequest>()
                .Property(x => x.Budget)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QuoteRequest>()
                .Property(x => x.Status)
                .HasMaxLength(50);

            modelBuilder.Entity<QuoteRequest>()
                .Property(x => x.DateSubmitted)
                .HasColumnType("datetime2");
        }
    }
}