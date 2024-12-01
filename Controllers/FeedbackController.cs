using CarMate.Data;
using CarMate.Dtos;
using CarMate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarMate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("SubmitRating")]
        [Authorize(Roles ="User,Company")]
        public async Task<IActionResult> SubmitRating([FromBody] RatingDto ratingDto)
        {
            var userIdClaim = User?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Invalid token or user not authenticated.");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userIdClaim);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var feedback = new Feedback
            {
                UserId = user.Id,
                ServiceRequestId = ratingDto.ServiceRequestId,
                Rating = ratingDto.Rating,
                Comments = ratingDto.Comments,
                SubmittedAt = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(feedback);
        }
        [HttpGet]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbacks = await _context.Feedbacks.Include(f => f.User).ToListAsync();
            return Ok(feedbacks);
        }
        [HttpPut("UpdateFeedback/{id}")]
        [Authorize(Roles = "User,Company")]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] RatingDto ratingDto)
        {
            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.Id == id);
            if (feedback == null)
            {
                return NotFound("Feedback not found.");
            }
            var userIdClaim = User?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || feedback.UserId.ToString() != userIdClaim)
            {
                return Unauthorized("You cannot update feedback that is not yours.");
            }
            feedback.Rating = ratingDto.Rating;
            feedback.Comments = ratingDto.Comments;
            feedback.SubmittedAt = DateTime.UtcNow;  
            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();

            return Ok(feedback);
        }
        [HttpDelete("DeleteFeedback/{id}")]
        [Authorize(Roles = "User,Company")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.Id == id);
            if (feedback == null)
            {
                return NotFound("Feedback not found.");
            }
            var userIdClaim = User?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || feedback.UserId.ToString() != userIdClaim)
            {
                return Unauthorized("You cannot delete feedback that is not yours.");
            }
            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Feedback deleted successfully." });
        }
    }
}
