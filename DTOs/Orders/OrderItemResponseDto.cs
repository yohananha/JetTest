namespace JetTest.DTOs.Orders;

public class OrderItemResponseDto
{
    public int DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
