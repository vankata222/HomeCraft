using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeCraft.Data.Models
{
    public class Topic
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(20)]
        public string Description { get; set; } = string.Empty;

        public string? MediaUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        // The Author of the topic
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // The list of multiple comments
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // The list of unique votes
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}