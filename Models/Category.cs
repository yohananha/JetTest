using System.ComponentModel.DataAnnotations;

namespace JetTest.Models;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Dish> Dishes { get; set; } = new List<Dish>();
}
