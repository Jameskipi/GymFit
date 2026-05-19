using System.ComponentModel.DataAnnotations;

namespace GymFit.Models
{
    public enum UserRole
    {
        Client,
        Trainer,
        Administrator
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Client;

        public bool IsBlocked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual TrainerProfile? TrainerProfile { get; set; }
        public virtual ICollection<MembershipClient> PurchasedMemberships { get; set; } = new List<MembershipClient>();
        public virtual ICollection<GroupActivityReservation> GroupActivityReservations { get; set; } = new List<GroupActivityReservation>();
    }
}
