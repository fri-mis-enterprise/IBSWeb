namespace IBS.Utility.Helpers
{
    public static class WithholdingTaxHelper
    {
        public static decimal? GetDefaultSupplierPercentByCategory(string? category)
        {
            return category switch
            {
                "Trade" => 0.005m,
                "Commissionee" => 0.10m,
                "Hauler" => 0.02m,
                _ => null
            };
        }

        public static string? GetAccountNumberByPercent(decimal taxPercent)
        {
            return taxPercent switch
            {
                0.005m => "201030250",
                0.01m => "201030210",
                0.02m => "201030220",
                0.05m => "201030230",
                0.10m => "201030240",
                _ => null
            };
        }
    }
}
