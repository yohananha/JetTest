using JetTest.DAL.Reports;

namespace JetTest.DAL.Interfaces;

public interface IReportRepository
{
    /// <summary>
    /// Returns the top <paramref name="topN"/> dishes per category, ranked inside SQL
    /// using ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY TotalQuantity DESC).
    /// </summary>
    Task<IEnumerable<DishAggregateRow>> GetTopDishesRankedAsync(int? restaurantId, int topN);

    /// <summary>
    /// Returns one row per (restaurant × status-transition), with per-segment and overall
    /// delivery-time averages computed in SQL via LAG() + DATEDIFF() CTEs.
    /// </summary>
    Task<IEnumerable<DeliverySegmentRow>> GetDeliverySegmentsAsync(int? restaurantId);
}
