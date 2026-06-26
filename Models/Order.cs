using System.ComponentModel.DataAnnotations;
using JetTest.Models.Enums;

namespace JetTest.Models;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public int? DeliveryDriverId { get; set; }
    public DeliveryDriver? DeliveryDriver { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Placed;

    [Required, MaxLength(300)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
