using JetTest.BL.Exceptions;
using JetTest.BL.Interfaces;
using JetTest.DAL.Interfaces;
using JetTest.DTOs.Categories;
using JetTest.Models;

namespace JetTest.BL.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository repo, ILogger<CategoryService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _repo.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} categories", categories.Count());
        return categories.Select(MapToDto);
    }

    public async Task<CategoryResponseDto> GetByIdAsync(int id)
    {
        var category = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Category {id} not found");
        return MapToDto(category);
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category { Name = dto.Name };

        var created = await _repo.CreateAsync(category);
        _logger.LogInformation("Created category {Id} ({Name})", created.Id, created.Name);
        return MapToDto(created);
    }

    public async Task<CategoryResponseDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Category {id} not found");

        category.Name = dto.Name;

        await _repo.UpdateAsync(category);
        _logger.LogInformation("Updated category {Id}", id);
        return MapToDto(category);
    }

    public async Task DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Category {id} not found");

        await _repo.DeleteAsync(id);
        _logger.LogInformation("Deleted category {Id}", id);
    }

    private static CategoryResponseDto MapToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name
    };
}
