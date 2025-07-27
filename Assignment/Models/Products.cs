using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class Products
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(500)]
        public string Name { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Description { get; set; }
        [Required]
        [StringLength(600)]
        public string Path { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Price { get; set; }
        [Required]
        [Range(0, long.MaxValue)]
        public long Stock { get; set; }
        [Required]
        [Range(0, long.MaxValue)]
        [DefaultValue(0)]
        public long Sold { get; set; }
        [Required]
        [Range(0, long.MaxValue)]
        public long Discount { get; set; }
        [Required]
        public bool IsPublish { get; set; }
        [Required]
        [StringLength(1000)]
        public string ProductImageUrl { get; set; }
        [Required]
        [Range(0, long.MaxValue)]
        public long PreparationTime { get; set; }
        [Required]
        [Range(0, long.MaxValue)]
        public long Calories { get; set; }
        [Required]
        [StringLength(1000)]
        public string Ingredients { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool IsSpicy { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool IsVegetarian { get; set; }
        [Required]
        [Range(0, long.MaxValue)]
        public long TotalEvaluate { get; set; } = 0;
        [Required]
        [Range(0, 5)]
        public double AverageEvaluate { get; set; } = 0;
        [Required]
        [DefaultValue("")]
        public string MetaTag { get; set; } = "";
        [Required]
        [DefaultValue(0)]
        [Range(-180, 180)]
        public double Longitude { get; set; } = 0; // Kinh độ
        [Required]
        [DefaultValue(0)]
        [Range(-90, 90)]
        public double Latitude { get; set; } = 0; // Vĩ độ
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public virtual Files ProductImage { get; set; }
        public string ProductImageId { get; set; }
        public virtual Categories Category { get; set; }
        public long CategoryId { get; set; }
        public virtual List<Orders> Orders { get; set; }
        public virtual List<Evaluates> Evaluates { get; set; }
    }
}
