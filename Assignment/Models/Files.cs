using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    public class Files
    {
        [Key]
        public string FileId { get; set; }
        [Required]
        [StringLength(1000)]
        public string FileName { get; set; }
        [Required]
        [StringLength(1000)]
        public string FileUrl { get; set; }
        [Required]
        [StringLength(100)]
        public string FileType { get; set; }
        [Required]
        [StringLength(1000)]
        public string FileAngleName { get; set; }
        [Required]
        [StringLength(1000)]
        public string FilePath { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double FileSize { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
