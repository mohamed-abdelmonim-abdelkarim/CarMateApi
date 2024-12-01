using CarMate.Data;
using CarMate.Models;
using Microsoft.EntityFrameworkCore;

namespace CarMate.Services
{
    public class VerificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public VerificationService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task SendVerificationCode(string email)
        {
            var verificationCode = GenerateVerificationCode();
            var existingVerification = await _context.EmailVerifications
                .FirstOrDefaultAsync(v => v.Email == email && !v.IsVerified);

            if (existingVerification != null)
            {
                _context.EmailVerifications.Remove(existingVerification);
            }
            var emailVerification = new EmailVerification
            {
                Email = email,
                VerificationCode = verificationCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10) 
            };

            _context.EmailVerifications.Add(emailVerification);
            await _context.SaveChangesAsync();
            var body = $"Your verification code is: {verificationCode}";
            await _emailService.SendEmailAsync(email, "Email Verification", body);
        }

        public async Task<bool> VerifyCode(string email, string code)
        {
            var verification = await _context.EmailVerifications
                .FirstOrDefaultAsync(v => v.Email == email && v.VerificationCode == code);

            if (verification == null || verification.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            verification.IsVerified = true;
            await _context.SaveChangesAsync();

            return true;
        }
    }

}
