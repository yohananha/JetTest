using JetTest.Models.Enums;

namespace JetTest.DTOs.Reports;

public class StatusTransitionAvgDto
{
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public double AverageMinutes { get; set; }
}
