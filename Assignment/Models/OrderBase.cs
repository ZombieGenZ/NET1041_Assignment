using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Assignment.Models
{
    public class OrderBase
    {
        public string Name { get; set; }
        public string? Email { get; set; }
        public string Phone { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Address { get; set; }
        public string? Voucher { get; set; }
        public List<OrderWithQuantity> Items { get; set; }
    }
}
