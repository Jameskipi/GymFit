using System.ComponentModel.DataAnnotations;

namespace GymFit.Models
{
    public class GroupActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public int DurationInMinutes { get; set; }

        [Required]
        public int CapacityLimit { get; set; }

        [Required]
        public int TrainerId { get; set; }
        public virtual TrainerProfile Trainer { get; set; } = null!;

        public virtual ICollection<GroupActivityReservation> Reservations { get; set; } = new List<GroupActivityReservation>();
    }
}
