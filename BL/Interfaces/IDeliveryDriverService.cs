using JetTest.DTOs.DeliveryDrivers;
using JetTest.Models;
using JetTest.Models.Enums;

namespace JetTest.BL.Interfaces;

public interface IDeliveryDriverService
{
    Task<IEnumerable<DeliveryDriverResponseDto>> GetAllAsync();
    Task<DeliveryDriverResponseDto> GetByIdAsync(int id);
    Task<DeliveryDriverResponseDto> CreateAsync(CreateDeliveryDriverDto dto);
    Task<DeliveryDriverResponseDto> UpdateAsync(int id, UpdateDeliveryDriverDto dto);
    Task DeleteAsync(int id);
    Task<DeliveryDriverResponseDto> UpdateStatusAsync(int id, DriverStatus newStatus);
    Task<IEnumerable<DriverStatusHistory>> GetHistoryAsync(int id);
}
