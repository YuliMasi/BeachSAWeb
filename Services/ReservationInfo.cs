using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;

namespace BeachSAWeb.Services
{
    public static class ReservationInfo
    {
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
