using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class VoucherBase
    {
        public bool AutoGeneratorCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public VoucherTypeEnum Type { get; set; }
        public long? UserId { get; set; }
        public double Discount { get; set; }
        public DiscountTypeEnum DiscountType { get; set; }
        public long Quantity { get; set; } = 0;
        public DateTime StartTime { get; set; }
        public bool IsLifeTime { get; set; } = false;
        public DateTime? EndTime { get; set; }
        public double MinimumRequirements { get; set; }
        public bool UnlimitedPercentageDiscount { get; set; }
        public double? MaximumPercentageReduction { get; set; }
    }
}
