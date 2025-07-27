using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class UserRegisterBody
    {
        public string? GoogleId { get; set; }
        public string? FacebookId { get; set; }
        public string? GithubId { get; set; }
        public string? DiscordId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
