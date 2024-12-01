using Microsoft.AspNetCore.Identity;

namespace CarMate.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PasswordHash { get; set; }
        public string? VehicleModel { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsEmailVerified { get; set; }=false;
        public string? ResetToken { get; set; } 
        public DateTime? ResetTokenExpiry { get; set; }
        public int RoleId { get; set; } 
        public Role? Role { get; set; }
    }
}
