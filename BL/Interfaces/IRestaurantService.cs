using JetTest.DTOs.Restaurants;

namespace JetTest.BL.Interfaces;

public interface IRestaurantService
{
    Task<IEnumerable<RestaurantResponseDto>> GetAllAsync();
    Task<RestaurantResponseDto> GetByIdAsync(int id);
    Task<RestaurantResponseDto> CreateAsync(CreateRestaurantDto dto);
    Task<RestaurantResponseDto> UpdateAsync(int id, UpdateRestaurantDto dto);
    Task DeleteAsync(int id);
    Task<RestaurantResponseDto> SetActiveAsync(int id, bool isActive);
}
