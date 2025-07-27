using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class OrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double PricePreItems { get; set; }
        [Required]
        [Range(0, long.MaxValue)]
        public long TotalQuantityPreItems { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double TotalPricePreItems { get; set; }
        public virtual Orders Order { get; set; }
        public virtual Products Product { get; set; }
    }
}
