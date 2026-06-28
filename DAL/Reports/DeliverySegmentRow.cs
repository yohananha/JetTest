namespace JetTest.DAL.Reports;

// One row per (restaurant × transition) returned by the delivery-efficiency raw SQL query.
// AvgOverallMinutes and DeliveredCount are at the restaurant level and repeat on every
// transition row for that restaurant — the service reads them from the first row of each group.
public class DeliverySegmentRow
{
    public int    RestaurantId       { get; set; }
    public string RestaurantName     { get; set; } = string.Empty;
    public int    OldStatus          { get; set; }   // stored as int; cast to OrderStatus in service
    public int    NewStatus          { get; set; }
    public double AvgSegmentMinutes  { get; set; }   // avg time spent in this transition, per restaurant
    public double AvgOverallMinutes  { get; set; }   // avg Placed→Delivered time, per restaurant
    public int    DeliveredCount     { get; set; }   // delivered orders at this restaurant
}
