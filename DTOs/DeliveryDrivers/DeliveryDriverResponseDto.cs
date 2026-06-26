using JetTest.Models.Enums;

namespace JetTest.DTOs.DeliveryDrivers;

public class DeliveryDriverResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public DriverStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
