using JetTest.Models;

namespace JetTest.DAL.Interfaces;

public interface IDishRepository : IRepository<Dish>
{
    Task<IEnumerable<Dish>> GetByRestaurantAsync(int restaurantId);
    Task<IEnumerable<Dish>> GetByCategoryAsync(int categoryId);
}
