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
        public OrderStatus Status { get; set; }
        [Required]
        [Range(0, long.MaxValue)]
        public long TotalQuantity { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double TotalPrice { get; set; }
        [Required]
        public DateTime OrderTime { get; set; } = DateTime.Now;
        public virtual Users User { get; set; }
        public long? UserId { get; set; }
        public virtual List<OrderDetail> OrderDetails { get; set; }
    }
}
