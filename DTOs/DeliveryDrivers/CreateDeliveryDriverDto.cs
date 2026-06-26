using System.ComponentModel.DataAnnotations;

namespace JetTest.DTOs.DeliveryDrivers;

public class CreateDeliveryDriverDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(50)]
    public string VehicleType { get; set; } = string.Empty;
}
