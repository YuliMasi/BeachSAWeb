namespace BeachSAWeb.Models
{
    /// <summary>
    /// Reservación de un paquete hecha por un cliente.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Identificador de la reservación.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Cédula del cliente que reservó.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Identificador del paquete reservado.
        /// </summary>
        public int PackageId { get; set; }
        /// <summary>
        /// Cantidad de noches de la estadía.
        /// </summary>
        public int AccommodationDays { get; set; }
        /// <summary>
        /// Porcentaje de descuento aplicado, en decimal.
        /// </summary>
        public double Discount { get; set; }
        /// <summary>
        /// Porcentaje de IVA aplicado, en decimal.
        /// </summary>
        public double IVA { get; set; }
        /// <summary>
        /// Forma de pago: Efectivo, Tarjeta o Cheque.
        /// </summary>
        public string PaymentMethod { get; set; }
        /// <summary>
        /// Número del cheque, solo si se paga con cheque.
        /// </summary>
        public string? CheckNumber { get; set; }
        /// <summary>
        /// Banco que emite el cheque, solo si se paga con cheque.
        /// </summary>
        public string? Bank { get; set; }
        /// <summary>
        /// Cantidad de personas.
        /// </summary>
        public int GuestQuantity { get; set; }
        /// <summary>
        /// Fecha de entrada.
        /// </summary>
        public DateTime CheckInDate { get; set; }
        /// <summary>
        /// Fecha de salida.
        /// </summary>
        public DateTime CheckOutDate { get; set; }

        /// <summary>
        /// Cliente de la reservación, cargado para mostrarlo en las vistas.
        /// </summary>
        public User? User { get; set; }
        /// <summary>
        /// Paquete de la reservación, cargado para mostrarlo en las vistas.
        /// </summary>
        public Package? Package { get; set; }
        /// <summary>
        /// Factura de la reservación, cargada para mostrar su total.
        /// </summary>
        public Invoice? Invoice { get; set; }
    }
}
