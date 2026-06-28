using JetTest.DAL.Interfaces;
using JetTest.DAL.Reports;
using JetTest.Data;
using Microsoft.EntityFrameworkCore;

namespace JetTest.DAL.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DishAggregateRow>> GetDishOrderAggregatesAsync(int? restaurantId)
    {
        var query = _context.OrderItems.AsQueryable();

        if (restaurantId.HasValue)
            query = query.Where(oi => oi.Dish.RestaurantId == restaurantId.Value);

        // Grouping + sum executed in SQL.
        return await query
            .GroupBy(oi => new
            {
                oi.Dish.CategoryId,
                CategoryName = oi.Dish.Category.Name,
                oi.DishId,
                DishName = oi.Dish.Name
            })
            .Select(g => new DishAggregateRow
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                DishId = g.Key.DishId,
                DishName = g.Key.DishName,
                TotalQuantity = g.Sum(x => x.Quantity),
                OrderCount = g.Count()
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderTimelineRow>> GetOrderTimelinesAsync(int? restaurantId)
    {
        // Single projected, ordered query; segment durations computed in the service.
        return await _context.OrderStatusHistories
            .Where(h => restaurantId == null || h.Order.RestaurantId == restaurantId.Value)
            .OrderBy(h => h.OrderId)
            .ThenBy(h => h.ChangedAt)
            .Select(h => new OrderTimelineRow
            {
                OrderId = h.OrderId,
                RestaurantId = h.Order.RestaurantId,
                RestaurantName = h.Order.Restaurant.Name,
                OrderCreatedAt = h.Order.CreatedAt,
                OldStatus = h.OldStatus,
                NewStatus = h.NewStatus,
                ChangedAt = h.ChangedAt
            })
            .ToListAsync();
    }
}
