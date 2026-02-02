namespace HomeCraft.Data.Models;

public class Comment 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string TopicId { get; set; }
    public virtual Topic Topic { get; set; }
    public string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
}