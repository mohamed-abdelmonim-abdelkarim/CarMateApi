using CarMate.Data;
using CarMate.Models;
using Microsoft.AspNetCore.Identity;

namespace CarMate.SeedRole
{
    public static class DbSeeder
    {
      
        public static void SeedCompanies(ApplicationDbContext context)
        {
            if (!context.Companies.Any())
            {
                var role = context.Roles.FirstOrDefault(r => r.Name == "Company");

                context.Companies.Add(new Company
                {
                    CompanyName = "Test Company",
                    Email = "company@test.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),  // تأكد من تخزين كلمة المرور بشكل آمن
                    RoleId = role?.Id ?? 0,
                    Location = "Cairo"
                });

                context.SaveChanges();
            }
        }
        public static void SeedUsers(ApplicationDbContext context)
        {
            if (!context.Users.Any())
            {
                var userRole = context.Roles.FirstOrDefault(r => r.Name == "User");
                context.Users.Add(new User { Name = "user1", Email = "user1@example.com", RoleId = userRole.Id });
                context.SaveChanges();
            }
        }
        
    }



}
