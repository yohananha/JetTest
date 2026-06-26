using JetTest.DAL.Interfaces;
using JetTest.Data;
using JetTest.Models;
using JetTest.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace JetTest.DAL.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId) =>
        await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status) =>
        await _context.Orders
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<OrderStatusHistory>> GetOrderHistoryAsync(int orderId) =>
        await _context.OrderStatusHistories
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
}
