namespace CarMate.Models
{
      public class FuelStation
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string? Address { get; set; }
            public string? ContactNumber { get; set; }
            public ICollection<ServiceRequest>? ServiceRequests { get; set; }
        }
    
}
