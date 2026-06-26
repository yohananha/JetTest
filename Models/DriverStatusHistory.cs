using JetTest.Models.Enums;

namespace JetTest.Models;

public class DriverStatusHistory
{
    public int Id { get; set; }

    public int DriverId { get; set; }
    public DeliveryDriver Driver { get; set; } = null!;

    public DriverStatus OldStatus { get; set; }
    public DriverStatus NewStatus { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
