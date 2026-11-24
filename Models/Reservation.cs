namespace BeachSAWeb.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public int AccommodationDays { get; set; }
        public double Discount { get; set; }
        public double IVA { get; set; }
        public string PaymentMethod { get; set; }
        public string? CheckNumber { get; set; }
        public string? Bank { get; set; }
        public int GuestQuantity { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public User? User { get; set; }
        public Package? Package { get; set; }
        public Invoice? Invoice { get; set; }
    }
}
