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


    // Database tables
    public DbSet<Project> Projects { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<QuoteRequest> QuoteRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Quote request budget precision
            modelBuilder.Entity<QuoteRequest>()
                .Property(q => q.Budget)
                .HasPrecision(18, 2);

            // Store expected start date as a PostgreSQL DATE
            modelBuilder.Entity<QuoteRequest>()
                .Property(q => q.ExpectedStartDate)
                .HasColumnType("date");

            // PostgreSQL timestamp defaults
            modelBuilder.Entity<Project>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<ContactMessage>()
                .Property(c => c.DateSent)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<QuoteRequest>()
                .Property(q => q.DateSubmitted)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}



