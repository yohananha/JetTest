using System.ComponentModel.DataAnnotations;
using JetTest.Models.Enums;

namespace JetTest.Models;

public class DeliveryDriver
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(50)]
    public string VehicleType { get; set; } = string.Empty;

    public DriverStatus Status { get; set; } = DriverStatus.Offline;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<DriverStatusHistory> StatusHistory { get; set; } = new List<DriverStatusHistory>();
}
