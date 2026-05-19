using GymFit.Models;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}

        public DbSet<User> Users { get; set; }
        public DbSet<TrainerProfile> TrainerProfiles { get; set; }
        public DbSet<MembershipOffer> MembershipOffers { get; set; }
        public DbSet<MembershipClient> ClientMemberships { get; set; }
        public DbSet<GroupActivity> GroupActivities { get; set; }
        public DbSet<GroupActivityReservation> GroupActivityReservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 1-to-1 relationship between User and TrainerProfile
            modelBuilder.Entity<TrainerProfile>()
                .HasKey(t => t.UserId);

            modelBuilder.Entity<TrainerProfile>()
                .HasOne(t => t.User)
                .WithOne(u => u.TrainerProfile)
                .HasForeignKey<TrainerProfile>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-Many configuration via GroupActivityReservation entity
            modelBuilder.Entity<GroupActivityReservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.GroupActivityReservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupActivity>()
                .HasMany(g => g.Reservations)
                .WithOne(r => r.GroupActivity)
                .HasForeignKey(r => r.GroupActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Convert decimal to double automatically
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.GetProperties()
                        .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

                    foreach (var property in properties)
                    {
                        property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<decimal, double>(
                            v => (double)v,
                            v => (decimal)v));
                    }
                }
            }
        }
    }
}