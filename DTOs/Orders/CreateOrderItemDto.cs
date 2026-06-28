using System.ComponentModel.DataAnnotations;

namespace JetTest.DTOs.Orders;

public class CreateOrderItemDto
{
    [Required]
    public int DishId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
