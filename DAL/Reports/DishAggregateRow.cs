namespace JetTest.DAL.Reports;

public class DishAggregateRow
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int OrderCount { get; set; }
}
