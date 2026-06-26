using JetTest.DAL.Interfaces;
using JetTest.Data;
using JetTest.Models;
using JetTest.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace JetTest.DAL.Repositories;

public class DeliveryDriverRepository : Repository<DeliveryDriver>, IDeliveryDriverRepository
{
    public DeliveryDriverRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<DeliveryDriver>> GetByStatusAsync(DriverStatus status) =>
        await _context.DeliveryDrivers.Where(d => d.Status == status).ToListAsync();

    public async Task<IEnumerable<DriverStatusHistory>> GetDriverHistoryAsync(int driverId) =>
        await _context.DriverStatusHistories
            .Where(h => h.DriverId == driverId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
}
