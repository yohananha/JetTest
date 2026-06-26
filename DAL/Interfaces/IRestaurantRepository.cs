using JetTest.Models;

namespace JetTest.DAL.Interfaces;

public interface IRestaurantRepository : IRepository<Restaurant>
{
    Task<IEnumerable<Restaurant>> GetActiveRestaurantsAsync();
}
