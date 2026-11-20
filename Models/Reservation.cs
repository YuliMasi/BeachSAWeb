namespace BeachSAWeb.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int AccommodationDays { get; set; }
        public double Discount { get; set; }
        public double IVA { get; set; }
        public string PaymentType { get; set; }   // Efectivo / Tarjeta / Cheque
        public string CheckNumber { get; set; }   // Solo si paga con cheque
        public string BankName { get; set; }      // Solo si paga con cheque
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public int People { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public User User { get; set; }
        public Package Package { get; set; }
    }
}
