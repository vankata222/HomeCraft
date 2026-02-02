using System.ComponentModel.DataAnnotations;

namespace HomeCraft.Data.Models;

public class Category
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}