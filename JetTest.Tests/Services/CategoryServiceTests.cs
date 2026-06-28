using FluentAssertions;
using JetTest.BL.Exceptions;
using JetTest.BL.Services;
using JetTest.DAL.Interfaces;
using JetTest.DTOs.Categories;
using JetTest.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace JetTest.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repoMock = new();
    private readonly Mock<ILogger<CategoryService>> _loggerMock = new();
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllCategories()
    {
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>
        {
            new() { Id = 1, Name = "Mains" },
            new() { Id = 2, Name = "Desserts" }
        });

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsCategory()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { Id = 1, Name = "Mains" });

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Mains");
    }

    [Fact]
    public async Task GetById_NonExistingId_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        await _sut.Invoking(s => s.GetByIdAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedCategory()
    {
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => { c.Id = 1; return c; });

        var result = await _sut.CreateAsync(new CreateCategoryDto { Name = "Mains" });

        result.Id.Should().Be(1);
        result.Name.Should().Be("Mains");
    }

    [Fact]
    public async Task Update_ExistingId_UpdatesName()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { Id = 1, Name = "Old" });

        var result = await _sut.UpdateAsync(1, new UpdateCategoryDto { Name = "New" });

        result.Name.Should().Be("New");
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ExistingId_CallsRepository()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { Id = 1 });

        await _sut.DeleteAsync(1);

        _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistingId_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        await _sut.Invoking(s => s.DeleteAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }
}
