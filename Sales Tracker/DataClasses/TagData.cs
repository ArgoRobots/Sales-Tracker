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
        public decimal PricePerUnitUSD { get; set; }
        public decimal ShippingUSD { get; set; }
        public decimal TaxUSD { get; set; }
        public decimal FeeUSD { get; set; }
        public decimal DiscountUSD { get; set; }
        public decimal ChargedDifferenceUSD { get; set; }
        public decimal ChargedOrCreditedUSD { get; set; }
        public string OriginalCurrency { get; set; }
        public decimal OriginalPricePerUnit { get; set; }
        public decimal OriginalShipping { get; set; }
        public decimal OriginalTax { get; set; }
        public decimal OriginalFee { get; set; }
        public decimal OriginalDiscount { get; set; }
        public decimal OriginalChargedDifference { get; set; }
        public decimal OriginalChargedOrCredited { get; set; }
    }
}