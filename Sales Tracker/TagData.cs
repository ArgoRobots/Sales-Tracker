namespace Sales_Tracker
{
    public class TagData
    {
        private decimal pricePerUnitUSD;
        private decimal shippingUSD;
        private decimal taxUSD;
        private decimal feeUSD;
        private decimal discountUSD;
        private decimal chargedDifferenceUSD;
        private decimal totalUSD;

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
        public decimal DiscountUSD
        {
            get => discountUSD;
            set => discountUSD = value;
        }
        public decimal ChargedDifferenceUSD
        {
            get => chargedDifferenceUSD;
            set => chargedDifferenceUSD = value;
        }
        public decimal TotalUSD
        {
            get => totalUSD;
            set => totalUSD = value;
        }
    }
}