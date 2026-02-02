using System.ComponentModel.DataAnnotations;

namespace HomeCraft.Data.Models;

public class Favorite
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser? User { get; set; }

    [Required]
    public string TopicId { get; set; } = string.Empty;
    public virtual Topic? Topic { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}