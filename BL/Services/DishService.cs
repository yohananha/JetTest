using JetTest.BL.Exceptions;
using JetTest.BL.Interfaces;
using JetTest.DAL.Interfaces;
using JetTest.DTOs.Dishes;
using JetTest.Models;

namespace JetTest.BL.Services;

public class DishService : IDishService
{
    private readonly IDishRepository _repo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IRestaurantRepository _restaurantRepo;
    private readonly ILogger<DishService> _logger;

    public DishService(
        IDishRepository repo,
        ICategoryRepository categoryRepo,
        IRestaurantRepository restaurantRepo,
        ILogger<DishService> logger)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
        _restaurantRepo = restaurantRepo;
        _logger = logger;
    }

    public async Task<IEnumerable<DishResponseDto>> GetAllAsync()
    {
        var dishes = await _repo.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} dishes", dishes.Count());
        return dishes.Select(MapToDto);
    }

    public async Task<DishResponseDto> GetByIdAsync(int id)
    {
        var dish = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Dish {id} not found");
        return MapToDto(dish);
    }

    public async Task<DishResponseDto> CreateAsync(CreateDishDto dto)
    {
        _ = await _categoryRepo.GetByIdAsync(dto.CategoryId)
            ?? throw new NotFoundException($"Category {dto.CategoryId} not found");

        _ = await _restaurantRepo.GetByIdAsync(dto.RestaurantId)
            ?? throw new NotFoundException($"Restaurant {dto.RestaurantId} not found");

        var dish = new Dish
        {
            Name = dto.Name,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            RestaurantId = dto.RestaurantId,
            IsAvailable = dto.IsAvailable
        };

        var created = await _repo.CreateAsync(dish);
        _logger.LogInformation("Created dish {Id} ({Name})", created.Id, created.Name);
        return MapToDto(created);
    }

    public async Task<DishResponseDto> UpdateAsync(int id, UpdateDishDto dto)
    {
        var dish = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Dish {id} not found");

        _ = await _categoryRepo.GetByIdAsync(dto.CategoryId)
            ?? throw new NotFoundException($"Category {dto.CategoryId} not found");

        dish.Name = dto.Name;
        dish.Price = dto.Price;
        dish.CategoryId = dto.CategoryId;
        dish.IsAvailable = dto.IsAvailable;

        await _repo.UpdateAsync(dish);
        _logger.LogInformation("Updated dish {Id}", id);
        return MapToDto(dish);
    }

    public async Task DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Dish {id} not found");

        await _repo.DeleteAsync(id);
        _logger.LogInformation("Deleted dish {Id}", id);
    }

    private static DishResponseDto MapToDto(Dish d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Price = d.Price,
        CategoryId = d.CategoryId,
        RestaurantId = d.RestaurantId,
        IsAvailable = d.IsAvailable
    };
}
