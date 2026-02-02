namespace HomeCraft.Data.Models;

public class Vote
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TopicId { get; set; }
    public virtual Topic Topic { get; set; }
    public string UserId { get; set; }
    public bool IsLiked { get; set; } // true = Like, false = Dislike
}