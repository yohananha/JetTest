using JetTest.DAL.Interfaces;
using JetTest.DAL.Reports;
using JetTest.Data;
using Microsoft.EntityFrameworkCore;

namespace JetTest.DAL.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context) => _context = context;

    /// <summary>
    /// Two CTEs:
    ///   DishTotals  — GROUP BY dish/category, SUM quantities in SQL.
    ///   Ranked      — ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY TotalQuantity DESC)
    ///                 so the outer WHERE filters to topN per category before any data leaves the DB.
    /// </summary>
    public async Task<IEnumerable<DishAggregateRow>> GetTopDishesRankedAsync(
        int? restaurantId, int topN)
    {
        return await _context.Database
            .SqlQuery<DishAggregateRow>($"""
                WITH DishTotals AS (
                    SELECT
                        d.CategoryId,
                        c.Name                     AS CategoryName,
                        oi.DishId,
                        d.Name                     AS DishName,
                        SUM(oi.Quantity)            AS TotalQuantity,
                        COUNT(DISTINCT oi.OrderId)  AS OrderCount
                    FROM  OrderItems  oi
                    INNER JOIN Dishes      d  ON d.Id  = oi.DishId
                    INNER JOIN Categories  c  ON c.Id  = d.CategoryId
                    INNER JOIN Orders      o  ON o.Id  = oi.OrderId
                    WHERE ({restaurantId} IS NULL OR o.RestaurantId = {restaurantId})
                    GROUP BY d.CategoryId, c.Name, oi.DishId, d.Name
                ),
                Ranked AS (
                    SELECT *,
                           ROW_NUMBER() OVER (
                               PARTITION BY CategoryId
                               ORDER BY TotalQuantity DESC, DishName
                           ) AS Rn
                    FROM DishTotals
                )
                SELECT CategoryId, CategoryName, DishId, DishName, TotalQuantity, OrderCount
                FROM   Ranked
                WHERE  Rn <= {topN}
                ORDER BY CategoryName, TotalQuantity DESC
                """)
            .ToListAsync();
    }

    /// <summary>
    /// Single CTE using two window functions side-by-side:
    ///   LAG(ChangedAt) OVER (PARTITION BY OrderId ORDER BY ChangedAt)
    ///     → previous event time; COALESCE to Order.CreatedAt for the first transition.
    ///   MAX(CASE WHEN NewStatus = 5 THEN DATEDIFF(...) END) OVER (PARTITION BY OrderId)
    ///     → propagates overall delivery minutes onto every row of the same order;
    ///       NULL for non-delivered orders, which the outer WHERE filters out.
    ///
    /// A single GROUP BY then averages both per (restaurant × transition).
    /// The service reads AvgOverallMinutes and DeliveredCount from the Placed row (OldStatus = 0)
    /// of each restaurant group — that row represents all delivered orders.
    /// </summary>
    public async Task<IEnumerable<DeliverySegmentRow>> GetDeliverySegmentsAsync(int? restaurantId)
    {
        return await _context.Database
            .SqlQuery<DeliverySegmentRow>($"""
                WITH Segments AS (
                    SELECT
                        h.OrderId,
                        o.RestaurantId,
                        r.Name                          AS RestaurantName,
                        h.OldStatus,
                        h.NewStatus,
                        DATEDIFF(MINUTE,
                            COALESCE(
                                LAG(h.ChangedAt) OVER (PARTITION BY h.OrderId ORDER BY h.ChangedAt),
                                o.CreatedAt
                            ),
                            h.ChangedAt)                AS SegmentMinutes,
                        MAX(CASE WHEN h.NewStatus = 5
                                 THEN DATEDIFF(MINUTE, o.CreatedAt, h.ChangedAt)
                            END) OVER (PARTITION BY h.OrderId) AS OverallMinutes
                    FROM   OrderStatusHistories h
                    INNER JOIN Orders      o ON o.Id = h.OrderId
                    INNER JOIN Restaurants r ON r.Id = o.RestaurantId
                    WHERE  ({restaurantId} IS NULL OR o.RestaurantId = {restaurantId})
                )
                SELECT
                    RestaurantId,
                    MAX(RestaurantName)                AS RestaurantName,
                    OldStatus,
                    NewStatus,
                    AVG(CAST(SegmentMinutes AS FLOAT)) AS AvgSegmentMinutes,
                    AVG(CAST(OverallMinutes AS FLOAT)) AS AvgOverallMinutes,
                    COUNT(DISTINCT OrderId)            AS DeliveredCount
                FROM   Segments
                WHERE  OverallMinutes IS NOT NULL
                GROUP BY RestaurantId, OldStatus, NewStatus
                ORDER BY RestaurantId, OldStatus
                """)
            .ToListAsync();
    }
}
