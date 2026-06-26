using FluentAssertions;
using JetTest.BL.Exceptions;
using JetTest.BL.Services;
using JetTest.DAL.Interfaces;
using JetTest.DTOs.Customers;
using JetTest.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace JetTest.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repoMock = new();
    private readonly Mock<ILogger<CustomerService>> _loggerMock = new();
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _sut = new CustomerService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllCustomers()
    {
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "Alice", Email = "alice@test.com", Phone = "111", Address = "Addr1" },
            new() { Id = 2, Name = "Bob", Email = "bob@test.com", Phone = "222", Address = "Addr2" }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsCustomer()
    {
        var customer = new Customer { Id = 1, Name = "Alice", Email = "alice@test.com", Phone = "111", Address = "Addr" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetById_NonExistingId_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Customer?)null);

        await _sut.Invoking(s => s.GetByIdAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedCustomer()
    {
        var dto = new CreateCustomerDto { Name = "Alice", Email = "alice@test.com", Phone = "111", Address = "Addr" };
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) => { c.Id = 1; return c; });

        var result = await _sut.CreateAsync(dto);

        result.Id.Should().Be(1);
        result.Email.Should().Be("alice@test.com");
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task Update_ExistingId_UpdatesFields()
    {
        var customer = new Customer { Id = 1, Name = "Old", Email = "a@test.com", Phone = "111", Address = "Old Addr" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

        var dto = new UpdateCustomerDto { Name = "New", Phone = "999", Address = "New Addr" };
        var result = await _sut.UpdateAsync(1, dto);

        result.Name.Should().Be("New");
        result.Phone.Should().Be("999");
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task Update_NonExistingId_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Customer?)null);

        await _sut.Invoking(s => s.UpdateAsync(99, new UpdateCustomerDto()))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_ExistingId_CallsRepository()
    {
        var customer = new Customer { Id = 1, Name = "Alice", Email = "a@test.com", Phone = "111", Address = "Addr" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

        await _sut.DeleteAsync(1);

        _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistingId_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Customer?)null);

        await _sut.Invoking(s => s.DeleteAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }
}
