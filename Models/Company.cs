namespace CarMate.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Location { get; set; }
        public int RoleId { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public Role? Role { get; set; }

    }
}
