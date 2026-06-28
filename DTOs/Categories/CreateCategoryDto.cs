using System.ComponentModel.DataAnnotations;

namespace JetTest.DTOs.Categories;

public class CreateCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
