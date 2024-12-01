﻿namespace CarMate.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public string AdminName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
