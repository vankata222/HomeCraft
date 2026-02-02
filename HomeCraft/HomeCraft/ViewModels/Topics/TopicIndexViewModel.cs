using HomeCraft.Data.Models;

namespace HomeCraft.ViewModels.Topics;

public class TopicIndexViewModel
{
    public Topic Topic { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public string ShortDescription => Topic.Description.Length > 100 
        ? Topic.Description.Substring(0, 100) + "..." 
        : Topic.Description;
}