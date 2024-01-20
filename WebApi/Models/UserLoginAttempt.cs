using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class UserLoginAttempt
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(36)]
        public string? UserId { get; set; }

        [Required]
        public DateTime Datetime { get; set; }
    }
}
