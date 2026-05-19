using System.ComponentModel.DataAnnotations;

namespace GymFit.Models
{
    public class GroupActivityReservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        public int GroupActivityId { get; set; }
        public virtual GroupActivity GroupActivity { get; set; } = null!;

        public DateTime BookingDate { get; set; } = DateTime.Now;

        public bool IsPresent { get; set; } = false;
    }
}
