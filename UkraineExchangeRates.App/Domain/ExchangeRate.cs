namespace UkraineExchangeRates.App.Domain
{
    public class ExchangeRate
    {
        public string Currency { get; set; }
        public double? SaleRate { get; set; }
        public double? PurchaseRate { get; set; }
    }
}