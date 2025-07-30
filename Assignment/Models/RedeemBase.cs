using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class RedeemBase
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Discount { get; set; }
        public DiscountTypeEnum DiscountType { get; set; }
        public bool IsLifeTime { get; set; } = false;
        public string? EndTime { get; set; }
        public double MinimumRequirements { get; set; }
        public bool UnlimitedPercentageDiscount { get; set; } = false;
        public double? MaximumPercentageReduction { get; set; }
        public double Price { get; set; }
        public UserRankEnum RankRequirement { get; set; }
        public bool IsPublish { get; set; }
    }
}
