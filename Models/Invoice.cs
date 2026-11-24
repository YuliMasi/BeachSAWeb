namespace BeachSAWeb.Models
{
    /// <summary>
    /// Factura generada para una reservación.
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Identificador de la factura.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Cédula del cliente facturado.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Identificador del paquete facturado.
        /// </summary>
        public int PackageId { get; set; }
        /// <summary>
        /// Identificador de la reservación facturada.
        /// </summary>
        public int ReservationId { get; set; }
        /// <summary>
        /// Número consecutivo de la factura.
        /// </summary>
        public int InvoiceNumber { get; set; }
        /// <summary>
        /// Fecha en que se emitió la factura.
        /// </summary>
        public DateTime InvoiceDate { get; set; }
        /// <summary>
        /// Monto antes de descuento e IVA, en dólares.
        /// </summary>
        public double Subtotal { get; set; }
        /// <summary>
        /// Porcentaje de descuento aplicado, en decimal.
        /// </summary>
        public double Discount { get; set; }
        /// <summary>
        /// Monto del descuento, en dólares.
        /// </summary>
        public double DiscountAmount { get; set; }
        /// <summary>
        /// Porcentaje de IVA aplicado, en decimal.
        /// </summary>
        public double IVA { get; set; }
        /// <summary>
        /// Monto del IVA, en dólares.
        /// </summary>
        public double IvaAmount { get; set; }
        /// <summary>
        /// Tipo de cambio del BCCR usado para la conversión.
        /// </summary>
        public double ExchangeRate { get; set; }
        /// <summary>
        /// Total a pagar en dólares.
        /// </summary>
        public double TotalUsd { get; set; }
        /// <summary>
        /// Total a pagar en colones.
        /// </summary>
        public double TotalColones { get; set; }
        /// <summary>
        /// Cliente de la factura, cargado para mostrarlo en las vistas.
        /// </summary>
        public User? User { get; set; }
        /// <summary>
        /// Paquete de la factura, cargado para mostrarlo en las vistas.
        /// </summary>
        public Package? Package { get; set; }
        /// <summary>
        /// Reservación de la factura, cargada para mostrarla en las vistas.
        /// </summary>
        public Reservation? Reservation { get; set; }
    }
}
