using JetTest.Models.Enums;

namespace JetTest.DTOs.Orders;

public class OrderResponseDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int RestaurantId { get; set; }
    public int? DeliveryDriverId { get; set; }
    public OrderStatus Status { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
