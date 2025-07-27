namespace Assignment.Models
{
    public class OrderBase
    {
        public string Name { get; set; }
        public string? Email { get; set; }
        public string Phone { get; set; }
        public List<OrderWithQuantity> Items { get; set; }
    }
}
