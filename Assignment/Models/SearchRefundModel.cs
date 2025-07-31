namespace Assignment.Models
{
    public class SearchRefundModel
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<Refund> Data { get; set; }
    }
}
