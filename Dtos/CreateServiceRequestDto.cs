namespace CarMate.Dtos
{
    public class CreateServiceRequestDto
    {
        public int UserId { get; set; }
        public string? ServiceType { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = "Pending"; // Default status
        public string? PaymentMethod { get; set; }
        public decimal EstimatedCost { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public string? TrackingLink { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
