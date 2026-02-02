using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HomeCraft.Data.Models
{
    public class Topic
    {
        public Topic()
        {
            this.Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        // Multimedia requirement: Can store a URL to an image or video
        public string? MediaUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Link to the User who created the topic
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // Link to the Reviews (Like/Dislike)
        // count likes
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}