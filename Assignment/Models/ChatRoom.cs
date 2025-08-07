using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class ChatRoom
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool IsIdentification { get; set; } = false;
        [Required]
        [DefaultValue(false)]
        public bool IsRead { get; set; } = false;
        [StringLength(50)]
        public string? ChatId { get; set; }
        public long? UserId { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public virtual Users? User { get; set; }
        public virtual List<ChatMessage> ChatMessages { get; set; }
    }
}
