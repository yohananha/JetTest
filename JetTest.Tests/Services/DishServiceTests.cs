using FluentAssertions;
using JetTest.BL.Exceptions;
using JetTest.BL.Services;
using JetTest.DAL.Interfaces;
using JetTest.DTOs.Dishes;
using JetTest.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace JetTest.Tests.Services;

public class DishServiceTests
{
    private readonly Mock<IDishRepository> _repoMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IRestaurantRepository> _restaurantRepoMock = new();
    private readonly Mock<ILogger<DishService>> _loggerMock = new();
    private readonly DishService _sut;

    public DishServiceTests()
    {
        _sut = new DishService(_repoMock.Object, _categoryRepoMock.Object, _restaurantRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllDishes()
    {
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dish>
        {
            new() { Id = 1, Name = "Pizza" },
            new() { Id = 2, Name = "Pasta" }
        });

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsDish()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Dish { Id = 1, Name = "Pizza", Price = 40m });

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Pizza");
        result.Price.Should().Be(40m);
    }

    [Fact]
    public async Task GetById_NonExistingId_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Dish?)null);

        await _sut.Invoking(s => s.GetByIdAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedDish()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Category { Id = 3 });
        _restaurantRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Restaurant { Id = 2 });
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => { d.Id = 1; return d; });

        var dto = new CreateDishDto { Name = "Pizza", Price = 40m, CategoryId = 3, RestaurantId = 2 };
        var result = await _sut.CreateAsync(dto);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Pizza");
        result.CategoryId.Should().Be(3);
        result.RestaurantId.Should().Be(2);
    }

    [Fact]
    public async Task Create_InvalidCategoryId_ThrowsNotFoundException()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        var dto = new CreateDishDto { Name = "Pizza", Price = 40m, CategoryId = 99, RestaurantId = 2 };
        await _sut.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_InvalidRestaurantId_ThrowsNotFoundException()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Category { Id = 3 });
        _restaurantRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Restaurant?)null);

        var dto = new CreateDishDto { Name = "Pizza", Price = 40m, CategoryId = 3, RestaurantId = 99 };
        await _sut.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_ExistingId_UpdatesFields()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Dish { Id = 1, Name = "Old", Price = 10m, CategoryId = 3 });
        _categoryRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Category { Id = 3 });

        var dto = new UpdateDishDto { Name = "New", Price = 55m, CategoryId = 3, IsAvailable = false };
        var result = await _sut.UpdateAsync(1, dto);

        result.Name.Should().Be("New");
        result.Price.Should().Be(55m);
        result.IsAvailable.Should().BeFalse();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Dish>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ExistingId_CallsRepository()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Dish { Id = 1 });

        await _sut.DeleteAsync(1);

        _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistingId_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Dish?)null);

        await _sut.Invoking(s => s.DeleteAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }
}
