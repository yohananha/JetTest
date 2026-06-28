using JetTest.DTOs.Reports;

namespace JetTest.BL.Interfaces;

public interface IReportService
{
    Task<IEnumerable<CategoryTopDishesDto>> GetTopDishesPerCategoryAsync(int? restaurantId, int topN);
    Task<IEnumerable<RestaurantDeliveryEfficiencyDto>> GetDeliveryEfficiencyAsync(int? restaurantId);
}
