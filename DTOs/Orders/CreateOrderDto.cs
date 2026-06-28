using System.ComponentModel.DataAnnotations;

namespace JetTest.DTOs.Orders;

public class CreateOrderDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int RestaurantId { get; set; }

    [Required, MaxLength(300)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public List<CreateOrderItemDto> Items { get; set; } = new();
}
