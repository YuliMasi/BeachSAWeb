namespace BeachSAWeb.Services
{
    public static class PackageInfo
    {
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
