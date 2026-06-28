namespace JetTest.DTOs.Reports;

public class RestaurantDeliveryEfficiencyDto
{
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public int DeliveredOrdersCount { get; set; }
    public double AverageOverallMinutes { get; set; }
    public List<StatusTransitionAvgDto> TransitionAverages { get; set; } = new();
}
