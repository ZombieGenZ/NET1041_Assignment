using Assignment.Enum;

namespace Assignment.Models
{
    public class SearchPurchaseHistoryModel
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public OrderStatus? Status { get; set; }
        public List<Orders> Data { get; set; }
    }
}
