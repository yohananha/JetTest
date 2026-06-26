using System.ComponentModel.DataAnnotations;
using JetTest.Models.Enums;

namespace JetTest.DTOs.Orders;

public class UpdateOrderStatusDto
{
    [Required]
    public OrderStatus Status { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }
}
