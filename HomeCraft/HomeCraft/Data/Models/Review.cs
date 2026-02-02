using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HomeCraft.Data.Models;

namespace HomeCraft.Data
{
    public class Review
    {
        public Review()
        {
            this.Id = Guid.NewGuid().ToString();
        }
        [Key]
        public string Id { get; set; }

        [Required]
        public bool IsLiked { get; set; } // True = Like, False = Dislike

        // Foreign Key for Topic
        [Required]
        public string TopicId { get; set; }
        
        [ForeignKey("TopicId")]
        public virtual Topic? Topic { get; set; }

        // Foreign Key for User
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}