using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Eventing.Reader;

namespace Assignment.Models
{
    public class ForgotPassword
    {
        [Key]
        public string Token { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public long UserId { get; set; }
        public DateTime ExpirationTime { get; set; } = DateTime.Now.AddMinutes(30);
        public virtual Users User { get; set; }
    }
}
