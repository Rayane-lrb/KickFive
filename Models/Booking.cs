using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KickFive.Models
{
    public class Booking
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Time")]
        public DateTime StartDateTime { get; set; } = DateTime.MinValue;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "End Time")]
        public DateTime EndDateTime { get; set; } = DateTime.MaxValue;

        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Required]
        [Display(Name = "Price")]
        public decimal Price { get; set; } = 80;

        [Required]
        [ForeignKey("Field")]
        public int FieldId { get; set; }
        public Field Field { get; set; } = null!;


        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        [Required]
        public DateTime DeletedAt = DateTime.MaxValue;

    }
}
