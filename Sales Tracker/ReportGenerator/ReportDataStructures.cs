namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// Represents a single element in a report (chart, table, text, etc.).
    /// </summary>
    public class ReportElement
    {
        /// <summary>
        /// Unique identifier for the element.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Type of report element.
        /// </summary>
        public ReportElementType Type { get; set; }

        /// <summary>
        /// Position and size of the element on the report canvas.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// The actual data/content for this element.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Additional properties for customization.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = [];

        /// <summary>
        /// Display name for the element (used in UI).
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Z-order for layering elements.
        /// </summary>
        public int ZOrder { get; set; }

        /// <summary>
        /// Whether the element is currently selected in the designer.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Whether the element is visible in the report.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Creates a new report element.
        /// </summary>
        public ReportElement()
        {
            Properties = [];
        }

        /// <summary>
        /// Creates a copy of this report element.
        /// </summary>
        public ReportElement Clone()
        {
            return new ReportElement
            {
                Id = Guid.NewGuid().ToString(),  // New ID for the clone
                Type = Type,
                Bounds = Bounds,
                Data = Data,
                Properties = new Dictionary<string, object>(Properties),
                DisplayName = DisplayName + " (Copy)",
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible
            };
        }
    }

    /// <summary>
    /// Types of elements that can be included in a report.
    /// </summary>
    public enum ReportElementType
    {
        Chart,
        TransactionTable,
        TextLabel,
        Image,
        Logo,
        DateRange,
        Summary
    }

    /// <summary>
    /// Complete configuration for a report including layout, data filters, and settings.
    /// </summary>
    public class ReportConfiguration
    {
        /// <summary>
        /// Page size and orientation settings.
        /// </summary>
        public PageSize PageSize { get; set; } = PageSize.A4;

        /// <summary>
        /// Page orientation
        /// </summary>
        public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;

        /// <summary>
        /// List of all elements in the report.
        /// </summary>
        public List<ReportElement> Elements { get; set; } = [];

        /// <summary>
        /// Data filtering configuration.
        /// </summary>
        public ReportFilters Filters { get; set; } = new ReportFilters();

        /// <summary>
        /// Template name (if created from a template).
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// Report title.
        /// </summary>
        public string Title { get; set; } = "Sales Report";

        /// <summary>
        /// Report description/subtitle.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Page margins in pixels.
        /// </summary>
        public Margins PageMargins { get; set; } = new Margins(40);

        /// <summary>
        /// Background color for the report.
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.White;

        /// <summary>
        /// Whether to show page numbers.
        /// </summary>
        public bool ShowPageNumbers { get; set; } = true;

        /// <summary>
        /// Whether to show header with title.
        /// </summary>
        public bool ShowHeader { get; set; } = true;

        /// <summary>
        /// Whether to show footer with date/time.
        /// </summary>
        public bool ShowFooter { get; set; } = true;

        /// <summary>
        /// Gets elements sorted by Z-order (for rendering).
        /// </summary>
        public List<ReportElement> GetElementsByZOrder()
        {
            return Elements.OrderBy(e => e.ZOrder).ToList();
        }

        /// <summary>
        /// Adds an element to the report.
        /// </summary>
        public void AddElement(ReportElement element)
        {
            if (element != null)
            {
                element.ZOrder = Elements.Count;
                Elements.Add(element);
            }
        }

        /// <summary>
        /// Removes an element from the report.
        /// </summary>
        public bool RemoveElement(string elementId)
        {
            ReportElement? element = Elements.FirstOrDefault(e => e.Id == elementId);
            if (element != null)
            {
                Elements.Remove(element);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Data filtering options for report generation.
    /// </summary>
    public class ReportFilters
    {
        /// <summary>
        /// Start date for data filtering.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for data filtering.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Transaction types to include.
        /// </summary>
        public TransactionType TransactionType { get; set; } = TransactionType.Both;

        /// <summary>
        /// Selected chart types to include.
        /// </summary>
        public List<MainMenu_Form.ChartDataType> SelectedChartTypes { get; set; } = [];

        /// <summary>
        /// Whether to include returned items.
        /// </summary>
        public bool IncludeReturns { get; set; } = true;

        /// <summary>
        /// Whether to include lost items.
        /// </summary>
        public bool IncludeLosses { get; set; } = true;
    }

    /// <summary>
    /// Transaction type filter options.
    /// </summary>
    public enum TransactionType
    {
        Sales,
        Purchases,
        Both
    }

    /// <summary>
    /// Supported page sizes for reports.
    /// </summary>
    public enum PageSize
    {
        A4,
        Letter,
        Legal,
        Tabloid,
        Custom
    }

    /// <summary>
    /// Page orientation options.
    /// </summary>
    public enum PageOrientation
    {
        Portrait,
        Landscape
    }

    /// <summary>
    /// Page margins specification.
    /// </summary>
    public class Margins(int left, int top, int right, int bottom)
    {
        public int Left { get; set; } = left;
        public int Top { get; set; } = top;
        public int Right { get; set; } = right;
        public int Bottom { get; set; } = bottom;

        public Margins(int allSides) : this(allSides, allSides, allSides, allSides) { }
    }

    /// <summary>
    /// Page size dimensions helper.
    /// </summary>
    public static class PageDimensions
    {
        /// <summary>
        /// Gets the dimensions for a page size in pixels (96 DPI).
        /// </summary>
        public static Size GetDimensions(PageSize pageSize, PageOrientation orientation)
        {
            Size size = pageSize switch
            {
                PageSize.A4 => new Size(794, 1123),        // 210 × 297 mm
                PageSize.Letter => new Size(816, 1056),    // 8.5 × 11 inches
                PageSize.Legal => new Size(816, 1344),     // 8.5 × 14 inches
                PageSize.Tabloid => new Size(1122, 1632),  // 11 × 17 inches
                _ => new Size(794, 1123)                   // Default to A4
            };

            // Swap dimensions for landscape
            if (orientation == PageOrientation.Landscape)
            {
                return new Size(size.Height, size.Width);
            }

            return size;
        }
    }

    /// <summary>
    /// Export settings for report output.
    /// </summary>
    public class ExportSettings
    {
        /// <summary>
        /// Export format.
        /// </summary>
        public ExportFormat Format { get; set; } = ExportFormat.PNG;

        /// <summary>
        /// Output file path.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// DPI for image exports.
        /// </summary>
        public int DPI { get; set; } = 300;

        /// <summary>
        /// Quality setting (0-100).
        /// </summary>
        public int Quality { get; set; } = 95;

        /// <summary>
        /// Whether to open the file after export.
        /// </summary>
        public bool OpenAfterExport { get; set; } = true;
    }

    /// <summary>
    /// Supported export formats.
    /// </summary>
    public enum ExportFormat
    {
        PNG,
        JPG,
        PDF,
        Excel,
        PowerPoint
    }
}