namespace JetTest.DTOs.Dishes;

public class DishResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int RestaurantId { get; set; }
    public bool IsAvailable { get; set; }
}
