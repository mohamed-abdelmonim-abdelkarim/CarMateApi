using CarMate.Data;
using CarMate.Dtos;
using CarMate.Models;
using CarMate.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CarMate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly VerificationService _verificationService;
        private readonly EmailService _emailService;

        public AuthenticationController(ApplicationDbContext context, IConfiguration config, VerificationService verificationService,EmailService emailService)
        {
            _context = context;
            _config = config;
            _verificationService = verificationService;
            _emailService = emailService;
        }
        //registter user
        [HttpPost("register/user")]
        public async Task<IActionResult> RegisterUser(UserRegistrationDto dto)
        {
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                VehicleModel = dto.VehicleModel,
                VehiclePlateNumber = dto.VehiclePlateNumber,
                RoleId = 1,
                IsEmailVerified = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _verificationService.SendVerificationCode(dto.Email);

            return Ok(new { Message = "User registered successfully And Verification email sent.!" });
        }
        [HttpPost("register/company")]
        public async Task<IActionResult> RegisterCompany(CompanyRegistrationDto dto)
        {
            var company = new Company
            {
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = 2,
                IsEmailVerified = false
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            await _verificationService.SendVerificationCode(dto.Email);
            return Ok(new { Message = "Company registered successfully And Verification email sent.!" });
        }
        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin(AdminRegistrationDto dto)
        {
            var admin = new Admin
            {
                AdminName = dto.AdminName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = 3 
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Admin registered successfully!" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(u => u.Email == dto.Email);

            if (user != null && VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Ok(new { Token = GenerateJwtToken(user.Role.Name, user.Id) });
            }
            var company = await _context.Companies
                .Include(c => c.Role)
                .SingleOrDefaultAsync(c => c.Email == dto.Email);

            if (company != null && VerifyPassword(dto.Password, company.Password))
            {
                return Ok(new { Token = GenerateJwtToken(company.Role.Name, company.Id) });
            }
            var admin = await _context.Admins
                .Include(a => a.Role)
                .SingleOrDefaultAsync(a => a.Email == dto.Email);

            if (admin != null && VerifyPassword(dto.Password, admin.PasswordHash))
            {
                return Ok(new { Token = GenerateJwtToken(admin.Role.Name, admin.Id) });
            }
            return Unauthorized(new { Message = "Invalid credentials!" });
        }
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyCodeDto dto)
        {
            var result = await _verificationService.VerifyCode(dto.Email, dto.Code);

            if (!result)
            {
                return BadRequest(new { Message = "Invalid or expired verification code." });
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user != null)
            {
                user.IsEmailVerified = true;
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (company != null)
            {
                company.IsEmailVerified = true;
            }
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Email verified successfully!" });
        }
        private bool VerifyPassword(string inputPassword, string storedPasswordHash)
        {
            if (!storedPasswordHash.StartsWith("$2"))
            {
                return inputPassword == storedPasswordHash;
            }
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedPasswordHash);
        }

        private string GenerateJwtToken(string role, int entityId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim("UserId", entityId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        [HttpPost("company-login")]
        public async Task<IActionResult> CompanyLogin([FromBody] CompanyLoginDto model)
        {
            var company = await _context.Companies
                .Include(c => c.Role)  
                .FirstOrDefaultAsync(c => c.Email == model.Email && c.Password == model.Password); 
                    if (company == null)
                    {
                        return Unauthorized("Invalid credentials.");
                    }
                        var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, company.CompanyName),
                                new Claim(ClaimTypes.Role, company.Role.Name)  
                            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSecretKeyHere"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "yourIssuer",
                audience: "yourAudience",
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { Token = tokenString });
        }
        //verfication 
        [HttpPost("send-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] SendCodeRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { Message = "Email is required." });
            }

            await _verificationService.SendVerificationCode(request.Email);
            return Ok(new { Message = "Verification code sent successfully." });
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Email and code are required.");

            var isVerified = await _verificationService.VerifyCode(request.Email, request.Code);
            if (!isVerified)
                return BadRequest(new { Message = "Invalid or expired verification code." });

            return Ok(new { Message = "Email verified successfully." });
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
           
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest("Email address not found.");
            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            var resetLink = $"{_config["AppSettings:BaseUrl"]}/reset-password?token={user.ResetToken}&email={user.Email}";
            var emailBody = $"To reset your password, please click the link: {resetLink}";
            await _emailService.SendEmailAsync(user.Email, "Reset Password", emailBody);

            return Ok("Reset password link sent.");
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.ResetToken == request.Token && u.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
                return BadRequest("Invalid or expired token.");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetToken = null; 
            user.ResetTokenExpiry = null;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok("Password reset successfully.");
        }
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var claims = User.Claims;
            foreach (var claim in claims)
            {
                Console.WriteLine($"Claim type: {claim.Type}, Claim value: {claim.Value}");
            }

            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null)
            {
                return Unauthorized("UserId claim is missing.");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return BadRequest("Invalid UserId claim.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                user.Name,
                user.Email,
                user.PhoneNumber
            });
        }

        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return Unauthorized("UserId claim is missing or invalid.");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return BadRequest("Invalid UserId claim format.");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }
            user.Name = request.Name;
            user.PhoneNumber = request.PhoneNumber;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Profile updated successfully.",
                user.Name,
                user.PhoneNumber
            });
        }




    }
}
