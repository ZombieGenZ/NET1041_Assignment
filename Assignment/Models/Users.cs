using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [Required]
        [StringLength(200)]
        public string Email { get; set; }
        [Required]
        [StringLength(11)]
        public string Phone { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string? Password { get; set; } = null;
        [Required]
        [StringLength(200)]
        public string Role { get; set; } = "Customer";
        [Required]
        [DefaultValue(0)]
        public double TotalAccumulatedPoints { get; set; } = 0;
        [Required]
        [DefaultValue(0)]
        public double AccumulatedPoints { get; set; } = 0;
        [Required]
        [DefaultValue(UserRankEnum.None)]
        public UserRankEnum Rank { get; set; } = UserRankEnum.None;
        [Required]
        [DefaultValue(UserTypeEnum.UnVerified)]
        public UserTypeEnum UserType { get; set; } = UserTypeEnum.UnVerified;
        [StringLength(500)]
        public string? GoogleId { get; set; } = null;
        [StringLength(500)]
        public string? FacebookId { get; set; } = null;
        [StringLength(500)]
        public string? GitHubId { get; set; } = null;
        [StringLength(500)]
        public string? DiscordId { get; set; } = null;
        [StringLength(50)]
        [DefaultValue("Default")]
        public string MainProvider { get; set; } = "Default";
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public virtual List<Vouchers> Vouchers { get; set; }
    }
}
