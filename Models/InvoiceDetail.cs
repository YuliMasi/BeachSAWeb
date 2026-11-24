namespace BeachSAWeb.Models
{
    /// <summary>
    /// Detalle completo de una factura, tal como lo devuelve la API.
    /// </summary>
    public class InvoiceDetail
    {
        /// <summary>
        /// Indica si la factura se envió por correo al crear la reservación.
        /// </summary>
        public bool EmailSent { get; set; }
        /// <summary>
        /// Número consecutivo de la factura.
        /// </summary>
        public int InvoiceNumber { get; set; }
        /// <summary>
        /// Fecha en que se emitió la factura.
        /// </summary>
        public DateTime InvoiceDate { get; set; }
        /// <summary>
        /// Datos del cliente facturado.
        /// </summary>
        public InvoiceCustomer Customer { get; set; }
        /// <summary>
        /// Datos de la reservación facturada.
        /// </summary>
        public InvoiceReservation Reservation { get; set; }
        /// <summary>
        /// Montos de la factura.
        /// </summary>
        public InvoiceAmounts Details { get; set; }
    }

    /// <summary>
    /// Datos del cliente que aparecen en la factura.
    /// </summary>
    public class InvoiceCustomer
    {
        /// <summary>
        /// Cédula del cliente.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Nombre completo del cliente.
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// Tipo de cédula del cliente.
        /// </summary>
        public string IdType { get; set; }
        /// <summary>
        /// Correo del cliente.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Teléfono del cliente.
        /// </summary>
        public int PhoneNumber { get; set; }
    }

    /// <summary>
    /// Datos de la reservación que aparecen en la factura.
    /// </summary>
    public class InvoiceReservation
    {
        /// <summary>
        /// Identificador de la reservación.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Tipo de paquete reservado.
        /// </summary>
        public string PackageType { get; set; }
        /// <summary>
        /// Cantidad de personas.
        /// </summary>
        public int GuestQuantity { get; set; }
        /// <summary>
        /// Cantidad de noches.
        /// </summary>
        public int AccommodationDays { get; set; }
        /// <summary>
        /// Fecha de entrada.
        /// </summary>
        public DateTime CheckInDate { get; set; }
        /// <summary>
        /// Fecha de salida.
        /// </summary>
        public DateTime CheckOutDate { get; set; }
        /// <summary>
        /// Forma de pago.
        /// </summary>
        public string PaymentMethod { get; set; }
        /// <summary>
        /// Número del cheque, si se usó.
        /// </summary>
        public string CheckNumber { get; set; }
        /// <summary>
        /// Banco del cheque, si se usó.
        /// </summary>
        public string Bank { get; set; }
    }

    /// <summary>
    /// Montos que aparecen en la factura.
    /// </summary>
    public class InvoiceAmounts
    {
        /// <summary>
        /// Monto antes de descuento e IVA, en dólares.
        /// </summary>
        public double Subtotal { get; set; }
        /// <summary>
        /// Porcentaje de descuento, en decimal.
        /// </summary>
        public double Discount { get; set; }
        /// <summary>
        /// Monto del descuento, en dólares.
        /// </summary>
        public double DiscountAmount { get; set; }
        /// <summary>
        /// Porcentaje de IVA, en decimal.
        /// </summary>
        public double Iva { get; set; }
        /// <summary>
        /// Monto del IVA, en dólares.
        /// </summary>
        public double IvaAmount { get; set; }
        /// <summary>
        /// Tipo de cambio del BCCR.
        /// </summary>
        public double ExchangeRate { get; set; }
        /// <summary>
        /// Total en dólares.
        /// </summary>
        public double TotalUsd { get; set; }
        /// <summary>
        /// Total en colones.
        /// </summary>
        public double TotalColones { get; set; }
    }
}
