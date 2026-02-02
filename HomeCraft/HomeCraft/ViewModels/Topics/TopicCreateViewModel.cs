using System.ComponentModel.DataAnnotations;

namespace HomeCraft.ViewModels.Topics;

public class TopicCreateViewModel
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? MediaUrl { get; set; }

    [Required(ErrorMessage = "Please select a category")]
    public string CategoryId { get; set; } = string.Empty;
}