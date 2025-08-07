using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Assignment.Models
{
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        public long ChatRoomId { get; set; }
        [Required]
        [StringLength(5000)]
        [DefaultValue("")]
        public string Message { get; set; } = string.Empty;
        [Required]
        [DefaultValue(true)]
        public bool IsFromUser { get; set; } = true;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [JsonIgnore]
        public virtual ChatRoom ChatRoom { get; set; }
    }
}
