using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HomeCraft.Data;
using HomeCraft.Data.Models; // This ensures it finds Topic and Review in the new folder

namespace HomeCraft.Data
{
    // We inherit from IdentityDbContext to include all the User/Role tables 
    // required for your "Individual Accounts" authentication.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Topic> Topics { get; set; }

        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction); 

            builder.Entity<Review>()
                .HasIndex(r => new { r.TopicId, r.UserId })
                .IsUnique();
        }
    }
}