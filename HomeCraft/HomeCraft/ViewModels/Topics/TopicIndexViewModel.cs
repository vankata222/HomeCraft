using HomeCraft.Data.Models;

namespace HomeCraft.ViewModels.Topics;

public class TopicIndexViewModel
{
    public Topic Topic { get; set; } = null!;
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public string ShortDescription { get; set; } = string.Empty;
}