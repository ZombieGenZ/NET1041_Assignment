namespace Assignment.Models
{
    public class ShipperStatisticalModel
    {
        public string ShipperName { get; set; }
        public long? ShipperId { get; set; }
        public long DeliveringOrders { get; set; }
        public long CompletedOrders { get; set; }
        public double TotalRevenue { get; set; }
        public double DeliveringRevenue { get; set; }
        public ChartData RevenueChart { get; set; }
        public ChartData StatusChart { get; set; }
    }
}
