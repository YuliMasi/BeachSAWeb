namespace BeachSAWeb.Models
{
    /// <summary>
    /// Datos que muestra la página de error.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Identificador de la petición que falló.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Indica si se debe mostrar el identificador de la petición.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
