using JetTest.DAL.Interfaces;
using JetTest.Data;
using JetTest.Models;
using Microsoft.EntityFrameworkCore;

namespace JetTest.DAL.Repositories;

public class DishRepository : Repository<Dish>, IDishRepository
{
    public DishRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Dish>> GetByRestaurantAsync(int restaurantId) =>
        await _context.Dishes.Where(d => d.RestaurantId == restaurantId).ToListAsync();

    public async Task<IEnumerable<Dish>> GetByCategoryAsync(int categoryId) =>
        await _context.Dishes.Where(d => d.CategoryId == categoryId).ToListAsync();
}
