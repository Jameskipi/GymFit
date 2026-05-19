using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymFit.Models
{
    public class TrainerProfile
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [StringLength(500)]
        public string Biography { get; set; } = string.Empty;

        public string Specializations { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public virtual ICollection<GroupActivity> ConductedActivities { get; set; } = new List<GroupActivity>();
    }
}
