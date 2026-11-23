using Microsoft.AspNetCore.Mvc.Rendering;

namespace BeachSAWeb.Services
{
    public record DefaultPackageOption(int Id, string Name, decimal PricePerNight);
    public static class ReservationInfo
    {
        public static List<DefaultPackageOption> DefaultPackages() => new()
        {
            new(1, PackageInfo.Name("Todo incluido"), 900m),
            new(2, PackageInfo.Name("Alimentación"), 550m),
            new(3, PackageInfo.Name("Hospedaje"), 420m)
        };

        public static List<SelectListItem> PaymentMethods(string? selectedValue = null)
        {
            var methods = new[]
            {
                "Efectivo",
                "Tarjeta",
                "SINPE Móvil",
                "Transferencia"
            };

            return methods.Select(m => new SelectListItem
            {
                Value = m,
                Text = m,
                Selected = string.Equals(m, selectedValue, StringComparison.OrdinalIgnoreCase)
            }).ToList();
        }

        public static string NormalizePaymentMethod(string? method)
        {
            return method?.Trim().ToLower() switch
            {
                "cash" or "efectivo" => "Efectivo",
                "card" or "tarjeta" => "Tarjeta",
                "sinpe" or "sinpe móvil" or "sinpe movil" => "SINPE Móvil",
                "transfer" or "transferencia" => "Transferencia",
                _ => "Efectivo"
            };
        }
    }
}
