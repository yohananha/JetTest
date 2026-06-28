using JetTest.BL.Interfaces;
using JetTest.DAL.Interfaces;
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

    public async Task<IEnumerable<CategoryTopDishesDto>> GetTopDishesPerCategoryAsync(
        int? restaurantId, int topN)
    {
        if (topN < 1) topN = 1;

        // SQL already ranked within each category via ROW_NUMBER() and filtered to topN rows.
        // The service only needs to group into the response shape.
        var rows = await _repo.GetTopDishesRankedAsync(restaurantId, topN);

        var result = rows
            .GroupBy(r => new { r.CategoryId, r.CategoryName })
            .Select(g => new CategoryTopDishesDto
            {
                CategoryId   = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                Dishes = g.Select(d => new TopDishDto
                {
                    DishId        = d.DishId,
                    DishName      = d.DishName,
                    TotalQuantity = d.TotalQuantity,
                    OrderCount    = d.OrderCount
                }).ToList()   // already ordered by SQL (TotalQuantity DESC)
            })
            .OrderBy(c => c.CategoryName)
            .ToList();

        _logger.LogInformation(
            "Top-dishes report: {Count} categories (restaurantId={RestaurantId}, topN={TopN})",
            result.Count, restaurantId, topN);

        return result;
    }

    public async Task<IEnumerable<RestaurantDeliveryEfficiencyDto>> GetDeliveryEfficiencyAsync(
        int? restaurantId)
    {
        // SQL computed LAG-based segment durations and AVG'd them per (restaurant × transition).
        // AvgOverallMinutes and DeliveredCount are restaurant-level values repeated on every row.
        var rows = await _repo.GetDeliverySegmentsAsync(restaurantId);

        var result = rows
            .GroupBy(r => new { r.RestaurantId, r.RestaurantName })
            .Select(g =>
            {
                var first = g.First();
                return new RestaurantDeliveryEfficiencyDto
                {
                    RestaurantId         = g.Key.RestaurantId,
                    RestaurantName       = g.Key.RestaurantName,
                    DeliveredOrdersCount = first.DeliveredCount,
                    AverageOverallMinutes = Math.Round(first.AvgOverallMinutes, 2),
                    TransitionAverages = g
                        .Select(r => new StatusTransitionAvgDto
                        {
                            FromStatus     = (OrderStatus)r.OldStatus,
                            ToStatus       = (OrderStatus)r.NewStatus,
                            AverageMinutes = Math.Round(r.AvgSegmentMinutes, 2)
                        })
                        .OrderBy(t => t.FromStatus)
                        .ThenBy(t => t.ToStatus)
                        .ToList()
                };
            })
            .OrderBy(r => r.RestaurantName)
            .ToList();

        _logger.LogInformation(
            "Delivery-efficiency report: {Count} restaurants (restaurantId={RestaurantId})",
            result.Count, restaurantId);

        return result;
    }
}
