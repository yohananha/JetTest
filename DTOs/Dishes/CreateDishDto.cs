using System.ComponentModel.DataAnnotations;

namespace JetTest.DTOs.Dishes;

public class CreateDishDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int RestaurantId { get; set; }

    public bool IsAvailable { get; set; } = true;
}
