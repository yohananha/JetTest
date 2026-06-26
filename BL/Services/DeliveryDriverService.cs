using JetTest.BL.Exceptions;
using JetTest.BL.Interfaces;
using JetTest.DAL.Interfaces;
using JetTest.Data;
using JetTest.DTOs.DeliveryDrivers;
using JetTest.Models;
using JetTest.Models.Enums;

namespace JetTest.BL.Services;

public class DeliveryDriverService : IDeliveryDriverService
{
    private readonly IDeliveryDriverRepository _repo;
    private readonly AppDbContext _context;
    private readonly ILogger<DeliveryDriverService> _logger;

    public DeliveryDriverService(IDeliveryDriverRepository repo, AppDbContext context, ILogger<DeliveryDriverService> logger)
    {
        _repo = repo;
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DeliveryDriverResponseDto>> GetAllAsync()
    {
        var drivers = await _repo.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} drivers", drivers.Count());
        return drivers.Select(MapToDto);
    }

    public async Task<DeliveryDriverResponseDto> GetByIdAsync(int id)
    {
        var driver = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Driver {id} not found");
        return MapToDto(driver);
    }

    public async Task<DeliveryDriverResponseDto> CreateAsync(CreateDeliveryDriverDto dto)
    {
        var driver = new DeliveryDriver
        {
            Name = dto.Name,
            Phone = dto.Phone,
            VehicleType = dto.VehicleType
        };

        var created = await _repo.CreateAsync(driver);
        _logger.LogInformation("Created driver {Id} ({Name})", created.Id, created.Name);
        return MapToDto(created);
    }

    public async Task<DeliveryDriverResponseDto> UpdateAsync(int id, UpdateDeliveryDriverDto dto)
    {
        var driver = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Driver {id} not found");

        driver.Name = dto.Name;
        driver.Phone = dto.Phone;
        driver.VehicleType = dto.VehicleType;

        await _repo.UpdateAsync(driver);
        _logger.LogInformation("Updated driver {Id}", id);
        return MapToDto(driver);
    }

    public async Task DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Driver {id} not found");

        await _repo.DeleteAsync(id);
        _logger.LogInformation("Deleted driver {Id}", id);
    }

    public async Task<DeliveryDriverResponseDto> UpdateStatusAsync(int id, DriverStatus newStatus)
    {
        var driver = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Driver {id} not found");

        if (driver.Status == newStatus)
            throw new InvalidOperationException($"Driver {id} is already in status {newStatus}");

        var history = new DriverStatusHistory
        {
            DriverId = id,
            OldStatus = driver.Status,
            NewStatus = newStatus,
            ChangedAt = DateTime.UtcNow
        };

        driver.Status = newStatus;

        await _context.DriverStatusHistories.AddAsync(history);
        await _repo.UpdateAsync(driver);

        _logger.LogInformation("Driver {Id} status changed from {Old} to {New}", id, history.OldStatus, newStatus);
        return MapToDto(driver);
    }

    public async Task<IEnumerable<DriverStatusHistory>> GetHistoryAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Driver {id} not found");

        return await _repo.GetDriverHistoryAsync(id);
    }

    private static DeliveryDriverResponseDto MapToDto(DeliveryDriver d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Phone = d.Phone,
        VehicleType = d.VehicleType,
        Status = d.Status,
        CreatedAt = d.CreatedAt
    };
}
