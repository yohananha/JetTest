using JetTest.DTOs.Dishes;

namespace JetTest.BL.Interfaces;

public interface IDishService
{
    Task<IEnumerable<DishResponseDto>> GetAllAsync();
    Task<DishResponseDto> GetByIdAsync(int id);
    Task<DishResponseDto> CreateAsync(CreateDishDto dto);
    Task<DishResponseDto> UpdateAsync(int id, UpdateDishDto dto);
    Task DeleteAsync(int id);
}
