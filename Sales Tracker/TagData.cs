namespace Sales_Tracker
{
    public class TagData
    {
        private decimal pricePerUnitUSD;
        private decimal shippingUSD;
        private decimal taxUSD;
        private decimal feeUSD;
        private decimal totalPriceUSD;

        public decimal PricePerUnitUSD
        {
            get => pricePerUnitUSD;
            set => pricePerUnitUSD = value;
        }
        public decimal ShippingUSD
        {
            get => shippingUSD;
            set => shippingUSD = value;
        }
        public decimal TaxUSD
        {
            get => taxUSD;
            set => taxUSD = value;
        }
        public decimal FeeUSD
        {
            get => feeUSD;
            set => feeUSD = value;
        }
        public decimal TotalPriceUSD
        {
            get => totalPriceUSD;
            set => totalPriceUSD = value;
        }
    }
}