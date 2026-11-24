using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;

namespace BeachSAWeb.Services
{
    /// <summary>
    /// Datos auxiliares para los formularios de reservación.
    /// </summary>
    public static class ReservationInfo
    {
        /// <summary>
        /// Arma la lista de formas de pago para el formulario.
        /// </summary>
        /// <param name="selectedValue">Forma de pago que debe aparecer seleccionada.</param>
        /// <returns>Las opciones Efectivo, Tarjeta y Cheque.</returns>
        public static List<SelectListItem> PaymentMethods(string? selectedValue = null)
        {
            string[] methods = { "Efectivo", "Tarjeta", "Cheque" };

            return methods.Select(m => new SelectListItem
            {
                Value = m,
                Text = m,
                Selected = string.Equals(m, selectedValue, StringComparison.OrdinalIgnoreCase)
            }).ToList();
        }

        /// <summary>
        /// Normaliza el nombre de una forma de pago para mostrarlo.
        /// </summary>
        /// <param name="method">Forma de pago guardada en la reservación.</param>
        /// <returns>El nombre de la forma de pago con mayúscula inicial.</returns>
        public static string PaymentName(string? method)
        {
            switch (method?.Trim().ToLower())
            {
                case "efectivo": return "Efectivo";
                case "tarjeta": return "Tarjeta";
                case "cheque": return "Cheque";
                default: return method ?? "";
            }
        }

        /// <summary>
        /// Consulta el tipo de cambio de venta del BCCR mediante GoMeta.
        /// </summary>
        /// <returns>El tipo de cambio, o 0 si no se pudo consultar.</returns>
        public static async Task<double> ExchangeRate()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://apis.gometa.org");
                    client.Timeout = TimeSpan.FromSeconds(10);

                    HttpResponseMessage http = await client.GetAsync("/tdc/tdc.json");

                    if (http.IsSuccessStatusCode)
                    {
                        JObject data = JObject.Parse(await http.Content.ReadAsStringAsync());
                        return (double?)data["venta"] ?? 0;
                    }
                }
            }
            catch (Exception)
            {
            }

            return 0;
        }
    }
}
