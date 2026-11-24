namespace BeachSAWeb.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public int ReservationId { get; set; }
        public int InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public double Subtotal { get; set; }
        public double Discount { get; set; }
        public double DiscountAmount { get; set; }
        public double IVA { get; set; }
        public double IvaAmount { get; set; }
        public double ExchangeRate { get; set; }
        public double TotalUsd { get; set; }
        public double TotalColones { get; set; }
        public User? User { get; set; }
        public Package? Package { get; set; }
        public Reservation? Reservation { get; set; }
    }
}
