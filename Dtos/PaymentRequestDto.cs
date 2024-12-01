namespace CarMate.Dtos
{
    public class PaymentRequestDto
    {
        public int ServiceRequestId { get; set; }
        public string Method { get; set; } // "Cash", "Credit Card", "PayPal"
    }

}
