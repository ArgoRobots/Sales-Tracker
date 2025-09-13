﻿using Sales_Tracker.DataClasses;

namespace Sales_Tracker.Excel
{
    public class ImportSession
    {
        public List<string> AddedAccountants { get; set; } = [];
        public List<string> AddedCompanies { get; set; } = [];
        public Dictionary<string, List<Product>> AddedProducts { get; set; } = [];
        public List<DataGridViewRow> AddedPurchaseRows { get; set; } = [];
        public List<DataGridViewRow> AddedSaleRows { get; set; } = [];
        public List<Category> AddedCategories { get; set; } = [];
        public HashSet<string> SkippedTransactionIds { get; set; } = [];
        public bool IsCancelled { get; set; } = false;
        public ImportUserChoices UserChoices { get; set; } = new();
        public bool HasChanges()
        {
            return AddedAccountants.Count > 0 || AddedCompanies.Count > 0 ||
                   AddedProducts.Count > 0 || AddedPurchaseRows.Count > 0 ||
                   AddedSaleRows.Count > 0 || AddedCategories.Count > 0;
        }
    }
}