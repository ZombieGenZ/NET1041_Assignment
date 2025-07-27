using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class Evaluates
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [Range(1, 5)]
        public double Star { get; set; }
        [Required] public string Content { get; set; } = "";
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long UserId { get; set; }
        public long ProductId { get; set; }
        public virtual Users User { get; set; }
        public virtual Products Product { get; set; }
    }
}
