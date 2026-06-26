using System.ComponentModel.DataAnnotations;

namespace JetTest.DTOs.Customers;

public class UpdateCustomerDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;
}
