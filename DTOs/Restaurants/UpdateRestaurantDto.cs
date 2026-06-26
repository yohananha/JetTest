using System.ComponentModel.DataAnnotations;

namespace JetTest.DTOs.Restaurants;

public class UpdateRestaurantDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CuisineType { get; set; } = string.Empty;
}
