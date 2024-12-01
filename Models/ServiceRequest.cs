using System.Text.Json.Serialization;

namespace CarMate.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? ServiceType { get; set; } // "Road Assist", "Battery", "Tire Change", etc.
        public string? Location { get; set; }
        public string Status { get; set; } = "Pending"; // "Pending", "In Progress", "Completed", etc.
        public string? TrackingLink { get; set; }
        public string? PaymentMethod { get; set; } // "Cash", "Credit Card", "PayPal"
        public decimal EstimatedCost { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public User? User { get; set; }
        public int? FuelStationId { get; set; }
        [JsonIgnore]
        public FuelStation? FuelStation { get; set; }
    }

}
