using System.ComponentModel.DataAnnotations;

namespace KickFive.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Field
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Field Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime DeletedAt = DateTime.MaxValue;
    }
}
