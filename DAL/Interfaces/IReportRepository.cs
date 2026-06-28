using JetTest.DAL.Reports;

namespace JetTest.DAL.Interfaces;

public interface IReportRepository
{
    Task<IEnumerable<DishAggregateRow>> GetDishOrderAggregatesAsync(int? restaurantId);
    Task<IEnumerable<OrderTimelineRow>> GetOrderTimelinesAsync(int? restaurantId);
}
