using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class Redeems
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(300)]
        public string Name { get; set; }
        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Discount { get; set; }
        [Required]
        public DiscountTypeEnum DiscountType { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool IsLifeTime { get; set; } = false;
        public string? EndTime { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double MinimumRequirements { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool UnlimitedPercentageDiscount { get; set; } = false;
        [Range(0, double.MaxValue)]
        public double? MaximumPercentageReduction { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Price { get; set; }
        [Required]
        public UserRankEnum RankRequirement { get; set; }
        [Required]
        public bool IsPublish { get; set; }
        [Required]
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
    }
}
