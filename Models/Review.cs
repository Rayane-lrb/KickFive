using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KickFive.Models
{
    public class Review
    {

        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Review")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        [Required]
        public DateTime DeletedAt = DateTime.MaxValue;

    }
}
