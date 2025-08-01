using System.Collections.Generic;

namespace Assignment.Models
{
    public class StatisticalModel
    {
        public double TotalRevenue { get; set; }
        public long TotalProducts { get; set; }
        public long TotalOrders { get; set; }

        public ChartData RevenueChart { get; set; }

        public ChartData CategoryChart { get; set; }

        public List<TopProduct> TopProducts { get; set; }

        public List<TopCustomer> TopCustomers { get; set; }
    }
}
