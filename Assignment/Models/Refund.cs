using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class Refund
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        public long OrderId { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Amount { get; set; }
        [Required]
        [StringLength(5000)]
        public string Reason { get; set; }
        [Required]
        [DefaultValue(RefundStatusEnum.Pending)]
        public RefundStatusEnum Status { get; set; } = RefundStatusEnum.Pending;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public virtual Orders Order { get; set; }
    }
}
