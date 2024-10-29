namespace Sales_Tracker.DataClasses
{
    /// <summary>
    /// Represents various cost-related data associated with a sales transaction, including pricing, shipping, tax, and total amounts in USD.
    /// This object is stored in the <see cref="DataGridViewRow.Tag"/> property of each row in the DataGridView.
    /// 
    /// Each property in this class is represented in USD, the world reserve currency, to convert to other currencies.
    /// These USD values are used for calculations, ensuring accurate exchange rate conversions.
    /// The use of USD as the standard currency avoids exchange rate fluctuations between lesser-used currencies.
    /// </summary>
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
    }
}