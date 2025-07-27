using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class Vouchers
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Code { get; set; }
        [Required]
        [StringLength(300)]
        public string Name { get; set; }
        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
        [Required]
        public VoucherTypeEnum Type { get; set; }
        public long? UserId { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Discount { get; set; }
        [Required]
        public DiscountTypeEnum DiscountType { get; set; }
        [Required]
        [Range(0, long.MaxValue)]
        [DefaultValue(0)]
        public long Used { get; set; } = 0;
        [Required]
        [Range(0, long.MaxValue)]
        [DefaultValue(0)]
        public long Quantity { get; set; } = 0;
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool IsLifeTime { get; set; } = false;
        public DateTime? EndTime { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double MinimumRequirements { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool UnlimitedPercentageDiscount { get; set; } = false;
        [Range(0, double.MaxValue)]
        public double? MaximumPercentageReduction { get; set; }
        [Required]
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
        public virtual Users? User { get; set; }
    }
}
