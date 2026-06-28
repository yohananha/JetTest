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
    /// Four CTEs:
    ///   DeliveredOrders  — identifies delivered orders and computes overall delivery minutes
    ///                      (CreatedAt → Delivered timestamp) per order.
    ///   RestaurantOverall — aggregates overall minutes to restaurant level (AVG, COUNT).
    ///   SegmentDurations  — uses LAG() OVER (PARTITION BY OrderId ORDER BY ChangedAt) to get
    ///                       the previous event timestamp; DATEDIFF gives segment minutes.
    ///                       COALESCE falls back to Order.CreatedAt for the first transition.
    ///   TransitionAverages — averages segment minutes by (restaurant × OldStatus × NewStatus).
    ///
    /// Final SELECT joins TransitionAverages with RestaurantOverall so each transition row
    /// carries the restaurant-level overall average — the service reads it from any row in the group.
    /// </summary>
    public async Task<IEnumerable<DeliverySegmentRow>> GetDeliverySegmentsAsync(int? restaurantId)
    {
        return await _context.Database
            .SqlQuery<DeliverySegmentRow>($"""
                WITH DeliveredOrders AS (
                    SELECT h.OrderId,
                           o.RestaurantId,
                           DATEDIFF(MINUTE, o.CreatedAt, h.ChangedAt) AS OverallMinutes
                    FROM   OrderStatusHistories h
                    INNER JOIN Orders o ON o.Id = h.OrderId
                    WHERE  h.NewStatus = 5   -- OrderStatus.Delivered
                      AND  ({restaurantId} IS NULL OR o.RestaurantId = {restaurantId})
                ),
                RestaurantOverall AS (
                    SELECT RestaurantId,
                           AVG(CAST(OverallMinutes AS FLOAT)) AS AvgOverallMinutes,
                           COUNT(*)                           AS DeliveredCount
                    FROM   DeliveredOrders
                    GROUP BY RestaurantId
                ),
                SegmentDurations AS (
                    SELECT h.OrderId,
                           o.RestaurantId,
                           r.Name AS RestaurantName,
                           h.OldStatus,
                           h.NewStatus,
                           DATEDIFF(MINUTE,
                               COALESCE(
                                   LAG(h.ChangedAt) OVER (PARTITION BY h.OrderId ORDER BY h.ChangedAt),
                                   o.CreatedAt      -- first transition: measure from order creation
                               ),
                               h.ChangedAt
                           ) AS SegmentMinutes
                    FROM   OrderStatusHistories h
                    INNER JOIN Orders      o  ON o.Id = h.OrderId
                    INNER JOIN Restaurants r  ON r.Id = o.RestaurantId
                    INNER JOIN DeliveredOrders d ON d.OrderId = h.OrderId
                ),
                TransitionAverages AS (
                    SELECT RestaurantId,
                           MAX(RestaurantName)               AS RestaurantName,
                           OldStatus,
                           NewStatus,
                           AVG(CAST(SegmentMinutes AS FLOAT)) AS AvgSegmentMinutes
                    FROM   SegmentDurations
                    GROUP BY RestaurantId, OldStatus, NewStatus
                )
                SELECT t.RestaurantId,
                       t.RestaurantName,
                       t.OldStatus,
                       t.NewStatus,
                       t.AvgSegmentMinutes,
                       ro.AvgOverallMinutes,
                       ro.DeliveredCount
                FROM   TransitionAverages t
                INNER JOIN RestaurantOverall ro ON ro.RestaurantId = t.RestaurantId
                ORDER BY t.RestaurantId, t.OldStatus
                """)
            .ToListAsync();
    }
}
