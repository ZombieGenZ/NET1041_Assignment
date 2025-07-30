using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    public class VerifyAccount
    {
        [Key]
        public string Token { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public long UserId { get; set; }
        public DateTime ExpirationTime { get; set; } = DateTime.Now.AddMinutes(30);
        public virtual Users User { get; set; }
    }
}
