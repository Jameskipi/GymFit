using System.ComponentModel.DataAnnotations;

namespace GymFit.Models
{
    public class MembershipOffer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int ValidityDays { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
