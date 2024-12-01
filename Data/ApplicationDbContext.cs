using CarMate.Models;
using Microsoft.EntityFrameworkCore;

namespace CarMate.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<PaymentOption> PaymentOptions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<FuelStation> FuelStations { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<Role>().HasData(
              new Role { Id = 1, Name = "User" },
              new Role { Id = 2, Name = "Company" },
              new Role { Id = 3, Name = "Admin" }
                                            );
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.User)
                .WithMany()
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.NoAction); 
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction); 
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.ServiceRequest)
                .WithMany()
                .HasForeignKey(f => f.ServiceRequestId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.FuelStation)
                .WithMany(fs => fs.ServiceRequests)
                .HasForeignKey(sr => sr.FuelStationId);
        }





    }
}
