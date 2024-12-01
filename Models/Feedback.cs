namespace CarMate.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ServiceRequestId { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public User? User { get; set; }
        public ServiceRequest? ServiceRequest { get; set; }
    }

}
