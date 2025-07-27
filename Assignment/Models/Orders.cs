using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Assignment.Enum;

namespace Assignment.Models
{
    public class Orders
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [StringLength(200)]
        public string? Email { get; set; }
        [Required]
        [StringLength(11)]
        public string Phone { get; set; }
        [Required]
        [StringLength(1000)]
        public string Address { get; set; }
        [Required]
        [StringLength(100)]
        public string Longitude { get; set; } // Kinh độ
        [Required]
        [StringLength(100)]
        public string Latitude { get; set; } // Vĩ độ
        [Required]
        [Range(0, double.MaxValue)]
        public double Distance { get; set; }
        [Required]
        [Range(0, long.MaxValue)]
        public long TotalQuantity { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double TotalPrice { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Fee { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double FeeExcludingTax { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Vat { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Tax { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double TotalBill { get; set; }
        [Required]
        public OrderStatus Status { get; set; }
        [Required]
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
        public virtual Users User { get; set; }
        public long? UserId { get; set; }
        public virtual List<OrderDetail> OrderDetails { get; set; }
    }
}
