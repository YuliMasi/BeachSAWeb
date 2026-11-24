namespace BeachSAWeb.Models
{
    public class InvoiceDetail
    {
        public bool EmailSent { get; set; }
        public int InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public InvoiceCustomer Customer { get; set; }
        public InvoiceReservation Reservation { get; set; }
        public InvoiceAmounts Details { get; set; }
    }

    public class InvoiceCustomer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string IdType { get; set; }
        public string Email { get; set; }
        public int PhoneNumber { get; set; }
    }

    public class InvoiceReservation
    {
        public int Id { get; set; }
        public string PackageType { get; set; }
        public int GuestQuantity { get; set; }
        public int AccommodationDays { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string PaymentMethod { get; set; }
        public string CheckNumber { get; set; }
        public string Bank { get; set; }
    }

    public class InvoiceAmounts
    {
        public double Subtotal { get; set; }
        public double Discount { get; set; }
        public double DiscountAmount { get; set; }
        public double Iva { get; set; }
        public double IvaAmount { get; set; }
        public double ExchangeRate { get; set; }
        public double TotalUsd { get; set; }
        public double TotalColones { get; set; }
    }
}
