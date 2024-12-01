using CarMate.Data;
using CarMate.Dtos;
using CarMate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarMate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration.GetValue<string>("Stripe:SecretKey");
        }
        [HttpPost("process")]
        [Authorize(Roles ="User")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto paymentRequest)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(paymentRequest.ServiceRequestId);
            if (serviceRequest == null)
                return NotFound(new { Message = "Service request not found!" });

            if (serviceRequest.Status != "Pending")
                return BadRequest(new { Message = "Service request is not in a valid state for payment." });

            if (paymentRequest.Method == "Credit Card")
            {
                try
                {
                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        LineItems = new List<SessionLineItemOptions>
                        {
                            new SessionLineItemOptions
                            {
                                PriceData = new SessionLineItemPriceDataOptions
                                {
                                    Currency = "usd",
                                    UnitAmount = (long)(serviceRequest.EstimatedCost * 100),
                                    ProductData = new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = "Service Payment",
                                    },
                                },
                                Quantity = 1,
                            },
                        },
                        Mode = "payment",
                        SuccessUrl = "https://yourdomain.com/success?session_id={CHECKOUT_SESSION_ID}",
                        CancelUrl = "https://yourdomain.com/cancel",
                    };

                    var service = new SessionService();
                    Session session = service.Create(options);

                    serviceRequest.Status = "Payment Pending";
                    serviceRequest.PaymentMethod = "Credit Card";
                    await _context.SaveChangesAsync();

                    return Ok(new { SessionId = session.Id });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = $"Payment failed: {ex.Message}" });
                }
            }
            else if (paymentRequest.Method == "Cash")
            {
                serviceRequest.Status = "Paid";
                serviceRequest.PaymentMethod = "Cash";
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Payment received in cash!", serviceRequest });
            }
            else
            {
                return BadRequest(new { Message = "Invalid payment method." });
            }
        }
        [HttpGet("all")]
        [Authorize(Roles = "Company,Admin")]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _context.ServiceRequests
                .Where(sr => sr.PaymentMethod != null)
                .ToListAsync();

            return Ok(payments);
        }
        [HttpGet("cash")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> GetCashPayments()
        {
            var cashPayments = await _context.ServiceRequests
                .Where(sr => sr.PaymentMethod == "Cash")
                .ToListAsync();

            return Ok(cashPayments);
        }
        [HttpGet("stripe")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> GetStripePayments()
        {
            var stripePayments = await _context.ServiceRequests
                .Where(sr => sr.PaymentMethod == "Credit Card")
                .ToListAsync();

            return Ok(stripePayments);
        }
        [HttpPut("update/{id}")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] PaymentRequestDto paymentRequest)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
                return NotFound(new { Message = "Service request not found!" });

            if (paymentRequest.Method == "Credit Card")
            {
                try
                {
                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        LineItems = new List<SessionLineItemOptions>
                        {
                            new SessionLineItemOptions
                            {
                                PriceData = new SessionLineItemPriceDataOptions
                                {
                                    Currency = "usd",
                                    UnitAmount = (long)(serviceRequest.EstimatedCost * 100),
                                    ProductData = new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = "Service Payment",
                                    },
                                },
                                Quantity = 1,
                            },
                        },
                        Mode = "payment",
                        SuccessUrl = "https://yourdomain.com/success?session_id={CHECKOUT_SESSION_ID}",
                        CancelUrl = "https://yourdomain.com/cancel",
                    };

                    var service = new SessionService();
                    Session session = service.Create(options);

                    serviceRequest.Status = "Payment Pending";
                    serviceRequest.PaymentMethod = "Credit Card";
                    await _context.SaveChangesAsync();

                    return Ok(new { SessionId = session.Id });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = $"Payment failed: {ex.Message}" });
                }
            }
            else if (paymentRequest.Method == "Cash")
            {
                serviceRequest.Status = "Paid";
                serviceRequest.PaymentMethod = "Cash";
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Payment received in cash!", serviceRequest });
            }
            else
            {
                return BadRequest(new { Message = "Invalid payment method." });
            }
        }
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
                return NotFound(new { Message = "Service request not found!" });

            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Payment deleted successfully." });
        }
    }
}
