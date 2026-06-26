using System.ComponentModel.DataAnnotations;
using JetTest.Models.Enums;

namespace JetTest.Models;

public class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(300)]
    public string? Notes { get; set; }
}
