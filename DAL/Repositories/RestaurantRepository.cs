using JetTest.DAL.Interfaces;
using JetTest.Data;
using JetTest.Models;
using Microsoft.EntityFrameworkCore;

namespace JetTest.DAL.Repositories;

public class RestaurantRepository : Repository<Restaurant>, IRestaurantRepository
{
    public RestaurantRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Restaurant>> GetActiveRestaurantsAsync() =>
        await _context.Restaurants.Where(r => r.IsActive).ToListAsync();
}
