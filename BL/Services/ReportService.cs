using JetTest.BL.Interfaces;
using JetTest.DAL.Interfaces;
using JetTest.DAL.Reports;
using JetTest.DTOs.Reports;
using JetTest.Models.Enums;

namespace JetTest.BL.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _repo;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IReportRepository repo, ILogger<ReportService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryTopDishesDto>> GetTopDishesPerCategoryAsync(int? restaurantId, int topN)
    {
        if (topN < 1) topN = 1;

        // Aggregation (group + sum) already executed in SQL.
        var rows = await _repo.GetDishOrderAggregatesAsync(restaurantId);

        var result = rows
            .GroupBy(r => new { r.CategoryId, r.CategoryName })
            .Select(g => new CategoryTopDishesDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                Dishes = g
                    .OrderByDescending(d => d.TotalQuantity)
                    .ThenBy(d => d.DishName)
                    .Take(topN)
                    .Select(d => new TopDishDto
                    {
                        DishId = d.DishId,
                        DishName = d.DishName,
                        TotalQuantity = d.TotalQuantity,
                        OrderCount = d.OrderCount
                    })
                    .ToList()
            })
            .OrderBy(c => c.CategoryName)
            .ToList();

        _logger.LogInformation("Top-dishes report produced {Count} categories (restaurantId={RestaurantId}, topN={TopN})",
            result.Count, restaurantId, topN);
        return result;
    }

    public async Task<IEnumerable<RestaurantDeliveryEfficiencyDto>> GetDeliveryEfficiencyAsync(int? restaurantId)
    {
        var rows = (await _repo.GetOrderTimelinesAsync(restaurantId)).ToList();

        // Accumulators keyed by restaurant.
        var perRestaurant = new Dictionary<int, RestaurantAccumulator>();

        // Rows are already ordered by OrderId then ChangedAt; process per order.
        foreach (var orderGroup in rows.GroupBy(r => r.OrderId))
        {
            var ordered = orderGroup.OrderBy(r => r.ChangedAt).ToList();
            var first = ordered[0];

            if (!perRestaurant.TryGetValue(first.RestaurantId, out var acc))
            {
                acc = new RestaurantAccumulator
                {
                    RestaurantId = first.RestaurantId,
                    RestaurantName = first.RestaurantName
                };
                perRestaurant[first.RestaurantId] = acc;
            }

            // Segment durations: time spent in OldStatus = thisChangedAt - previousPoint.
            DateTime previousPoint = first.OrderCreatedAt;
            foreach (var row in ordered)
            {
                var minutes = (row.ChangedAt - previousPoint).TotalMinutes;
                var key = (row.OldStatus, row.NewStatus);

                if (!acc.Transitions.TryGetValue(key, out var stat))
                {
                    stat = new DurationStat();
                    acc.Transitions[key] = stat;
                }
                stat.Add(minutes);

                previousPoint = row.ChangedAt;

                // Overall delivery time = Delivered timestamp - order creation.
                if (row.NewStatus == OrderStatus.Delivered)
                {
                    acc.OverallMinutes.Add((row.ChangedAt - first.OrderCreatedAt).TotalMinutes);
                }
            }
        }

        var result = perRestaurant.Values
            .Select(acc => new RestaurantDeliveryEfficiencyDto
            {
                RestaurantId = acc.RestaurantId,
                RestaurantName = acc.RestaurantName,
                DeliveredOrdersCount = acc.OverallMinutes.Count,
                AverageOverallMinutes = acc.OverallMinutes.Count > 0
                    ? Math.Round(acc.OverallMinutes.Average, 2)
                    : 0,
                TransitionAverages = acc.Transitions
                    .Select(kvp => new StatusTransitionAvgDto
                    {
                        FromStatus = kvp.Key.Item1,
                        ToStatus = kvp.Key.Item2,
                        AverageMinutes = Math.Round(kvp.Value.Average, 2)
                    })
                    .OrderBy(t => t.FromStatus)
                    .ThenBy(t => t.ToStatus)
                    .ToList()
            })
            .OrderBy(r => r.RestaurantName)
            .ToList();

        _logger.LogInformation("Delivery-efficiency report produced {Count} restaurants (restaurantId={RestaurantId})",
            result.Count, restaurantId);
        return result;
    }

    private class RestaurantAccumulator
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public Dictionary<(OrderStatus, OrderStatus), DurationStat> Transitions { get; } = new();
        public DurationStat OverallMinutes { get; } = new();
    }

    private class DurationStat
    {
        private double _sum;
        public int Count { get; private set; }
        public void Add(double minutes)
        {
            _sum += minutes;
            Count++;
        }
        public double Average => Count > 0 ? _sum / Count : 0;
    }
}
