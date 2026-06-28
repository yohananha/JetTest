namespace JetTest.DTOs.Reports;

public class TopDishDto
{
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int OrderCount { get; set; }
}
