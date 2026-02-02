using HomeCraft.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeCraft.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Topic> Topics { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Vote> Votes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // This ensures one user can only have ONE vote per topic.
            builder.Entity<Vote>()
                .HasIndex(v => new { v.TopicId, v.UserId })
                .IsUnique();

            // A Topic has many Comments
            builder.Entity<Comment>()
                .HasOne(c => c.Topic)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TopicId)
                .OnDelete(DeleteBehavior.Cascade); // Delete comments if topic is deleted

            // A Topic has many Votes
            builder.Entity<Vote>()
                .HasOne(v => v.Topic)
                .WithMany(t => t.Votes)
                .HasForeignKey(v => v.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany() 
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent multiple cascade paths
        }
    }
}