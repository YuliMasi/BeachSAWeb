namespace BeachSAWeb.Models
{
    /// <summary>
    /// Respuesta de la API al iniciar sesión.
    /// </summary>
    public class AuthorizationResponse
    {
        /// <summary>
        /// Token JWT para autorizar las peticiones a la API.
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Indica si las credenciales fueron válidas.
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// Mensaje devuelto por la API.
        /// </summary>
        public string Msj { get; set; }
    }
}