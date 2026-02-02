using System.ComponentModel.DataAnnotations;

namespace HomeCraft.ViewModels.Topics;

public class TopicCreateViewModel
{
    [Required(ErrorMessage = "Please enter a title.")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    public string? MediaUrl { get; set; }
}