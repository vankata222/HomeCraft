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
        
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.TopicId })
                .IsUnique();

            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany() 
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction); 

            builder.Entity<Favorite>()
                .HasOne(f => f.Topic)
                .WithMany(t => t.Favorites)
                .HasForeignKey(f => f.TopicId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Seed Categories
            builder.Entity<Category>().HasData(
                new Category { Id = "cat-001-plumb", Name = "Plumbing", Icon = "bi-droplet-fill" },
                new Category { Id = "cat-002-elect", Name = "Electrical", Icon = "bi-lightning-charge-fill" },
                new Category { Id = "cat-003-carpt", Name = "Carpentry", Icon = "bi-hammer" },
                new Category { Id = "cat-004-paint", Name = "Painting", Icon = "bi-brush" }
            );
        }
    }
}