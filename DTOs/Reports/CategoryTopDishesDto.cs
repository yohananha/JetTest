namespace JetTest.DTOs.Reports;

public class CategoryTopDishesDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<TopDishDto> Dishes { get; set; } = new();
}
