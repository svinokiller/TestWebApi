using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class Query
    {
        [Key]
        [Required]
        [MaxLength(36)]
        public Guid QueryId { get; set; }

        [Required]
        public DateTime Start { get; set; }

        public string? Result { get; set; } 

    }
}
