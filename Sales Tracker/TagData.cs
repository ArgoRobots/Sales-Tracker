namespace Sales_Tracker
{
    public class TagData
    {
        // Properties
        private decimal _pricePerUnitUSD;
        private decimal _shippingUSD;
        private decimal _taxUSD;
        private decimal _feeUSD;
        private decimal _discountUSD;
        private decimal _chargedDifferenceUSD;
        private decimal _totalUSD;
        private string _defaultCurrencyType;

        // Getters and setters
        public decimal PricePerUnitUSD
        {
            get => _pricePerUnitUSD;
            set => _pricePerUnitUSD = value;
        }
        public decimal ShippingUSD
        {
            get => _shippingUSD;
            set => _shippingUSD = value;
        }
        public decimal TaxUSD
        {
            get => _taxUSD;
            set => _taxUSD = value;
        }
        public decimal FeeUSD
        {
            get => _feeUSD;
            set => _feeUSD = value;
        }
        public decimal DiscountUSD
        {
            get => _discountUSD;
            set => _discountUSD = value;
        }
        public decimal ChargedDifferenceUSD
        {
            get => _chargedDifferenceUSD;
            set => _chargedDifferenceUSD = value;
        }
        public decimal TotalUSD
        {
            get => _totalUSD;
            set => _totalUSD = value;
        }
        public string DefaultCurrencyType
        {
            get => _defaultCurrencyType;
            set => _defaultCurrencyType = value;
        }
    }
}