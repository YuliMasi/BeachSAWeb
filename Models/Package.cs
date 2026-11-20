namespace BeachSAWeb.Models
{
    public class Package
    {
        public int Id { get; set; }
        public string PackageType { get; set; }   // All inclusive / Feeding / Lodging
        public double Cost { get; set; }
        public double PercentageTotalAmount { get; set; }
        public int RemainingMonthlyInstallments { get; set; }
    }
}
