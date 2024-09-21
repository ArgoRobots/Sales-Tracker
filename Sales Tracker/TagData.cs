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

        private decimal _pricePerUnitDefault;
        private decimal _shippingDefault;
        private decimal _taxDefault;
        private decimal _feeDefault;
        private decimal _discountDefault;
        private decimal _chargedDifferenceDefault;
        private decimal _totalDefault;

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
        public decimal PricePerUnitDefault
        {
            get => _pricePerUnitDefault;
            set => _pricePerUnitDefault = value;
        }
        public decimal ShippingDefault
        {
            get => _shippingDefault;
            set => _shippingDefault = value;
        }
        public decimal TaxDefault
        {
            get => _taxDefault;
            set => _taxDefault = value;
        }
        public decimal FeeDefault
        {
            get => _feeDefault;
            set => _feeDefault = value;
        }
        public decimal DiscountDefault
        {
            get => _discountDefault;
            set => _discountDefault = value;
        }
        public decimal ChargedDifferenceDefault
        {
            get => _chargedDifferenceDefault;
            set => _chargedDifferenceDefault = value;
        }
        public decimal TotalDefault
        {
            get => _totalDefault;
            set => _totalDefault = value;
        }
    }
}