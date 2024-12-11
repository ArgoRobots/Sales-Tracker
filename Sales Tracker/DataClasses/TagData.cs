namespace Sales_Tracker.DataClasses
{
    /// <summary>
    /// Represents various cost-related data associated with a sales transaction, including pricing, shipping, tax, and total amounts in USD.
    /// This object is stored in the Tag property of each row in the DataGridView.
    /// 
    /// Each property in this class is represented in USD, the world reserve currency, to convert to other currencies.
    /// These USD values are used for calculations, ensuring accurate exchange rate conversions.
    /// </summary>
    public class TagData
    {
        // Properties
        private decimal _pricePerUnitUSD,
            _shippingUSD,
            _taxUSD,
            _feeUSD,
            _discountUSD,
            _chargedDifferenceUSD,
            _chargedOrCreditedUSD;

        private string _originalCurrency;

        private decimal _originalPricePerUnit,
            _originalShipping,
            _originalTax,
            _originalFee,
            _originalDiscount,
            _originalChargedDifference,
            _originalChargedOrCredited;

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
        public decimal ChargedOrCreditedUSD
        {
            get => _chargedOrCreditedUSD;
            set => _chargedOrCreditedUSD = value;
        }
        public string OriginalCurrency
        {
            get => _originalCurrency;
            set => _originalCurrency = value;
        }
        public decimal OriginalPricePerUnit
        {
            get => _originalPricePerUnit;
            set => _originalPricePerUnit = value;
        }
        public decimal OriginalShipping
        {
            get => _originalShipping;
            set => _originalShipping = value;
        }
        public decimal OriginalTax
        {
            get => _originalTax;
            set => _originalTax = value;
        }
        public decimal OriginalFee
        {
            get => _originalFee;
            set => _originalFee = value;
        }
        public decimal OriginalDiscount
        {
            get => _originalDiscount;
            set => _originalDiscount = value;
        }
        public decimal OriginalChargedDifference
        {
            get => _originalChargedDifference;
            set => _originalChargedDifference = value;
        }
        public decimal OriginalChargedOrCredited
        {
            get => _originalChargedOrCredited;
            set => _originalChargedOrCredited = value;
        }
    }
}