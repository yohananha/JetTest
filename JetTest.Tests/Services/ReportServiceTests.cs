using FluentAssertions;
using JetTest.BL.Services;
using JetTest.DAL.Repositories;
using JetTest.Data;
using JetTest.Models;
using JetTest.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace JetTest.Tests.Services;

public class ReportServiceTests
{
    private readonly AppDbContext _context;
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        // Real repository over the in-memory context, mocked logger.
        var repo = new ReportRepository(_context);
        _sut = new ReportService(repo, new Mock<ILogger<ReportService>>().Object);
    }

    // ---------- Top dishes ----------

    private void SeedDishData()
    {
        _context.Restaurants.AddRange(
            new Restaurant { Id = 1, Name = "Bistro" },
            new Restaurant { Id = 2, Name = "Cafe" });

        _context.Categories.AddRange(
            new Category { Id = 1, Name = "Mains" },
            new Category { Id = 2, Name = "Desserts" });

        _context.Dishes.AddRange(
            new Dish { Id = 1, Name = "Pizza", CategoryId = 1, RestaurantId = 1, Price = 40m },
            new Dish { Id = 2, Name = "Burger", CategoryId = 1, RestaurantId = 1, Price = 35m },
            new Dish { Id = 3, Name = "Cake", CategoryId = 2, RestaurantId = 1, Price = 20m },
            new Dish { Id = 4, Name = "Salad", CategoryId = 1, RestaurantId = 2, Price = 25m });

        // R1 orders
        _context.Orders.Add(new Order { Id = 1, CustomerId = 1, RestaurantId = 1, DeliveryAddress = "a" });
        _context.Orders.Add(new Order { Id = 2, CustomerId = 1, RestaurantId = 1, DeliveryAddress = "b" });
        // R2 order
        _context.Orders.Add(new Order { Id = 3, CustomerId = 1, RestaurantId = 2, DeliveryAddress = "c" });

        _context.OrderItems.AddRange(
            new OrderItem { Id = 1, OrderId = 1, DishId = 1, Quantity = 5, UnitPrice = 40m }, // Pizza
            new OrderItem { Id = 2, OrderId = 1, DishId = 2, Quantity = 2, UnitPrice = 35m }, // Burger
            new OrderItem { Id = 3, OrderId = 1, DishId = 3, Quantity = 3, UnitPrice = 20m }, // Cake
            new OrderItem { Id = 4, OrderId = 2, DishId = 1, Quantity = 1, UnitPrice = 40m }, // Pizza
            new OrderItem { Id = 5, OrderId = 2, DishId = 3, Quantity = 1, UnitPrice = 20m }, // Cake
            new OrderItem { Id = 6, OrderId = 3, DishId = 4, Quantity = 10, UnitPrice = 25m }); // Salad (R2)

        _context.SaveChanges();
    }

    [Fact]
    public async Task TopDishes_FilteredByRestaurant_RanksByQuantityWithinCategory()
    {
        SeedDishData();

        var result = (await _sut.GetTopDishesPerCategoryAsync(restaurantId: 1, topN: 5)).ToList();

        result.Should().HaveCount(2); // Mains + Desserts

        var mains = result.Single(c => c.CategoryName == "Mains");
        mains.Dishes.Select(d => d.DishName).Should().ContainInOrder("Pizza", "Burger");
        mains.Dishes.First().TotalQuantity.Should().Be(6); // Pizza 5 + 1

        var desserts = result.Single(c => c.CategoryName == "Desserts");
        desserts.Dishes.Should().ContainSingle();
        desserts.Dishes[0].DishName.Should().Be("Cake");
        desserts.Dishes[0].TotalQuantity.Should().Be(4); // Cake 3 + 1
    }

    [Fact]
    public async Task TopDishes_RespectsTopN()
    {
        SeedDishData();

        var result = (await _sut.GetTopDishesPerCategoryAsync(restaurantId: 1, topN: 1)).ToList();

        var mains = result.Single(c => c.CategoryName == "Mains");
        mains.Dishes.Should().ContainSingle();
        mains.Dishes[0].DishName.Should().Be("Pizza");
    }

    [Fact]
    public async Task TopDishes_AllRestaurants_AggregatesAcrossRestaurants()
    {
        SeedDishData();

        var result = (await _sut.GetTopDishesPerCategoryAsync(restaurantId: null, topN: 1)).ToList();

        // Across all restaurants the top Main is Salad (qty 10) over Pizza (qty 6).
        var mains = result.Single(c => c.CategoryName == "Mains");
        mains.Dishes[0].DishName.Should().Be("Salad");
        mains.Dishes[0].TotalQuantity.Should().Be(10);
    }

    [Fact]
    public async Task TopDishes_NoData_ReturnsEmpty()
    {
        var result = await _sut.GetTopDishesPerCategoryAsync(null, 5);
        result.Should().BeEmpty();
    }

    // ---------- Delivery efficiency ----------

    private void SeedDeliveryData()
    {
        _context.Restaurants.AddRange(
            new Restaurant { Id = 1, Name = "Bistro" },
            new Restaurant { Id = 2, Name = "Cafe" });

        var o1Created = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var o2Created = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var o3Created = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        _context.Orders.AddRange(
            new Order { Id = 1, CustomerId = 1, RestaurantId = 1, DeliveryAddress = "a", CreatedAt = o1Created },
            new Order { Id = 2, CustomerId = 1, RestaurantId = 1, DeliveryAddress = "b", CreatedAt = o2Created },
            new Order { Id = 3, CustomerId = 1, RestaurantId = 2, DeliveryAddress = "c", CreatedAt = o3Created });

        // O1 (R1): Placed 10m, Accepted 20m, Preparing 30m, overall 60m
        _context.OrderStatusHistories.AddRange(
            new OrderStatusHistory { Id = 1, OrderId = 1, OldStatus = OrderStatus.Placed, NewStatus = OrderStatus.Accepted, ChangedAt = o1Created.AddMinutes(10) },
            new OrderStatusHistory { Id = 2, OrderId = 1, OldStatus = OrderStatus.Accepted, NewStatus = OrderStatus.Preparing, ChangedAt = o1Created.AddMinutes(30) },
            new OrderStatusHistory { Id = 3, OrderId = 1, OldStatus = OrderStatus.Preparing, NewStatus = OrderStatus.Delivered, ChangedAt = o1Created.AddMinutes(60) });

        // O2 (R1): Placed 20m, Accepted 30m, Preparing 50m, overall 100m
        _context.OrderStatusHistories.AddRange(
            new OrderStatusHistory { Id = 4, OrderId = 2, OldStatus = OrderStatus.Placed, NewStatus = OrderStatus.Accepted, ChangedAt = o2Created.AddMinutes(20) },
            new OrderStatusHistory { Id = 5, OrderId = 2, OldStatus = OrderStatus.Accepted, NewStatus = OrderStatus.Preparing, ChangedAt = o2Created.AddMinutes(50) },
            new OrderStatusHistory { Id = 6, OrderId = 2, OldStatus = OrderStatus.Preparing, NewStatus = OrderStatus.Delivered, ChangedAt = o2Created.AddMinutes(100) });

        // O3 (R2): Placed 5m, Accepted 30m, overall 35m
        _context.OrderStatusHistories.AddRange(
            new OrderStatusHistory { Id = 7, OrderId = 3, OldStatus = OrderStatus.Placed, NewStatus = OrderStatus.Accepted, ChangedAt = o3Created.AddMinutes(5) },
            new OrderStatusHistory { Id = 8, OrderId = 3, OldStatus = OrderStatus.Accepted, NewStatus = OrderStatus.Delivered, ChangedAt = o3Created.AddMinutes(35) });

        _context.SaveChanges();
    }

    [Fact]
    public async Task DeliveryEfficiency_ComputesPerTransitionAndOverallAverages()
    {
        SeedDeliveryData();

        var result = (await _sut.GetDeliveryEfficiencyAsync(restaurantId: 1)).ToList();

        result.Should().ContainSingle();
        var r1 = result[0];
        r1.RestaurantName.Should().Be("Bistro");
        r1.DeliveredOrdersCount.Should().Be(2);
        r1.AverageOverallMinutes.Should().Be(80); // (60 + 100) / 2

        var placedToAccepted = r1.TransitionAverages.Single(t => t.FromStatus == OrderStatus.Placed && t.ToStatus == OrderStatus.Accepted);
        placedToAccepted.AverageMinutes.Should().Be(15); // (10 + 20) / 2

        var acceptedToPreparing = r1.TransitionAverages.Single(t => t.FromStatus == OrderStatus.Accepted && t.ToStatus == OrderStatus.Preparing);
        acceptedToPreparing.AverageMinutes.Should().Be(25); // (20 + 30) / 2

        var preparingToDelivered = r1.TransitionAverages.Single(t => t.FromStatus == OrderStatus.Preparing && t.ToStatus == OrderStatus.Delivered);
        preparingToDelivered.AverageMinutes.Should().Be(40); // (30 + 50) / 2
    }

    [Fact]
    public async Task DeliveryEfficiency_AllRestaurants_ReturnsEachRestaurant()
    {
        SeedDeliveryData();

        var result = (await _sut.GetDeliveryEfficiencyAsync(null)).ToList();

        result.Should().HaveCount(2);
        result.Single(r => r.RestaurantName == "Cafe").AverageOverallMinutes.Should().Be(35);
    }

    [Fact]
    public async Task DeliveryEfficiency_NoData_ReturnsEmpty()
    {
        var result = await _sut.GetDeliveryEfficiencyAsync(null);
        result.Should().BeEmpty();
    }
}
