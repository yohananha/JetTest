using JetTest.BL.Exceptions;
using JetTest.BL.Interfaces;
using JetTest.DAL.Interfaces;
using JetTest.DTOs.Restaurants;
using JetTest.Models;

namespace JetTest.BL.Services;

public class RestaurantService : IRestaurantService
{
    private readonly IRestaurantRepository _repo;
    private readonly ILogger<RestaurantService> _logger;

    public RestaurantService(IRestaurantRepository repo, ILogger<RestaurantService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<RestaurantResponseDto>> GetAllAsync()
    {
        var restaurants = await _repo.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} restaurants", restaurants.Count());
        return restaurants.Select(MapToDto);
    }

    public async Task<RestaurantResponseDto> GetByIdAsync(int id)
    {
        var restaurant = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Restaurant {id} not found");
        return MapToDto(restaurant);
    }

    public async Task<RestaurantResponseDto> CreateAsync(CreateRestaurantDto dto)
    {
        var restaurant = new Restaurant
        {
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            CuisineType = dto.CuisineType
        };

        var created = await _repo.CreateAsync(restaurant);
        _logger.LogInformation("Created restaurant {Id} ({Name})", created.Id, created.Name);
        return MapToDto(created);
    }

    public async Task<RestaurantResponseDto> UpdateAsync(int id, UpdateRestaurantDto dto)
    {
        var restaurant = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Restaurant {id} not found");

        restaurant.Name = dto.Name;
        restaurant.Address = dto.Address;
        restaurant.Phone = dto.Phone;
        restaurant.CuisineType = dto.CuisineType;

        await _repo.UpdateAsync(restaurant);
        _logger.LogInformation("Updated restaurant {Id}", id);
        return MapToDto(restaurant);
    }

    public async Task DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Restaurant {id} not found");

        await _repo.DeleteAsync(id);
        _logger.LogInformation("Deleted restaurant {Id}", id);
    }

    public async Task<RestaurantResponseDto> SetActiveAsync(int id, bool isActive)
    {
        var restaurant = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Restaurant {id} not found");

        restaurant.IsActive = isActive;
        await _repo.UpdateAsync(restaurant);
        _logger.LogInformation("Restaurant {Id} IsActive set to {IsActive}", id, isActive);
        return MapToDto(restaurant);
    }

    private static RestaurantResponseDto MapToDto(Restaurant r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Address = r.Address,
        Phone = r.Phone,
        CuisineType = r.CuisineType,
        IsActive = r.IsActive,
        CreatedAt = r.CreatedAt
    };
}
