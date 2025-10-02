using Sales_Tracker.ReportGenerator.Elements;

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
                Orientation = PageOrientation.Landscape,
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
                MainMenu_Form.ChartDataType.AverageTransactionValue
            ]);

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
                MainMenu_Form.ChartDataType.Profits
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
                MainMenu_Form.ChartDataType.AverageTransactionValue,
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
            TemplateLayoutHelper.LayoutContext context = new()
            {
                PageSize = PageDimensions.GetDimensions(config.PageSize, config.Orientation)
            };

            // Date range element
            config.AddElement(new DateRangeElement
            {
                DisplayName = "Report Period",
                Bounds = TemplateLayoutHelper.GetDateRangeBounds(context),
                ZOrder = 0
            });

            // Create a 2x2 grid for charts
            Rectangle[,] grid = TemplateLayoutHelper.CreateGrid(context, 2, 2);

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.TotalSales,
                DisplayName = "Total Sales",
                Bounds = grid[0, 0],
                ZOrder = 1
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.DistributionOfSales,
                DisplayName = "Sales Distribution",
                Bounds = grid[0, 1],
                ZOrder = 2
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.GrowthRates,
                DisplayName = "Growth Rates",
                Bounds = grid[1, 0],
                ZOrder = 3
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.AverageTransactionValue,
                DisplayName = "Average Order Value",
                Bounds = grid[1, 1],
                ZOrder = 4
            });
        }
        private static void AddFinancialOverviewElements(ReportConfiguration config)
        {
            TemplateLayoutHelper.LayoutContext context = new()
            {
                PageSize = PageDimensions.GetDimensions(config.PageSize, config.Orientation)
            };

            // Date range
            config.AddElement(new DateRangeElement
            {
                DisplayName = "Report Period",
                Bounds = TemplateLayoutHelper.GetDateRangeBounds(context),
                ZOrder = 0
            });

            // Create 2x2 grid for charts
            Rectangle[,] grid = TemplateLayoutHelper.CreateGrid(context, 2, 2);

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.TotalExpensesVsSales,
                DisplayName = "Sales vs Expenses",
                Bounds = grid[0, 0],
                ZOrder = 1
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.Profits,
                DisplayName = "Total Profits",
                Bounds = grid[0, 1],
                ZOrder = 2
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.TotalSales,
                DisplayName = "Total Sales",
                Bounds = grid[1, 0],
                ZOrder = 3
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.TotalPurchases,
                DisplayName = "Total Purchases",
                Bounds = grid[1, 1],
                ZOrder = 4
            });
        }
        private static void AddPerformanceAnalysisElements(ReportConfiguration config)
        {
            TemplateLayoutHelper.LayoutContext context = new()
            {
                PageSize = PageDimensions.GetDimensions(config.PageSize, config.Orientation)
            };

            // Date range
            config.AddElement(new DateRangeElement
            {
                DisplayName = "Report Period",
                Bounds = TemplateLayoutHelper.GetDateRangeBounds(context),
                ZOrder = 0
            });

            // Create vertical stack: summary (15%), then 3 equal charts
            Rectangle[] stack = TemplateLayoutHelper.CreateVerticalStack(context, 0.15f, 0.28f, 0.28f, 0.29f);

            config.AddElement(new SummaryElement
            {
                DisplayName = "Performance Summary",
                Bounds = stack[0],
                ZOrder = 1
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.GrowthRates,
                DisplayName = "Growth Rates",
                Bounds = stack[1],
                ZOrder = 2
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.AverageTransactionValue,
                DisplayName = "Average Order Value",
                Bounds = stack[2],
                ZOrder = 3
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.ReturnsOverTime,
                DisplayName = "Returns Over Time",
                Bounds = stack[3],
                ZOrder = 4
            });
        }
        private static void AddReturnsAnalysisElements(ReportConfiguration config)
        {
            TemplateLayoutHelper.LayoutContext context = new()
            {
                PageSize = PageDimensions.GetDimensions(config.PageSize, config.Orientation)
            };

            // Date range
            config.AddElement(new DateRangeElement
            {
                DisplayName = "Report Period",
                Bounds = TemplateLayoutHelper.GetDateRangeBounds(context),
                ZOrder = 0
            });

            // Mixed layout: full-width chart at top, then 2x2 grid
            Rectangle[] topStack = TemplateLayoutHelper.CreateVerticalStack(context, 0.33f, 0.67f);

            // Use the top portion for the full-width chart
            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.ReturnsOverTime,
                DisplayName = "Returns Over Time",
                Bounds = topStack[0],
                ZOrder = 1
            });

            // Create a 2x2 grid in the bottom portion
            Rectangle bottomArea = topStack[1];
            int gridWidth = (bottomArea.Width - context.ElementSpacing) / 2;
            int gridHeight = (bottomArea.Height - context.ElementSpacing) / 2;

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.ReturnReasons,
                DisplayName = "Return Reasons",
                Bounds = new Rectangle(bottomArea.X, bottomArea.Y, gridWidth, gridHeight),
                ZOrder = 2
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.ReturnFinancialImpact,
                DisplayName = "Financial Impact",
                Bounds = new Rectangle(bottomArea.X + gridWidth + context.ElementSpacing, bottomArea.Y, gridWidth, gridHeight),
                ZOrder = 3
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.LossesOverTime,
                DisplayName = "Losses Over Time",
                Bounds = new Rectangle(bottomArea.X, bottomArea.Y + gridHeight + context.ElementSpacing, gridWidth, gridHeight),
                ZOrder = 4
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.LossReasons,
                DisplayName = "Loss Reasons",
                Bounds = new Rectangle(bottomArea.X + gridWidth + context.ElementSpacing, bottomArea.Y + gridHeight + context.ElementSpacing, gridWidth, gridHeight),
                ZOrder = 5
            });
        }
        private static void AddGeographicAnalysisElements(ReportConfiguration config)
        {
            TemplateLayoutHelper.LayoutContext context = new()
            {
                PageSize = PageDimensions.GetDimensions(config.PageSize, config.Orientation)
            };

            // Date range
            config.AddElement(new DateRangeElement
            {
                DisplayName = "Report Period",
                Bounds = TemplateLayoutHelper.GetDateRangeBounds(context),
                ZOrder = 0
            });

            // Create vertical split: map (50%), then bottom grid (50%)
            Rectangle[] stack = TemplateLayoutHelper.CreateVerticalStack(context, 0.5f, 0.5f);

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.WorldMap,
                DisplayName = "World Map",
                Bounds = stack[0],
                ZOrder = 1
            });

            // Split bottom area into 2 columns
            Rectangle bottomArea = stack[1];
            int colWidth = (bottomArea.Width - context.ElementSpacing) / 2;

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.CountriesOfOrigin,
                DisplayName = "Countries of Origin",
                Bounds = new Rectangle(bottomArea.X, bottomArea.Y, colWidth, bottomArea.Height),
                ZOrder = 2
            });

            config.AddElement(new ChartElement
            {
                ChartType = MainMenu_Form.ChartDataType.CountriesOfDestination,
                DisplayName = "Countries of Destination",
                Bounds = new Rectangle(bottomArea.X + colWidth + context.ElementSpacing, bottomArea.Y, colWidth, bottomArea.Height),
                ZOrder = 3
            });
        }

        // Template management
        public static class TemplateNames
        {
            public const string Custom = "Custom Report";
            public const string MonthlySales = "Monthly Sales Report";
            public const string FinancialOverview = "Financial Overview";
            public const string PerformanceAnalysis = "Performance Analysis";
            public const string ReturnsAnalysis = "Returns Analysis";
            public const string GeographicAnalysis = "Geographic Analysis";
        }

        /// <summary>
        /// Gets all available template names.
        /// </summary>
        public static List<string> GetAvailableTemplates()
        {
            return
            [
                TemplateNames.Custom,
                TemplateNames.MonthlySales,
                TemplateNames.FinancialOverview,
                TemplateNames.PerformanceAnalysis,
                TemplateNames.ReturnsAnalysis,
                TemplateNames.GeographicAnalysis
            ];
        }

        /// <summary>
        /// Creates a report configuration from a template name.
        /// </summary>
        public static ReportConfiguration CreateFromTemplate(string templateName)
        {
            return templateName switch
            {
                TemplateNames.MonthlySales => CreateMonthlySalesTemplate(),
                TemplateNames.FinancialOverview => CreateFinancialOverviewTemplate(),
                TemplateNames.PerformanceAnalysis => CreatePerformanceAnalysisTemplate(),
                TemplateNames.ReturnsAnalysis => CreateReturnsAnalysisTemplate(),
                TemplateNames.GeographicAnalysis => CreateGeographicAnalysisTemplate(),
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
                TemplateNames.MonthlySales => "Comprehensive monthly sales performance with charts for total sales, distribution, growth, and average order value.",
                TemplateNames.FinancialOverview => "Complete financial summary including sales vs expenses, profits, and transaction totals in landscape format.",
                TemplateNames.PerformanceAnalysis => "Detailed business performance metrics showing growth rates, order values, and return trends.",
                TemplateNames.ReturnsAnalysis => "Analysis of product returns and losses with detailed breakdowns by reasons and financial impact.",
                TemplateNames.GeographicAnalysis => "Geographic distribution analysis with world map and country-based charts.",
                _ => "Create a custom report with your own selection of charts and layout."
            };
        }
    }
}