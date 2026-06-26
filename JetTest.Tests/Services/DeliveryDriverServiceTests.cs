using FluentAssertions;
using JetTest.BL.Exceptions;
using JetTest.BL.Services;
using JetTest.DAL.Interfaces;
using JetTest.Data;
using JetTest.DTOs.DeliveryDrivers;
using JetTest.Models;
using JetTest.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace JetTest.Tests.Services;

public class DeliveryDriverServiceTests
{
    private readonly Mock<IDeliveryDriverRepository> _repoMock = new();
    private readonly Mock<ILogger<DeliveryDriverService>> _loggerMock = new();
    private readonly AppDbContext _context;
    private readonly DeliveryDriverService _sut;

    public DeliveryDriverServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _sut = new DeliveryDriverService(_repoMock.Object, _context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllDrivers()
    {
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DeliveryDriver>
        {
            new() { Id = 1, Name = "John" },
            new() { Id = 2, Name = "Jane" }
        });

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsDriver()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new DeliveryDriver { Id = 1, Name = "John" });

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Name.Should().Be("John");
    }

    [Fact]
    public async Task GetById_NonExistingId_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((DeliveryDriver?)null);

        await _sut.Invoking(s => s.GetByIdAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedDriver()
    {
        var dto = new CreateDeliveryDriverDto { Name = "John", Phone = "111", VehicleType = "Car" };
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<DeliveryDriver>()))
            .ReturnsAsync((DeliveryDriver d) => { d.Id = 1; return d; });

        var result = await _sut.CreateAsync(dto);

        result.Id.Should().Be(1);
        result.Name.Should().Be("John");
    }

    [Fact]
    public async Task Update_ExistingId_UpdatesFields()
    {
        var driver = new DeliveryDriver { Id = 1, Name = "Old", Phone = "000", VehicleType = "Bike" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);

        var result = await _sut.UpdateAsync(1, new UpdateDeliveryDriverDto { Name = "New", Phone = "999", VehicleType = "Car" });

        result.Name.Should().Be("New");
        result.VehicleType.Should().Be("Car");
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<DeliveryDriver>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ExistingId_CallsRepository()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new DeliveryDriver { Id = 1 });

        await _sut.DeleteAsync(1);

        _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_DifferentStatus_WritesHistoryRow()
    {
        var driver = new DeliveryDriver { Id = 1, Name = "John", Status = DriverStatus.Offline };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);

        await _sut.UpdateStatusAsync(1, DriverStatus.Available);

        _context.DriverStatusHistories.Local.Should().HaveCount(1);
        _context.DriverStatusHistories.Local.First().OldStatus.Should().Be(DriverStatus.Offline);
        _context.DriverStatusHistories.Local.First().NewStatus.Should().Be(DriverStatus.Available);
    }

    [Fact]
    public async Task UpdateStatus_SameStatus_ThrowsInvalidOperationException()
    {
        var driver = new DeliveryDriver { Id = 1, Status = DriverStatus.Available };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);

        await _sut.Invoking(s => s.UpdateStatusAsync(1, DriverStatus.Available))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}
