namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// Provides predefined report templates for common business reporting scenarios.
    /// </summary>
    public static class ReportTemplates
    {
        // Template creation methods

        /// <summary>
        /// Creates a monthly sales report template.
        /// </summary>
        public static ReportConfiguration CreateMonthlySalesTemplate()
        {
            ReportConfiguration config = new()
            {
                Title = "Monthly Sales Report",
                Description = "Comprehensive monthly sales performance analysis",
                TemplateName = "Monthly Sales Report",
                PageSize = PageSize.A4,
                Orientation = PageOrientation.Portrait,
                ShowHeader = true,
                ShowFooter = true,
                ShowPageNumbers = true
            };

            // Configure filters for sales data
            config.Filters.TransactionType = TransactionType.Sales;
            config.Filters.StartDate = DateTime.Now.AddMonths(-1).Date;
            config.Filters.EndDate = DateTime.Now.Date;
            config.Filters.SelectedChartTypes.AddRange(
            [
                MainMenu_Form.ChartDataType.TotalSales,
                MainMenu_Form.ChartDataType.DistributionOfSales,
                MainMenu_Form.ChartDataType.GrowthRates,
                MainMenu_Form.ChartDataType.AverageOrderValue
            ]);

            // Add report elements with specific positioning
            AddSalesReportElements(config);

            return config;
        }

        /// <summary>
        /// Creates a financial overview template.
        /// </summary>
        public static ReportConfiguration CreateFinancialOverviewTemplate()
        {
            ReportConfiguration config = new()
            {
                Title = "Financial Overview",
                Description = "Complete financial performance summary",
                TemplateName = "Financial Overview",
                PageSize = PageSize.A4,
                Orientation = PageOrientation.Landscape,
                ShowHeader = true,
                ShowFooter = true,
                ShowPageNumbers = true
            };

            // Configure filters for financial data
            config.Filters.TransactionType = TransactionType.Both;
            config.Filters.StartDate = DateTime.Now.AddMonths(-3).Date;
            config.Filters.EndDate = DateTime.Now.Date;
            config.Filters.SelectedChartTypes.AddRange(
            [
                MainMenu_Form.ChartDataType.TotalSales,
                MainMenu_Form.ChartDataType.TotalPurchases,
                MainMenu_Form.ChartDataType.TotalExpensesVsSales,
                MainMenu_Form.ChartDataType.TotalProfits
            ]);

            AddFinancialOverviewElements(config);

            return config;
        }

        /// <summary>
        /// Creates a performance analysis template.
        /// </summary>
        public static ReportConfiguration CreatePerformanceAnalysisTemplate()
        {
            ReportConfiguration config = new()
            {
                Title = "Performance Analysis",
                Description = "Detailed business performance metrics and trends",
                TemplateName = "Performance Analysis",
                PageSize = PageSize.A4,
                Orientation = PageOrientation.Portrait,
                ShowHeader = true,
                ShowFooter = true,
                ShowPageNumbers = true
            };

            // Configure filters for performance data
            config.Filters.TransactionType = TransactionType.Both;
            config.Filters.StartDate = DateTime.Now.AddMonths(-6).Date;
            config.Filters.EndDate = DateTime.Now.Date;
            config.Filters.SelectedChartTypes.AddRange(
            [
                MainMenu_Form.ChartDataType.GrowthRates,
                MainMenu_Form.ChartDataType.AverageOrderValue,
                MainMenu_Form.ChartDataType.TotalTransactions,
                MainMenu_Form.ChartDataType.ReturnsOverTime
            ]);

            AddPerformanceAnalysisElements(config);

            return config;
        }

        /// <summary>
        /// Creates a returns and losses analysis template.
        /// </summary>
        public static ReportConfiguration CreateReturnsAnalysisTemplate()
        {
            ReportConfiguration config = new()
            {
                Title = "Returns & Losses Analysis",
                Description = "Analysis of product returns and inventory losses",
                TemplateName = "Returns Analysis",
                PageSize = PageSize.A4,
                Orientation = PageOrientation.Portrait,
                ShowHeader = true,
                ShowFooter = true,
                ShowPageNumbers = true
            };

            // Configure filters for returns data
            config.Filters.TransactionType = TransactionType.Both;
            config.Filters.IncludeReturns = true;
            config.Filters.IncludeLosses = true;
            config.Filters.StartDate = DateTime.Now.AddMonths(-12).Date;
            config.Filters.EndDate = DateTime.Now.Date;
            config.Filters.SelectedChartTypes.AddRange(
            [
                MainMenu_Form.ChartDataType.ReturnsOverTime,
                MainMenu_Form.ChartDataType.ReturnReasons,
                MainMenu_Form.ChartDataType.ReturnFinancialImpact,
                MainMenu_Form.ChartDataType.LossesOverTime,
                MainMenu_Form.ChartDataType.LossReasons
            ]);

            AddReturnsAnalysisElements(config);

            return config;
        }

        /// <summary>
        /// Creates a geographic analysis template.
        /// </summary>
        public static ReportConfiguration CreateGeographicAnalysisTemplate()
        {
            ReportConfiguration config = new()
            {
                Title = "Geographic Analysis",
                Description = "Geographic distribution of sales and purchases",
                TemplateName = "Geographic Analysis",
                PageSize = PageSize.A4,
                Orientation = PageOrientation.Landscape,
                ShowHeader = true,
                ShowFooter = true,
                ShowPageNumbers = true
            };

            // Configure filters for geographic data
            config.Filters.TransactionType = TransactionType.Both;
            config.Filters.StartDate = DateTime.Now.AddMonths(-6).Date;
            config.Filters.EndDate = DateTime.Now.Date;
            config.Filters.SelectedChartTypes.AddRange(
            [
                MainMenu_Form.ChartDataType.WorldMap,
                MainMenu_Form.ChartDataType.CountriesOfOrigin,
                MainMenu_Form.ChartDataType.CountriesOfDestination,
                MainMenu_Form.ChartDataType.CompaniesOfOrigin
            ]);

            AddGeographicAnalysisElements(config);

            return config;
        }

        // Element addition methods
        private static void AddSalesReportElements(ReportConfiguration config)
        {
            Size pageSize = PageDimensions.GetDimensions(config.PageSize, config.Orientation);
            int margin = 40;
            int headerHeight = 80;
            int footerHeight = 50;
            int availableHeight = pageSize.Height - headerHeight - footerHeight - (margin * 2);

            // Date range element
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.DateRange,
                DisplayName = "Report Period",
                Bounds = new Rectangle(margin, headerHeight + margin, pageSize.Width - (margin * 2), 30),
                ZOrder = 0
            });

            // Sales total chart (top half, left)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.TotalSales,
                DisplayName = "Total Sales",
                Bounds = new Rectangle(margin, headerHeight + margin + 40, (pageSize.Width - margin * 3) / 2, availableHeight / 2 - 20),
                ZOrder = 1
            });

            // Sales distribution chart (top half, right)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.DistributionOfSales,
                DisplayName = "Sales Distribution",
                Bounds = new Rectangle((pageSize.Width / 2) + (margin / 2), headerHeight + margin + 40, (pageSize.Width - margin * 3) / 2, availableHeight / 2 - 20),
                ZOrder = 2
            });

            // Growth rates chart (bottom half, left)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.GrowthRates,
                DisplayName = "Growth Rates",
                Bounds = new Rectangle(margin, headerHeight + margin + availableHeight / 2 + 20, (pageSize.Width - margin * 3) / 2, availableHeight / 2 - 20),
                ZOrder = 3
            });

            // Average order value chart (bottom half, right)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.AverageOrderValue,
                DisplayName = "Average Order Value",
                Bounds = new Rectangle((pageSize.Width / 2) + (margin / 2), headerHeight + margin + availableHeight / 2 + 20, (pageSize.Width - margin * 3) / 2, availableHeight / 2 - 20),
                ZOrder = 4
            });
        }
        private static void AddFinancialOverviewElements(ReportConfiguration config)
        {
            Size pageSize = PageDimensions.GetDimensions(config.PageSize, config.Orientation);
            int margin = 40;
            int headerHeight = 80;
            int footerHeight = 50;
            int availableHeight = pageSize.Height - headerHeight - footerHeight - (margin * 2);
            int chartWidth = (pageSize.Width - margin * 3) / 2;
            int chartHeight = (availableHeight - 30) / 2;

            // Date range
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.DateRange,
                DisplayName = "Report Period",
                Bounds = new Rectangle(margin, headerHeight + margin, pageSize.Width - (margin * 2), 30),
                ZOrder = 0
            });

            // Sales vs Expenses (top left)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.TotalExpensesVsSales,
                DisplayName = "Sales vs Expenses",
                Bounds = new Rectangle(margin, headerHeight + margin + 40, chartWidth, chartHeight),
                ZOrder = 1
            });

            // Total Profits (top right)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.TotalProfits,
                DisplayName = "Total Profits",
                Bounds = new Rectangle(margin * 2 + chartWidth, headerHeight + margin + 40, chartWidth, chartHeight),
                ZOrder = 2
            });

            // Total Sales (bottom left)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.TotalSales,
                DisplayName = "Total Sales",
                Bounds = new Rectangle(margin, headerHeight + margin + 50 + chartHeight, chartWidth, chartHeight),
                ZOrder = 3
            });

            // Total Purchases (bottom right)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.TotalPurchases,
                DisplayName = "Total Purchases",
                Bounds = new Rectangle(margin * 2 + chartWidth, headerHeight + margin + 50 + chartHeight, chartWidth, chartHeight),
                ZOrder = 4
            });
        }
        private static void AddPerformanceAnalysisElements(ReportConfiguration config)
        {
            Size pageSize = PageDimensions.GetDimensions(config.PageSize, config.Orientation);
            int margin = 40;
            int headerHeight = 80;
            int footerHeight = 50;
            int availableHeight = pageSize.Height - headerHeight - footerHeight - (margin * 2);

            // Summary section
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Summary,
                DisplayName = "Performance Summary",
                Bounds = new Rectangle(margin, headerHeight + margin, pageSize.Width - (margin * 2), 80),
                ZOrder = 0
            });

            // Growth rates chart (top)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.GrowthRates,
                DisplayName = "Growth Rates",
                Bounds = new Rectangle(margin, headerHeight + margin + 90, pageSize.Width - (margin * 2), (availableHeight - 100) / 3),
                ZOrder = 1
            });

            // Average order value (middle)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.AverageOrderValue,
                DisplayName = "Average Order Value",
                Bounds = new Rectangle(margin, headerHeight + margin + 100 + (availableHeight - 100) / 3, pageSize.Width - (margin * 2), (availableHeight - 100) / 3),
                ZOrder = 2
            });

            // Returns over time (bottom)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.ReturnsOverTime,
                DisplayName = "Returns Over Time",
                Bounds = new Rectangle(margin, headerHeight + margin + 110 + 2 * (availableHeight - 100) / 3, pageSize.Width - (margin * 2), (availableHeight - 100) / 3),
                ZOrder = 3
            });
        }
        private static void AddReturnsAnalysisElements(ReportConfiguration config)
        {
            Size pageSize = PageDimensions.GetDimensions(config.PageSize, config.Orientation);
            int margin = 40;
            int headerHeight = 80;
            int footerHeight = 50;
            int availableHeight = pageSize.Height - headerHeight - footerHeight - (margin * 2);
            int chartWidth = (pageSize.Width - margin * 3) / 2;
            int chartHeight = availableHeight / 3;

            // Returns over time (top, full width)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.ReturnsOverTime,
                DisplayName = "Returns Over Time",
                Bounds = new Rectangle(margin, headerHeight + margin, pageSize.Width - (margin * 2), chartHeight),
                ZOrder = 0
            });

            // Return reasons (middle left)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.ReturnReasons,
                DisplayName = "Return Reasons",
                Bounds = new Rectangle(margin, headerHeight + margin + chartHeight + 10, chartWidth, chartHeight),
                ZOrder = 1
            });

            // Return financial impact (middle right)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.ReturnFinancialImpact,
                DisplayName = "Financial Impact",
                Bounds = new Rectangle(margin * 2 + chartWidth, headerHeight + margin + chartHeight + 10, chartWidth, chartHeight),
                ZOrder = 2
            });

            // Losses over time (bottom left)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.LossesOverTime,
                DisplayName = "Losses Over Time",
                Bounds = new Rectangle(margin, headerHeight + margin + 2 * chartHeight + 20, chartWidth, chartHeight),
                ZOrder = 3
            });

            // Loss reasons (bottom right)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.LossReasons,
                DisplayName = "Loss Reasons",
                Bounds = new Rectangle(margin * 2 + chartWidth, headerHeight + margin + 2 * chartHeight + 20, chartWidth, chartHeight),
                ZOrder = 4
            });
        }
        private static void AddGeographicAnalysisElements(ReportConfiguration config)
        {
            Size pageSize = PageDimensions.GetDimensions(config.PageSize, config.Orientation);
            int margin = 40;
            int headerHeight = 80;
            int footerHeight = 50;
            int availableHeight = pageSize.Height - headerHeight - footerHeight - (margin * 2);
            int chartWidth = (pageSize.Width - margin * 3) / 2;

            // World map (top, full width)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.WorldMap,
                DisplayName = "World Map",
                Bounds = new Rectangle(margin, headerHeight + margin, pageSize.Width - (margin * 2), availableHeight / 2),
                ZOrder = 0
            });

            // Countries of origin (bottom left)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.CountriesOfOrigin,
                DisplayName = "Countries of Origin",
                Bounds = new Rectangle(margin, headerHeight + margin + availableHeight / 2 + 10, chartWidth, availableHeight / 2 - 10),
                ZOrder = 1
            });

            // Countries of destination (bottom right)
            config.AddElement(new ReportElement
            {
                Type = ReportElementType.Chart,
                Data = MainMenu_Form.ChartDataType.CountriesOfDestination,
                DisplayName = "Countries of Destination",
                Bounds = new Rectangle(margin * 2 + chartWidth, headerHeight + margin + availableHeight / 2 + 10, chartWidth, availableHeight / 2 - 10),
                ZOrder = 2
            });
        }

        // Template management
        /// <summary>
        /// Gets all available template names.
        /// </summary>
        public static List<string> GetAvailableTemplates()
        {
            return
            [
                "Custom Report",
                "Monthly Sales Report",
                "Financial Overview",
                "Performance Analysis",
                "Returns Analysis",
                "Geographic Analysis"
            ];
        }

        /// <summary>
        /// Creates a report configuration from a template name.
        /// </summary>
        public static ReportConfiguration CreateFromTemplate(string templateName)
        {
            return templateName switch
            {
                "Monthly Sales Report" => CreateMonthlySalesTemplate(),
                "Financial Overview" => CreateFinancialOverviewTemplate(),
                "Performance Analysis" => CreatePerformanceAnalysisTemplate(),
                "Returns Analysis" => CreateReturnsAnalysisTemplate(),
                "Geographic Analysis" => CreateGeographicAnalysisTemplate(),
                _ => new ReportConfiguration()  // Custom report
            };
        }

        /// <summary>
        /// Gets a description for a template.
        /// </summary>
        public static string GetTemplateDescription(string templateName)
        {
            return templateName switch
            {
                "Monthly Sales Report" => "Comprehensive monthly sales performance with charts for total sales, distribution, growth, and average order value.",
                "Financial Overview" => "Complete financial summary including sales vs expenses, profits, and transaction totals in landscape format.",
                "Performance Analysis" => "Detailed business performance metrics showing growth rates, order values, and return trends.",
                "Returns Analysis" => "Analysis of product returns and losses with detailed breakdowns by reasons and financial impact.",
                "Geographic Analysis" => "Geographic distribution analysis with world map and country-based charts.",
                _ => "Create a custom report with your own selection of charts and layout."
            };
        }
    }
}