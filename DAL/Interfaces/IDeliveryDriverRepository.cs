using JetTest.Models;
using JetTest.Models.Enums;

namespace JetTest.DAL.Interfaces;

public interface IDeliveryDriverRepository : IRepository<DeliveryDriver>
{
    Task<IEnumerable<DeliveryDriver>> GetByStatusAsync(DriverStatus status);
    Task<IEnumerable<DriverStatusHistory>> GetDriverHistoryAsync(int driverId);
}
