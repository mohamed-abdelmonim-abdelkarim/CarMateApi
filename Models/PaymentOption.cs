namespace CarMate.Models
{
    public class PaymentOption
    {
        public int Id { get; set; }
        public string Method { get; set; } // "Cash", "Credit Card", "PayPal"
        public bool IsAvailable { get; set; } = true; // لتحديد ما إذا كان الخيار متاحًا أو لا
    }

}
