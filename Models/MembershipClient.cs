using System.ComponentModel.DataAnnotations;

namespace GymFit.Models
{
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Cancelled,
        Refunded
    }

    public class MembershipClient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        public int MembershipOfferId { get; set; }
        public virtual MembershipOffer MembershipOffer { get; set; } = null!;

        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    }
}
