using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class Categories
    {
        [Key]
        public long Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        [DeniedValues("")]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Range(1, long.MaxValue)]
        public long Index { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public virtual List<Products> Products { get; set; }
    }
}
