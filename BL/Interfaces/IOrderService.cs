using JetTest.DTOs.Orders;
using JetTest.Models;
using JetTest.Models.Enums;

namespace JetTest.BL.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderResponseDto>> GetAllAsync();
    Task<OrderResponseDto> GetByIdAsync(int id);
    Task<OrderResponseDto> CreateAsync(CreateOrderDto dto);
    Task<OrderResponseDto> UpdateAsync(int id, UpdateOrderDto dto);
    Task DeleteAsync(int id);
    Task<OrderResponseDto> UpdateStatusAsync(int id, OrderStatus newStatus, string? notes = null);
    Task<IEnumerable<OrderStatusHistory>> GetHistoryAsync(int id);
}
