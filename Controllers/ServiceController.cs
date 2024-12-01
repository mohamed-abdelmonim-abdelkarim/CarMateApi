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
    public class ServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("GetClosestFuelStation")]
        [Authorize]
        public async Task<IActionResult> GetClosestFuelStation([FromQuery] double latitude, [FromQuery] double longitude)
        {
            var fuelStations = await _context.FuelStations.ToListAsync();

            if (!fuelStations.Any())
                return NotFound("No fuel stations found.");

            var closestStation = fuelStations
                .Select(station => new
                {
                    Station = station,
                    Distance = CalculateDistance(latitude, longitude, station.Latitude, station.Longitude)
                })
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            if (closestStation == null)
                return NotFound("No fuel stations found near this location.");

            return Ok(new
            {
                FuelStation = closestStation.Station,
                Distance = closestStation.Distance
            });
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            double latDistance = DegreesToRadians(lat2 - lat1);
            double lonDistance = DegreesToRadians(lon2 - lon1);
            double a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        private double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
        [HttpPost("CreateServiceRequest")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateServiceRequest([FromBody] CreateServiceRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token or user not authenticated.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");
            var closestFuelStationResult = await GetClosestFuelStation(requestDto.Latitude, requestDto.Longitude);
            if (closestFuelStationResult is NotFoundObjectResult)
                return closestFuelStationResult; 
            var closestFuelStation = (closestFuelStationResult as OkObjectResult)?.Value as dynamic;
            if (closestFuelStation == null)
                return NotFound("No fuel stations available near this location.");

            var serviceRequest = new ServiceRequest
            {
                UserId = userId,
                ServiceType = requestDto.ServiceType,
                Location = requestDto.Location,
                Status = requestDto.Status,
                PaymentMethod = requestDto.PaymentMethod,
                EstimatedCost = requestDto.EstimatedCost,
                RequestedAt = DateTime.UtcNow,
                TrackingLink = requestDto.TrackingLink ?? "N/A",
                FuelStationId = closestFuelStation.FuelStation.Id 
            };

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();

            return Ok(serviceRequest);
        }


        [Authorize(Roles = "Company,Admin")]
        [HttpGet("GetServiceRequest/{id}")]
        public async Task<IActionResult> GetServiceRequest(int id)
        {
            var serviceRequest = await _context.ServiceRequests
                .Include(sr => sr.User)
                .Include(sr => sr.FuelStation) 
                .FirstOrDefaultAsync(sr => sr.Id == id);

            if (serviceRequest == null)
                return NotFound();

            return Ok(serviceRequest);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> UpdateServiceStatus(int id, [FromBody] string status)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null) return NotFound(new { Message = "Service request not found!" });

            serviceRequest.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Service status updated successfully!" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteServiceRequest(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
                return NotFound(new { Message = "Service request not found!" });

            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Service request deleted successfully!" });
        }
        [HttpGet("GetServiceRequestsByType")]
        [Authorize]
        public async Task<IActionResult> GetServiceRequestsByType([FromQuery] string serviceType)
        {
            var serviceRequests = await _context.ServiceRequests
                .Where(sr => sr.ServiceType == serviceType)
                .Include(sr => sr.User)
                .Include(sr => sr.FuelStation)
                .ToListAsync();

            if (!serviceRequests.Any())
                return NotFound(new { Message = "No service requests found for this service type." });

            return Ok(serviceRequests);
        }
        [HttpPost("AddFuelStation")]
        public async Task<IActionResult> AddFuelStation([FromBody] FuelStation fuelStation)
        {
            if (fuelStation == null)
                return BadRequest("Invalid fuel station data.");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _context.FuelStations.Add(fuelStation);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFuelStation), new { id = fuelStation.Id }, fuelStation);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFuelStation(int id)
        {
            var fuelStation = await _context.FuelStations.FindAsync(id);

            if (fuelStation == null)
                return NotFound();

            return Ok(fuelStation);
        }
    }
}
