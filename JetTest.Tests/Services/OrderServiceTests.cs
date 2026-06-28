using FluentAssertions;
using JetTest.BL.Exceptions;
using JetTest.BL.Services;
using JetTest.DAL.Interfaces;
using JetTest.Data;
using JetTest.DTOs.Orders;
using JetTest.Models;
using JetTest.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace JetTest.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IRestaurantRepository> _restaurantRepoMock = new();
    private readonly Mock<IDishRepository> _dishRepoMock = new();
    private readonly Mock<ILogger<OrderService>> _loggerMock = new();
    private readonly AppDbContext _context;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _sut = new OrderService(
            _orderRepoMock.Object,
            _customerRepoMock.Object,
            _restaurantRepoMock.Object,
            _dishRepoMock.Object,
            _context,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllOrders()
    {
        _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Order>
        {
            new() { Id = 1, DeliveryAddress = "Addr1" },
            new() { Id = 2, DeliveryAddress = "Addr2" }
        });

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOrder()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Order { Id = 1, DeliveryAddress = "Addr1" });

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingId_ThrowsNotFoundException()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Order?)null);

        await _sut.Invoking(s => s.GetByIdAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedOrder()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Customer { Id = 1 });
        _restaurantRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Restaurant { Id = 2 });
        _orderRepoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => { o.Id = 10; return o; });

        var dto = new CreateOrderDto { CustomerId = 1, RestaurantId = 2, DeliveryAddress = "123 St" };
        var result = await _sut.CreateAsync(dto);

        result.Id.Should().Be(10);
        result.DeliveryAddress.Should().Be("123 St");
    }

    [Fact]
    public async Task Create_InvalidCustomerId_ThrowsNotFoundException()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Customer?)null);

        var dto = new CreateOrderDto { CustomerId = 99, RestaurantId = 1, DeliveryAddress = "Addr" };
        await _sut.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_InvalidRestaurantId_ThrowsNotFoundException()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Customer { Id = 1 });
        _restaurantRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Restaurant?)null);

        var dto = new CreateOrderDto { CustomerId = 1, RestaurantId = 99, DeliveryAddress = "Addr" };
        await _sut.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_WithItems_SnapshotsUnitPriceAndAttachesItems()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Customer { Id = 1 });
        _restaurantRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Restaurant { Id = 2 });
        _dishRepoMock.Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(new Dish { Id = 5, Name = "Pizza", Price = 42.50m });
        _orderRepoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => { o.Id = 10; return o; });

        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            RestaurantId = 2,
            DeliveryAddress = "123 St",
            Items = new() { new CreateOrderItemDto { DishId = 5, Quantity = 3 } }
        };
        var result = await _sut.CreateAsync(dto);

        result.Items.Should().HaveCount(1);
        result.Items[0].DishId.Should().Be(5);
        result.Items[0].DishName.Should().Be("Pizza");
        result.Items[0].Quantity.Should().Be(3);
        result.Items[0].UnitPrice.Should().Be(42.50m);
    }

    [Fact]
    public async Task Create_InvalidDishId_ThrowsNotFoundException()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Customer { Id = 1 });
        _restaurantRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Restaurant { Id = 2 });
        _dishRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Dish?)null);

        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            RestaurantId = 2,
            DeliveryAddress = "Addr",
            Items = new() { new CreateOrderItemDto { DishId = 99, Quantity = 1 } }
        };
        await _sut.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_ExistingId_UpdatesFields()
    {
        var order = new Order { Id = 1, DeliveryAddress = "Old Addr", Notes = null };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _sut.UpdateAsync(1, new UpdateOrderDto { DeliveryAddress = "New Addr", Notes = "Extra sauce" });

        result.DeliveryAddress.Should().Be("New Addr");
        result.Notes.Should().Be("Extra sauce");
    }

    [Fact]
    public async Task Delete_ExistingId_CallsRepository()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Order { Id = 1 });

        await _sut.DeleteAsync(1);

        _orderRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_ValidTransition_WritesHistoryAndUpdatesTimestamp()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Placed, UpdatedAt = DateTime.UtcNow.AddHours(-1) };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var before = DateTime.UtcNow;
        var result = await _sut.UpdateStatusAsync(1, OrderStatus.Accepted, "Accepted by kitchen");

        result.Status.Should().Be(OrderStatus.Accepted);
        result.UpdatedAt.Should().BeOnOrAfter(before);

        _context.OrderStatusHistories.Local.Should().HaveCount(1);
        _context.OrderStatusHistories.Local.First().OldStatus.Should().Be(OrderStatus.Placed);
        _context.OrderStatusHistories.Local.First().NewStatus.Should().Be(OrderStatus.Accepted);
        _context.OrderStatusHistories.Local.First().Notes.Should().Be("Accepted by kitchen");
    }

    [Fact]
    public async Task UpdateStatus_SameStatus_ThrowsInvalidOperationException()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Placed };
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        await _sut.Invoking(s => s.UpdateStatusAsync(1, OrderStatus.Placed))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}
