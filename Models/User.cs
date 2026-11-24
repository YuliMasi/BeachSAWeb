using System.ComponentModel.DataAnnotations;

namespace BeachSAWeb.Models
{
    /// <summary>
    /// Usuario del sistema, ya sea cliente o administrador.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Cédula del usuario, que funciona como su identificador.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Tipo de cédula del usuario.
        /// </summary>
        public string IdType { get; set; }
        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        public int PhoneNumber { get; set; }
        /// <summary>
        /// Dirección del usuario.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Correo electrónico con el que inicia sesión.
        /// </summary>
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [DataType(DataType.Password)]
        public string Password { get; set; }
        /// <summary>
        /// Rol del usuario: Admin o Customer.
        /// </summary>
        public string Role { get; set; }
    }
}
