namespace HomeCraft.ViewModels.Topics;

public class TopicEditViewModel
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string? MediaUrl { get; set; }
    public string CategoryId { get; set; } 
    public string UserId { get; set; } 
    public DateTime CreatedAt { get; set; }
}