using System.ComponentModel.DataAnnotations;

namespace JetTest.DTOs.Orders;

public class UpdateOrderDto
{
    public int? DeliveryDriverId { get; set; }

    [MaxLength(300)]
    public string? DeliveryAddress { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
