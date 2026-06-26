using JetTest.BL.Exceptions;
using JetTest.BL.Interfaces;
using JetTest.DAL.Interfaces;
using JetTest.DTOs.Customers;
using JetTest.Models;

namespace JetTest.BL.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ICustomerRepository repo, ILogger<CustomerService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerResponseDto>> GetAllAsync()
    {
        var customers = await _repo.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} customers", customers.Count());
        return customers.Select(MapToDto);
    }

    public async Task<CustomerResponseDto> GetByIdAsync(int id)
    {
        var customer = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Customer {id} not found");
        return MapToDto(customer);
    }

    public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address
        };

        var created = await _repo.CreateAsync(customer);
        _logger.LogInformation("Created customer {Id} ({Email})", created.Id, created.Email);
        return MapToDto(created);
    }

    public async Task<CustomerResponseDto> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Customer {id} not found");

        customer.Name = dto.Name;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;

        await _repo.UpdateAsync(customer);
        _logger.LogInformation("Updated customer {Id}", id);
        return MapToDto(customer);
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Customer {id} not found");

        await _repo.DeleteAsync(id);
        _logger.LogInformation("Deleted customer {Id}", id);
    }

    private static CustomerResponseDto MapToDto(Customer c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Email = c.Email,
        Phone = c.Phone,
        Address = c.Address,
        CreatedAt = c.CreatedAt
    };
}
