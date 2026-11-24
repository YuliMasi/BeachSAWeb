namespace BeachSAWeb.Services
{
    /// <summary>
    /// Datos de presentación de cada tipo de paquete.
    /// </summary>
    public static class PackageInfo
    {
        /// <summary>
        /// Obtiene el nombre que se muestra para un tipo de paquete.
        /// </summary>
        /// <param name="type">Tipo de paquete guardado en la base de datos.</param>
        /// <returns>El nombre del paquete para mostrar en pantalla.</returns>
        public static string Name(string type)
        {
            switch (type?.Trim().ToLower())
            {
                case "all inclusive":
                case "todo incluido": return "Todo incluido";
                case "feeding":
                case "alimentación":
                case "alimentacion": return "Alimentación";
                case "lodging":
                case "hospedaje": return "Hospedaje";
                default: return type;
            }
        }

        /// <summary>
        /// Obtiene la ruta de la fotografía de un tipo de paquete.
        /// </summary>
        /// <param name="type">Tipo de paquete guardado en la base de datos.</param>
        /// <returns>La ruta de la imagen dentro de wwwroot.</returns>
        public static string Image(string type)
        {
            switch (Name(type))
            {
                case "Todo incluido": return "~/img/gallery-1.png";
                case "Alimentación": return "~/img/gallery-2.png";
                case "Hospedaje": return "~/img/gallery-3.png";
                default: return "~/img/hero.png";
            }
        }
    }
}
