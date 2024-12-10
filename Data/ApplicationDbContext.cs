//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ExpenseTracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define relationships and foreign keys
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany()  // Assuming one user can have many transactions
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);  // Use Restrict if you don't want cascading delete

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany()  // Assuming one category can have many transactions
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);  // Use Restrict if you don't want cascading delete
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

    }
}
