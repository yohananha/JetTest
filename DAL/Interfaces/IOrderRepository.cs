using JetTest.Models;
using JetTest.Models.Enums;

namespace JetTest.DAL.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
    Task<IEnumerable<OrderStatusHistory>> GetOrderHistoryAsync(int orderId);
}
