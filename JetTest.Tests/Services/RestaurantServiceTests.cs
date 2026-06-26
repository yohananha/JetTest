using FluentAssertions;
using JetTest.BL.Exceptions;
using JetTest.BL.Services;
using JetTest.DAL.Interfaces;
using JetTest.DTOs.Restaurants;
using JetTest.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace JetTest.Tests.Services;

public class RestaurantServiceTests
{
    private readonly Mock<IRestaurantRepository> _repoMock = new();
    private readonly Mock<ILogger<RestaurantService>> _loggerMock = new();
    private readonly RestaurantService _sut;

    public RestaurantServiceTests()
    {
        _sut = new RestaurantService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllRestaurants()
    {
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Restaurant>
        {
            new() { Id = 1, Name = "Pizza Place" },
            new() { Id = 2, Name = "Sushi Bar" }
        });

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsRestaurant()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Restaurant { Id = 1, Name = "Pizza Place" });

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Pizza Place");
    }

    [Fact]
    public async Task GetById_NonExistingId_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Restaurant?)null);

        await _sut.Invoking(s => s.GetByIdAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedRestaurant()
    {
        var dto = new CreateRestaurantDto { Name = "Pizza Place", Address = "123 St", Phone = "111", CuisineType = "Italian" };
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Restaurant>()))
            .ReturnsAsync((Restaurant r) => { r.Id = 1; return r; });

        var result = await _sut.CreateAsync(dto);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Pizza Place");
    }

    [Fact]
    public async Task Update_ExistingId_UpdatesFields()
    {
        var restaurant = new Restaurant { Id = 1, Name = "Old Name", Address = "Old", Phone = "000", CuisineType = "Thai" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(restaurant);

        var result = await _sut.UpdateAsync(1, new UpdateRestaurantDto { Name = "New Name", Address = "New", Phone = "999", CuisineType = "Italian" });

        result.Name.Should().Be("New Name");
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Restaurant>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ExistingId_CallsRepository()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Restaurant { Id = 1 });

        await _sut.DeleteAsync(1);

        _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task SetActive_TogglesIsActive()
    {
        var restaurant = new Restaurant { Id = 1, IsActive = true };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(restaurant);

        var result = await _sut.SetActiveAsync(1, false);

        result.IsActive.Should().BeFalse();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Restaurant>()), Times.Once);
    }

    [Fact]
    public async Task SetActive_NonExistingId_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Restaurant?)null);

        await _sut.Invoking(s => s.SetActiveAsync(99, false))
            .Should().ThrowAsync<NotFoundException>();
    }
}
