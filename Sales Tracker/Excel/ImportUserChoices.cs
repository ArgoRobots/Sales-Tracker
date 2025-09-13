namespace Sales_Tracker.Excel
{
    public class ImportUserChoices
    {
        // null = ask, true = yes to all, false = no to all
        public bool? DuplicateItemChoice { get; set; } = null;
        public bool? CountryNotFoundChoice { get; set; } = null;
        public bool? ProductNotFoundChoice { get; set; } = null;
        public bool? DuplicateAccountantChoice { get; set; } = null;
        public bool? DuplicateCompanyChoice { get; set; } = null;
        public bool? DuplicateProductChoice { get; set; } = null;
    }
}