using Guna.UI2.WinForms;
using Sales_Tracker.Language;
using Sales_Tracker.Theme;
using static Sales_Tracker.MainMenu_Form;

namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// Second step in report generation - drag-and-drop canvas for arranging report elements.
    /// </summary>
    public partial class ReportLayoutDesigner_Form : Form
    {
        // Properties
        /// <summary>
        /// Gets the parent report generator form.
        /// </summary>
        public ReportGenerator_Form ParentReportForm { get; private set; }

        /// <summary>
        /// Gets the current report configuration
        /// </summary>
        protected ReportConfiguration ReportConfig => ParentReportForm?.CurrentReportConfiguration;

        /// <summary>
        /// Indicates if the form is currently being loaded/updated programmatically.
        /// </summary>
        protected bool IsUpdating { get; private set; }

        private bool _isDragging = false;
        private Point _dragStartPoint;
        private ReportElement _selectedElement;

        // Init.
        public ReportLayoutDesigner_Form(ReportGenerator_Form parentForm)
        {
            InitializeComponent();
            ParentReportForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));

            InitializeChildForm();
        }
        protected virtual void InitializeChildForm()
        {
            SetupCanvas();
            SetupToolsPanel();
        }
        private void SetupCanvas()
        {
            // Configure canvas panel for drag-and-drop
            Canvas_Panel.AllowDrop = true;
            Canvas_Panel.DragEnter += Canvas_Panel_DragEnter;
            Canvas_Panel.DragDrop += Canvas_Panel_DragDrop;
            Canvas_Panel.Paint += Canvas_Panel_Paint;
            Canvas_Panel.MouseDown += Canvas_Panel_MouseDown;
            Canvas_Panel.MouseMove += Canvas_Panel_MouseMove;
            Canvas_Panel.MouseUp += Canvas_Panel_MouseUp;

            // Set canvas background
            Canvas_Panel.BackColor = Color.White;
        }
        private void SetupToolsPanel()
        {
            // Clear existing controls
            ToolsContainer_Panel.Controls.Clear();

            int yPosition = 20;
            const int buttonHeight = 40;
            const int spacing = 10;

            // Add element buttons
            AddToolButton("Chart Element", "Add a chart to the report", yPosition, AddChartElement);
            yPosition += buttonHeight + spacing;

            AddToolButton("Text Label", "Add a text label", yPosition, AddTextElement);
            yPosition += buttonHeight + spacing;

            AddToolButton("Date Range", "Add date range display", yPosition, AddDateRangeElement);
            yPosition += buttonHeight + spacing;

            AddToolButton("Summary", "Add summary statistics", yPosition, AddSummaryElement);
            yPosition += buttonHeight + spacing;

            // Add separator
            yPosition += 20;
            Label separator = new()
            {
                Text = "Layout Tools",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, yPosition),
                Size = new Size(200, 20)
            };
            ToolsContainer_Panel.Controls.Add(separator);
            yPosition += 30;

            // Layout tools
            AddToolButton("Align Left", "Align selected elements to the left", yPosition, AlignLeft);
            yPosition += buttonHeight + spacing;

            AddToolButton("Align Center", "Center selected elements horizontally", yPosition, AlignCenter);
            yPosition += buttonHeight + spacing;

            AddToolButton("Delete", "Delete selected element", yPosition, DeleteSelected);
        }
        private void AddToolButton(string text, string tooltip, int yPosition, EventHandler clickHandler)
        {
            Guna2Button button = new()
            {
                Text = text,
                Size = new Size(200, 35),
                Location = new Point(15, yPosition),
                BorderRadius = 6,
                FillColor = CustomColors.AccentBlue,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White
            };

            button.Click += clickHandler;
            ToolsContainer_Panel.Controls.Add(button);

            // Add tooltip
            ToolTip toolTip = new();
            toolTip.SetToolTip(button, tooltip);
        }

        // Canvas event handlers
        private void Canvas_Panel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void Canvas_Panel_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string elementType = (string)e.Data.GetData(DataFormats.Text);
                Point dropLocation = Canvas_Panel.PointToClient(new Point(e.X, e.Y));

                CreateElementAtLocation(elementType, dropLocation);
            }
        }
        private void Canvas_Panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Check if clicking on an element
                ReportElement clickedElement = GetElementAtPoint(e.Location);
                if (clickedElement != null)
                {
                    SelectElement(clickedElement);
                    _isDragging = true;
                    _dragStartPoint = e.Location;
                }
                else
                {
                    // Clear selection
                    ClearSelection();
                }
            }
        }
        private void Canvas_Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _selectedElement != null)
            {
                int deltaX = e.X - _dragStartPoint.X;
                int deltaY = e.Y - _dragStartPoint.Y;

                // Update element position
                Rectangle newBounds = _selectedElement.Bounds;
                newBounds.X += deltaX;
                newBounds.Y += deltaY;

                // Keep within canvas bounds
                if (newBounds.X >= 0 && newBounds.Right <= Canvas_Panel.Width &&
                    newBounds.Y >= 0 && newBounds.Bottom <= Canvas_Panel.Height)
                {
                    _selectedElement.Bounds = newBounds;
                    _dragStartPoint = e.Location;
                    Canvas_Panel.Invalidate();
                }
            }
        }
        private void Canvas_Panel_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }
        private void Canvas_Panel_Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics);
            DrawElements(e.Graphics);
            DrawSelection(e.Graphics);
        }

        // Drawing methods
        private void DrawGrid(Graphics g)
        {
            const int gridSize = 20;
            using Pen pen = new(Color.LightGray, 1);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            // Draw vertical lines
            for (int x = 0; x < Canvas_Panel.Width; x += gridSize)
            {
                g.DrawLine(pen, x, 0, x, Canvas_Panel.Height);
            }

            // Draw horizontal lines
            for (int y = 0; y < Canvas_Panel.Height; y += gridSize)
            {
                g.DrawLine(pen, 0, y, Canvas_Panel.Width, y);
            }
        }
        private void DrawElements(Graphics g)
        {
            if (ReportConfig?.Elements == null) { return; }

            foreach (ReportElement? element in ReportConfig.Elements.Where(e => e.IsVisible))
            {
                DrawElement(g, element);
            }
        }
        private static void DrawElement(Graphics g, ReportElement element)
        {
            using SolidBrush brush = new(GetElementColor(element.Type));
            using Pen pen = new(Color.Gray, 1);
            using Font font = new("Segoe UI", 9);
            using SolidBrush textBrush = new(Color.Black);

            // Draw element background
            g.FillRectangle(brush, element.Bounds);
            g.DrawRectangle(pen, element.Bounds);

            // Draw element label
            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            string displayText = element.DisplayName ?? element.Type.ToString();
            g.DrawString(displayText, font, textBrush, element.Bounds, format);
        }
        private void DrawSelection(Graphics g)
        {
            if (_selectedElement != null)
            {
                using Pen pen = new(CustomColors.AccentBlue);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(pen, _selectedElement.Bounds);

                // Draw resize handles
                DrawResizeHandles(g, _selectedElement.Bounds);
            }
        }
        private static void DrawResizeHandles(Graphics g, Rectangle bounds)
        {
            const int handleSize = 8;
            using SolidBrush brush = new(CustomColors.AccentBlue);

            // Corner handles
            g.FillRectangle(brush, bounds.Left - handleSize / 2, bounds.Top - handleSize / 2, handleSize, handleSize);
            g.FillRectangle(brush, bounds.Right - handleSize / 2, bounds.Top - handleSize / 2, handleSize, handleSize);
            g.FillRectangle(brush, bounds.Left - handleSize / 2, bounds.Bottom - handleSize / 2, handleSize, handleSize);
            g.FillRectangle(brush, bounds.Right - handleSize / 2, bounds.Bottom - handleSize / 2, handleSize, handleSize);
        }
        private static Color GetElementColor(ReportElementType elementType)
        {
            return elementType switch
            {
                ReportElementType.Chart => Color.LightBlue,
                ReportElementType.TextLabel => Color.LightYellow,
                ReportElementType.DateRange => Color.LightGreen,
                ReportElementType.Summary => Color.LightCyan,
                ReportElementType.TransactionTable => Color.LightPink,
                _ => Color.LightGray
            };
        }

        // Element management
        private void CreateElementAtLocation(string elementType, Point location)
        {
            ReportElement element = CreateElementByType(elementType, location);
            if (element != null)
            {
                ReportConfig?.AddElement(element);
                Canvas_Panel.Invalidate();
                NotifyParentValidationChanged();
            }
        }
        private ReportElement? CreateElementByType(string elementType, Point location)
        {
            Size size = new(200, 150);  // Default size

            return elementType.ToLower() switch
            {
                "chart" => new ReportElement
                {
                    Type = ReportElementType.Chart,
                    DisplayName = "Chart",
                    Bounds = new Rectangle(location, size),
                    Data = GetDefaultChartType()
                },
                "text" => new ReportElement
                {
                    Type = ReportElementType.TextLabel,
                    DisplayName = "Text Label",
                    Bounds = new Rectangle(location, new Size(150, 30)),
                    Data = "Sample Text"
                },
                "daterange" => new ReportElement
                {
                    Type = ReportElementType.DateRange,
                    DisplayName = "Date Range",
                    Bounds = new Rectangle(location, new Size(200, 30))
                },
                "summary" => new ReportElement
                {
                    Type = ReportElementType.Summary,
                    DisplayName = "Summary",
                    Bounds = new Rectangle(location, new Size(300, 100))
                },
                _ => null
            };
        }
        private ChartDataType GetDefaultChartType()
        {
            // Use the first selected chart type, or default to TotalSales
            return ReportConfig?.Filters?.SelectedChartTypes?.FirstOrDefault() ?? ChartDataType.TotalSales;
        }
        private ReportElement? GetElementAtPoint(Point point)
        {
            if (ReportConfig?.Elements == null) { return null; }

            // Check elements in reverse Z-order (top to bottom)
            IOrderedEnumerable<ReportElement> sortedElements = ReportConfig.Elements
                .Where(e => e.IsVisible)
                .OrderByDescending(e => e.ZOrder);

            foreach (ReportElement? element in sortedElements)
            {
                if (element.Bounds.Contains(point))
                {
                    return element;
                }
            }

            return null;
        }
        private void SelectElement(ReportElement element)
        {
            if (_selectedElement != null)
            {
                _selectedElement.IsSelected = false;
            }

            _selectedElement = element;
            element.IsSelected = true;

            Canvas_Panel.Invalidate();
            UpdatePropertiesPanel();
        }
        private void ClearSelection()
        {
            if (_selectedElement != null)
            {
                _selectedElement.IsSelected = false;
                _selectedElement = null;
                Canvas_Panel.Invalidate();
                UpdatePropertiesPanel();
            }
        }
        private void UpdatePropertiesPanel()
        {
            // Update properties panel based on selected element
            if (_selectedElement != null)
            {
                ElementProperties_Label.Text = $"Selected: {_selectedElement.DisplayName}";
                ElementProperties_Label.Visible = true;
            }
            else
            {
                ElementProperties_Label.Text = "No element selected";
                ElementProperties_Label.Visible = true;
            }
        }

        // Form event handlers
        private void ReportLayoutDesigner_Form_Shown(object sender, EventArgs e)
        {
            if (!IsUpdating)
            {
                OnStepActivated();
            }
        }
        private void ReportLayoutDesigner_Form_VisibleChanged(object sender, EventArgs e)
        {
            if (!IsUpdating)
            {
                if (Visible)
                {
                    OnStepActivated();
                }
                else
                {
                    OnStepDeactivated();
                }
            }
        }

        // Tool event handlers
        private void AddChartElement(object sender, EventArgs e)
        {
            CreateElementAtLocation("chart", new Point(50, 50));
        }
        private void AddTextElement(object sender, EventArgs e)
        {
            CreateElementAtLocation("text", new Point(50, 220));
        }
        private void AddDateRangeElement(object sender, EventArgs e)
        {
            CreateElementAtLocation("daterange", new Point(50, 280));
        }
        private void AddSummaryElement(object sender, EventArgs e)
        {
            CreateElementAtLocation("summary", new Point(50, 320));
        }
        private void AlignLeft(object sender, EventArgs e)
        {
            if (_selectedElement != null)
            {
                Rectangle bounds = _selectedElement.Bounds;
                bounds.X = 20;  // Align to left margin
                _selectedElement.Bounds = bounds;
                Canvas_Panel.Invalidate();
            }
        }
        private void AlignCenter(object sender, EventArgs e)
        {
            if (_selectedElement != null)
            {
                Rectangle bounds = _selectedElement.Bounds;
                bounds.X = (Canvas_Panel.Width - bounds.Width) / 2;
                _selectedElement.Bounds = bounds;
                Canvas_Panel.Invalidate();
            }
        }
        private void DeleteSelected(object sender, EventArgs e)
        {
            if (_selectedElement != null)
            {
                ReportConfig?.RemoveElement(_selectedElement.Id);
                _selectedElement = null;
                Canvas_Panel.Invalidate();
                NotifyParentValidationChanged();
            }
        }

        // Form implementation methods
        public virtual bool IsValidForNextStep()
        {
            // Must have at least one element positioned on the canvas
            return ReportConfig?.Elements?.Count > 0;
        }
        public virtual bool ValidateStep()
        {
            if (!IsValidForNextStep())
            {
                MessageBox.Show(
                    LanguageManager.TranslateString("Please add at least one element to your report layout."),
                    LanguageManager.TranslateString("Empty Layout"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            return true;
        }
        public virtual void UpdateReportConfiguration()
        {
            if (ReportConfig == null) { return; }

            // Configuration is updated in real-time as elements are moved
            ReportConfig.LastModified = DateTime.Now;
        }
        public virtual void LoadFromReportConfiguration()
        {
            if (ReportConfig == null) { return; }

            PerformUpdate(() =>
            {
                // Auto-generate elements from selected chart types if no elements exist
                if (ReportConfig.Elements.Count == 0 && ReportConfig.Filters.SelectedChartTypes.Count > 0)
                {
                    GenerateElementsFromTemplate();
                }

                Canvas_Panel.Invalidate();
            });
        }

        /// <summary>
        /// Called when the form becomes active (user navigates to this step).
        /// </summary>
        public virtual void OnStepActivated()
        {
            LoadFromReportConfiguration();
            NotifyParentValidationChanged();
        }

        /// <summary>
        /// Called when the form becomes inactive (user navigates away from this step).
        /// </summary>
        public virtual void OnStepDeactivated()
        {
            UpdateReportConfiguration();
        }
        private void GenerateElementsFromTemplate()
        {
            if (ReportConfig?.Filters?.SelectedChartTypes == null) { return; }

            // Auto-arrange charts based on template or create simple layout
            int x = 50, y = 50;
            const int chartWidth = 350;
            const int chartHeight = 250;
            const int spacing = 20;
            const int maxWidth = 800;

            foreach (ChartDataType chartType in ReportConfig.Filters.SelectedChartTypes)
            {
                ReportElement element = new()
                {
                    Type = ReportElementType.Chart,
                    Data = chartType,
                    DisplayName = GetChartDisplayName(chartType),
                    Bounds = new Rectangle(x, y, chartWidth, chartHeight)
                };

                ReportConfig.AddElement(element);

                // Move to next position
                x += chartWidth + spacing;
                if (x + chartWidth > maxWidth)
                {
                    x = 50;
                    y += chartHeight + spacing;
                }
            }
        }
        private static string GetChartDisplayName(ChartDataType chartType)
        {
            return chartType switch
            {
                ChartDataType.TotalSales => "Total Sales",
                ChartDataType.TotalPurchases => "Total Purchases",
                ChartDataType.DistributionOfSales => "Sales Distribution",
                ChartDataType.TotalExpensesVsSales => "Sales vs Expenses",
                ChartDataType.GrowthRates => "Growth Rates",
                ChartDataType.AverageOrderValue => "Average Order Value",
                _ => chartType.ToString()
            };
        }

        // Helper methods for base functionality
        /// <summary>
        /// Notifies the parent form that validation state has changed.
        /// </summary>
        protected void NotifyParentValidationChanged()
        {
            ParentReportForm?.OnChildFormValidationChanged();
        }

        /// <summary>
        /// Safely updates UI controls without triggering events.
        /// </summary>
        /// <param name="updateAction">Action to perform during update</param>
        protected void PerformUpdate(Action updateAction)
        {
            if (updateAction == null) { return; }

            IsUpdating = true;

            try
            {
                updateAction();
            }
            finally
            {
                IsUpdating = false;
            }
        }
    }
}