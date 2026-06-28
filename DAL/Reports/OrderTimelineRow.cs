using JetTest.Models.Enums;

namespace JetTest.DAL.Reports;

public class OrderTimelineRow
{
    public int OrderId { get; set; }
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public DateTime OrderCreatedAt { get; set; }
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
}
