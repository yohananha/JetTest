using FluentAssertions;
using JetTest.BL.Services;
using JetTest.DAL.Interfaces;
using JetTest.DAL.Reports;
using JetTest.DTOs.Reports;
using JetTest.Models.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace JetTest.Tests.Services;

// Note: ReportRepository uses raw SQL (ROW_NUMBER, LAG, DATEDIFF) which cannot run against
// the EF in-memory provider. These tests mock IReportRepository and verify that ReportService
// correctly assembles DTOs from the pre-aggregated rows the repository returns.
// SQL correctness is validated end-to-end against a real SQL Server instance.
public class ReportServiceTests
{
    private readonly Mock<IReportRepository> _repoMock = new();
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        _sut = new ReportService(_repoMock.Object, new Mock<ILogger<ReportService>>().Object);
    }

    // ── Top dishes ────────────────────────────────────────────────────────────

    [Fact]
    public async Task TopDishes_GroupsByCategory_MapsToDto()
    {
        // Simulate what SQL returns after ROW_NUMBER filtering — already the top rows.
        _repoMock.Setup(r => r.GetTopDishesRankedAsync(null, 5))
            .ReturnsAsync(new List<DishAggregateRow>
            {
                new() { CategoryId = 1, CategoryName = "Mains",    DishId = 10, DishName = "Pizza",  TotalQuantity = 6, OrderCount = 3 },
                new() { CategoryId = 1, CategoryName = "Mains",    DishId = 11, DishName = "Burger", TotalQuantity = 2, OrderCount = 1 },
                new() { CategoryId = 2, CategoryName = "Desserts", DishId = 20, DishName = "Cake",   TotalQuantity = 4, OrderCount = 2 },
            });

        var result = (await _sut.GetTopDishesPerCategoryAsync(null, 5)).ToList();

        result.Should().HaveCount(2);

        var mains = result.Single(c => c.CategoryName == "Mains");
        mains.Dishes.Should().HaveCount(2);
        mains.Dishes[0].DishName.Should().Be("Pizza");
        mains.Dishes[0].TotalQuantity.Should().Be(6);

        var desserts = result.Single(c => c.CategoryName == "Desserts");
        desserts.Dishes.Should().ContainSingle().Which.DishName.Should().Be("Cake");
    }

    [Fact]
    public async Task TopDishes_PassesTopNToRepository()
    {
        _repoMock.Setup(r => r.GetTopDishesRankedAsync(1, 3))
            .ReturnsAsync(new List<DishAggregateRow>
            {
                new() { CategoryId = 1, CategoryName = "Mains", DishId = 10, DishName = "Pizza", TotalQuantity = 6, OrderCount = 3 },
            });

        await _sut.GetTopDishesPerCategoryAsync(restaurantId: 1, topN: 3);

        _repoMock.Verify(r => r.GetTopDishesRankedAsync(1, 3), Times.Once);
    }

    [Fact]
    public async Task TopDishes_ClampsTopNToMinimumOne()
    {
        _repoMock.Setup(r => r.GetTopDishesRankedAsync(null, 1)).ReturnsAsync(new List<DishAggregateRow>());

        await _sut.GetTopDishesPerCategoryAsync(null, topN: 0);

        // topN 0 is clamped to 1 before being forwarded to the repo
        _repoMock.Verify(r => r.GetTopDishesRankedAsync(null, 1), Times.Once);
    }

    [Fact]
    public async Task TopDishes_NoData_ReturnsEmpty()
    {
        _repoMock.Setup(r => r.GetTopDishesRankedAsync(null, 5)).ReturnsAsync(new List<DishAggregateRow>());

        var result = await _sut.GetTopDishesPerCategoryAsync(null, 5);

        result.Should().BeEmpty();
    }

    // ── Delivery efficiency ───────────────────────────────────────────────────

    [Fact]
    public async Task DeliveryEfficiency_MapsSegmentRowsToDtos()
    {
        // Simulate SQL output: restaurant R1 has two transitions; AvgOverall repeats on each row.
        _repoMock.Setup(r => r.GetDeliverySegmentsAsync(null))
            .ReturnsAsync(new List<DeliverySegmentRow>
            {
                new() { RestaurantId = 1, RestaurantName = "Bistro",
                        OldStatus = (int)OrderStatus.Placed,   NewStatus = (int)OrderStatus.Accepted,
                        AvgSegmentMinutes = 15, AvgOverallMinutes = 80, DeliveredCount = 2 },
                new() { RestaurantId = 1, RestaurantName = "Bistro",
                        OldStatus = (int)OrderStatus.Accepted, NewStatus = (int)OrderStatus.Delivered,
                        AvgSegmentMinutes = 65, AvgOverallMinutes = 80, DeliveredCount = 2 },
            });

        var result = (await _sut.GetDeliveryEfficiencyAsync(null)).ToList();

        result.Should().ContainSingle();
        var r1 = result[0];
        r1.RestaurantName.Should().Be("Bistro");
        r1.DeliveredOrdersCount.Should().Be(2);
        r1.AverageOverallMinutes.Should().Be(80);
        r1.TransitionAverages.Should().HaveCount(2);

        var placedToAccepted = r1.TransitionAverages
            .Single(t => t.FromStatus == OrderStatus.Placed && t.ToStatus == OrderStatus.Accepted);
        placedToAccepted.AverageMinutes.Should().Be(15);
    }

    [Fact]
    public async Task DeliveryEfficiency_MultipleRestaurants_GroupedSeparately()
    {
        _repoMock.Setup(r => r.GetDeliverySegmentsAsync(null))
            .ReturnsAsync(new List<DeliverySegmentRow>
            {
                new() { RestaurantId = 1, RestaurantName = "Bistro",
                        OldStatus = (int)OrderStatus.Placed, NewStatus = (int)OrderStatus.Delivered,
                        AvgSegmentMinutes = 60, AvgOverallMinutes = 60, DeliveredCount = 1 },
                new() { RestaurantId = 2, RestaurantName = "Cafe",
                        OldStatus = (int)OrderStatus.Placed, NewStatus = (int)OrderStatus.Delivered,
                        AvgSegmentMinutes = 35, AvgOverallMinutes = 35, DeliveredCount = 1 },
            });

        var result = (await _sut.GetDeliveryEfficiencyAsync(null)).ToList();

        result.Should().HaveCount(2);
        result.Single(r => r.RestaurantName == "Cafe").AverageOverallMinutes.Should().Be(35);
    }

    [Fact]
    public async Task DeliveryEfficiency_PassesRestaurantIdFilterToRepository()
    {
        _repoMock.Setup(r => r.GetDeliverySegmentsAsync(7)).ReturnsAsync(new List<DeliverySegmentRow>());

        await _sut.GetDeliveryEfficiencyAsync(restaurantId: 7);

        _repoMock.Verify(r => r.GetDeliverySegmentsAsync(7), Times.Once);
    }

    [Fact]
    public async Task DeliveryEfficiency_NoData_ReturnsEmpty()
    {
        _repoMock.Setup(r => r.GetDeliverySegmentsAsync(null)).ReturnsAsync(new List<DeliverySegmentRow>());

        var result = await _sut.GetDeliveryEfficiencyAsync(null);

        result.Should().BeEmpty();
    }
}
