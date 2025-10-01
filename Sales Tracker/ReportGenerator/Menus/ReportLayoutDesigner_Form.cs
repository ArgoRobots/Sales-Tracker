using Guna.UI2.WinForms;
using Sales_Tracker.Theme;

namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// Second step in report generation - drag-and-drop canvas for arranging report elements.
    /// </summary>
    public partial class ReportLayoutDesigner_Form : Form
    {
        // Properties
        private int _initialFormWidth;
        private int _initialLeftPanelWidth;
        private int _initialRightPanelWidth;
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private ReportElement _selectedElement;
        private bool _isResizing = false;
        private ResizeHandle _activeResizeHandle;
        private Rectangle _originalBounds;

        // Property controls
        private readonly Dictionary<string, Control> _propertyControls = [];

        /// <summary>
        /// Gets the parent report generator form.
        /// </summary>
        public ReportGenerator_Form ParentReportForm { get; private set; }

        /// <summary>
        /// Gets the current report configuration.
        /// </summary>
        private ReportConfiguration? ReportConfig => ParentReportForm?.CurrentReportConfiguration;

        /// <summary>
        /// Indicates if the form is currently being loaded/updated programmatically.
        /// </summary>
        private bool _isUpdating;

        // Resize handle enumeration
        private enum ResizeHandle
        {
            None,
            TopLeft, TopRight, BottomLeft, BottomRight,
            Top, Bottom, Left, Right
        }

        // Init.
        public ReportLayoutDesigner_Form(ReportGenerator_Form parentForm)
        {
            InitializeComponent();
            ParentReportForm = parentForm;

            SetupCanvas();
            SetupToolsPanel();
            StoreInitialSizes();
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

            // Set canvas backgrounds
            Canvas_Panel.BackColor = Color.White;
        }
        private void SetupToolsPanel()
        {
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
                ForeColor = CustomColors.Text,
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
                ForeColor = CustomColors.Text
            };

            button.Click += clickHandler;
            ToolsContainer_Panel.Controls.Add(button);

            ToolTip toolTip = new();
            toolTip.SetToolTip(button, tooltip);
        }
        private void StoreInitialSizes()
        {
            _initialFormWidth = Width;
            _initialLeftPanelWidth = LeftTools_Panel.Width;
            _initialRightPanelWidth = RightCanvas_Panel.Width;
        }

        // Form event handlers
        private void ReportLayoutDesigner_Form_VisibleChanged(object sender, EventArgs e)
        {
            if (!_isUpdating && Visible)
            {
                if (ReportConfig == null)
                {
                    NotifyParentValidationChanged();
                    return;
                }

                PerformUpdate(SynchronizeCanvasWithSelection);
                NotifyParentValidationChanged();
            }
        }
        private void ReportLayoutDesigner_Form_Resize(object sender, EventArgs e)
        {
            if (_initialFormWidth == 0) { return; }

            // Calculate the form's width change ratio
            float widthRatio = (float)Width / _initialFormWidth;

            // Calculate new panel widths while maintaining proportion
            LeftTools_Panel.Width = (int)(_initialLeftPanelWidth * widthRatio);
            RightCanvas_Panel.Width = (int)(_initialRightPanelWidth * widthRatio);

            // Position the right panel
            RightCanvas_Panel.Left = LeftTools_Panel.Width;
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
                // Check if clicking on a resize handle
                if (_selectedElement != null)
                {
                    ResizeHandle handle = GetResizeHandleAt(e.Location);
                    if (handle != ResizeHandle.None)
                    {
                        _isResizing = true;
                        _activeResizeHandle = handle;
                        _originalBounds = _selectedElement.Bounds;
                        _dragStartPoint = e.Location;
                        return;
                    }
                }

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
                    ClearSelection();
                }
            }
        }
        private void Canvas_Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_selectedElement != null)
            {
                // Update cursor based on hover over resize handles
                if (!_isResizing && !_isDragging)
                {
                    ResizeHandle handle = GetResizeHandleAt(e.Location);
                    Canvas_Panel.Cursor = GetCursorForHandle(handle);
                }

                // Handle resizing
                if (_isResizing)
                {
                    ResizeElement(e.Location);
                }

                // Handle dragging
                else if (_isDragging)
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
                        UpdatePropertiesPanel();
                    }
                }
            }
        }
        private void Canvas_Panel_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            _isResizing = false;
            _activeResizeHandle = ResizeHandle.None;
            Canvas_Panel.Cursor = Cursors.Default;
        }
        private void Canvas_Panel_Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics);
            DrawElements(e.Graphics);
            DrawSelection(e.Graphics);
        }

        // Resize handling methods
        private ResizeHandle GetResizeHandleAt(Point point)
        {
            if (_selectedElement == null) return ResizeHandle.None;

            const int handleSize = 8;
            Rectangle bounds = _selectedElement.Bounds;

            // Corner handles
            if (new Rectangle(bounds.Left - handleSize / 2, bounds.Top - handleSize / 2, handleSize, handleSize).Contains(point))
            {
                return ResizeHandle.TopLeft;
            }
            if (new Rectangle(bounds.Right - handleSize / 2, bounds.Top - handleSize / 2, handleSize, handleSize).Contains(point))
            {
                return ResizeHandle.TopRight;
            }
            if (new Rectangle(bounds.Left - handleSize / 2, bounds.Bottom - handleSize / 2, handleSize, handleSize).Contains(point))
            {
                return ResizeHandle.BottomLeft;
            }
            if (new Rectangle(bounds.Right - handleSize / 2, bounds.Bottom - handleSize / 2, handleSize, handleSize).Contains(point))
            {
                return ResizeHandle.BottomRight;
            }

            // Edge handles
            if (new Rectangle(bounds.Left - handleSize / 2, bounds.Top + handleSize, handleSize, bounds.Height - 2 * handleSize).Contains(point))
            {
                return ResizeHandle.Left;
            }
            if (new Rectangle(bounds.Right - handleSize / 2, bounds.Top + handleSize, handleSize, bounds.Height - 2 * handleSize).Contains(point))
            {
                return ResizeHandle.Right;
            }
            if (new Rectangle(bounds.Left + handleSize, bounds.Top - handleSize / 2, bounds.Width - 2 * handleSize, handleSize).Contains(point))
            {
                return ResizeHandle.Top;
            }
            if (new Rectangle(bounds.Left + handleSize, bounds.Bottom - handleSize / 2, bounds.Width - 2 * handleSize, handleSize).Contains(point))
            {
                return ResizeHandle.Bottom;
            }

            return ResizeHandle.None;
        }
        private static Cursor GetCursorForHandle(ResizeHandle handle)
        {
            return handle switch
            {
                ResizeHandle.TopLeft or ResizeHandle.BottomRight => Cursors.SizeNWSE,
                ResizeHandle.TopRight or ResizeHandle.BottomLeft => Cursors.SizeNESW,
                ResizeHandle.Left or ResizeHandle.Right => Cursors.SizeWE,
                ResizeHandle.Top or ResizeHandle.Bottom => Cursors.SizeNS,
                _ => Cursors.Default
            };
        }
        private void ResizeElement(Point currentPoint)
        {
            if (_selectedElement == null) return;

            int deltaX = currentPoint.X - _dragStartPoint.X;
            int deltaY = currentPoint.Y - _dragStartPoint.Y;
            Rectangle newBounds = _originalBounds;

            switch (_activeResizeHandle)
            {
                case ResizeHandle.TopLeft:
                    newBounds.X += deltaX;
                    newBounds.Y += deltaY;
                    newBounds.Width -= deltaX;
                    newBounds.Height -= deltaY;
                    break;
                case ResizeHandle.TopRight:
                    newBounds.Y += deltaY;
                    newBounds.Width += deltaX;
                    newBounds.Height -= deltaY;
                    break;
                case ResizeHandle.BottomLeft:
                    newBounds.X += deltaX;
                    newBounds.Width -= deltaX;
                    newBounds.Height += deltaY;
                    break;
                case ResizeHandle.BottomRight:
                    newBounds.Width += deltaX;
                    newBounds.Height += deltaY;
                    break;
                case ResizeHandle.Left:
                    newBounds.X += deltaX;
                    newBounds.Width -= deltaX;
                    break;
                case ResizeHandle.Right:
                    newBounds.Width += deltaX;
                    break;
                case ResizeHandle.Top:
                    newBounds.Y += deltaY;
                    newBounds.Height -= deltaY;
                    break;
                case ResizeHandle.Bottom:
                    newBounds.Height += deltaY;
                    break;
            }

            // Ensure minimum size
            if (newBounds.Width >= 50 && newBounds.Height >= 30)
            {
                _selectedElement.Bounds = newBounds;
                Canvas_Panel.Invalidate();
                UpdatePropertiesPanel();
            }
        }

        // Drawing methods
        private void DrawGrid(Graphics g)
        {
            const int gridSize = 20;
            using Pen pen = new(CustomColors.ControlBorder, 1);
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
            // Draw element background
            using SolidBrush brush = new(GetElementColor(element.Type));
            using Pen pen = new(Color.Gray, 1);
            g.FillRectangle(brush, element.Bounds);
            g.DrawRectangle(pen, element.Bounds);

            // Draw element content based on type
            if (element.Type == ReportElementType.TextLabel)
            {
                DrawTextElementContent(g, element);
            }
            else
            {
                // For other elements, draw default label
                using Font defaultFont = new("Segoe UI", 9);
                using SolidBrush defaultTextBrush = new(Color.Black);
                StringFormat defaultFormat = new()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                string displayText = element.DisplayName ?? element.Type.ToString();
                g.DrawString(displayText, defaultFont, defaultTextBrush, element.Bounds, defaultFormat);
            }
        }
        private static void DrawTextElementContent(Graphics g, ReportElement element)
        {
            // Get text properties
            string text = element.Data?.ToString() ?? "Text";
            string fontFamily = element.Properties.ContainsKey("FontFamily") ?
                element.Properties["FontFamily"].ToString() : "Segoe UI";
            float fontSize = element.Properties.ContainsKey("FontSize") ?
                Convert.ToSingle(element.Properties["FontSize"]) : 12f;
            FontStyle fontStyle = element.Properties.ContainsKey("FontStyle") ?
                (FontStyle)element.Properties["FontStyle"] : FontStyle.Regular;
            Color textColor = element.Properties.ContainsKey("TextColor") ?
                (Color)element.Properties["TextColor"] : Color.Black;
            StringAlignment alignment = element.Properties.ContainsKey("Alignment") ?
                (StringAlignment)element.Properties["Alignment"] : StringAlignment.Near;

            // Create font and draw text
            try
            {
                using Font font = new(fontFamily, fontSize, fontStyle);
                using SolidBrush textBrush = new(textColor);
                StringFormat format = new()
                {
                    Alignment = alignment,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                g.DrawString(text, font, textBrush, element.Bounds, format);
            }
            catch
            {
                // Fallback to default font if there's an error
                using Font fallbackFont = new("Segoe UI", 12, FontStyle.Regular);
                using SolidBrush fallbackBrush = new(Color.Black);
                g.DrawString(text, fallbackFont, fallbackBrush, element.Bounds);
            }
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
                    Data = GetDefaultChartType(),
                    Properties = []
                },
                "text" => new ReportElement
                {
                    Type = ReportElementType.TextLabel,
                    DisplayName = "Text Label",
                    Bounds = new Rectangle(location, new Size(150, 30)),
                    Data = "Sample Text",
                    Properties = new Dictionary<string, object>
                    {
                        ["FontFamily"] = "Segoe UI",
                        ["FontSize"] = 12f,
                        ["FontStyle"] = FontStyle.Regular,
                        ["TextColor"] = Color.Black,
                        ["Alignment"] = StringAlignment.Near
                    }
                },
                "daterange" => new ReportElement
                {
                    Type = ReportElementType.DateRange,
                    DisplayName = "Date Range",
                    Bounds = new Rectangle(location, new Size(200, 30)),
                    Properties = []
                },
                "summary" => new ReportElement
                {
                    Type = ReportElementType.Summary,
                    DisplayName = "Summary",
                    Bounds = new Rectangle(location, new Size(300, 100)),
                    Properties = []
                },
                _ => null
            };
        }
        private MainMenu_Form.ChartDataType GetDefaultChartType()
        {
            // Use the first selected chart type, or default to TotalSales
            return ReportConfig?.Filters?.SelectedChartTypes?.FirstOrDefault() ?? MainMenu_Form.ChartDataType.TotalSales;
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
            // Clear existing property controls
            PropertiesContainer_Panel.Controls.Clear();
            _propertyControls.Clear();

            if (_selectedElement != null)
            {
                ElementProperties_Label.Text = $"Selected: {_selectedElement.DisplayName}";
                ElementProperties_Label.Visible = true;

                // Create property controls based on element type
                CreatePropertyControls(_selectedElement);
            }
            else
            {
                ElementProperties_Label.Text = "No element selected";
                ElementProperties_Label.Visible = true;
            }
        }
        private void CreatePropertyControls(ReportElement element)
        {
            int yPosition = 10;
            const int rowHeight = 35;

            // Common properties for all elements
            AddPropertyLabel("Name:", yPosition);
            AddPropertyTextBox("Name", element.DisplayName ?? "", yPosition, (value) =>
            {
                element.DisplayName = value;
                Canvas_Panel.Invalidate();
                NotifyParentValidationChanged();  // Notify parent to update preview if visible
            });
            yPosition += rowHeight;

            AddPropertyLabel("X:", yPosition);
            AddPropertyNumericUpDown("X", element.Bounds.X, yPosition, (value) =>
            {
                Rectangle bounds = element.Bounds;
                bounds.X = (int)value;
                element.Bounds = bounds;
                Canvas_Panel.Invalidate();
            });
            yPosition += rowHeight;

            AddPropertyLabel("Y:", yPosition);
            AddPropertyNumericUpDown("Y", element.Bounds.Y, yPosition, (value) =>
            {
                Rectangle bounds = element.Bounds;
                bounds.Y = (int)value;
                element.Bounds = bounds;
                Canvas_Panel.Invalidate();
            });
            yPosition += rowHeight;

            AddPropertyLabel("Width:", yPosition);
            AddPropertyNumericUpDown("Width", element.Bounds.Width, yPosition, (value) =>
            {
                Rectangle bounds = element.Bounds;
                bounds.Width = Math.Max(50, (int)value);
                element.Bounds = bounds;
                Canvas_Panel.Invalidate();
            });
            yPosition += rowHeight;

            AddPropertyLabel("Height:", yPosition);
            AddPropertyNumericUpDown("Height", element.Bounds.Height, yPosition, (value) =>
            {
                Rectangle bounds = element.Bounds;
                bounds.Height = Math.Max(30, (int)value);
                element.Bounds = bounds;
                Canvas_Panel.Invalidate();
            });
            yPosition += rowHeight;

            // Type-specific properties
            switch (element.Type)
            {
                case ReportElementType.TextLabel:
                    yPosition = AddTextElementProperties(element, yPosition, rowHeight);
                    break;
                case ReportElementType.Chart:
                    yPosition = AddChartElementProperties(element, yPosition, rowHeight);
                    break;
            }

            // Add separator
            yPosition += 10;
            Panel separator = new()
            {
                BackColor = CustomColors.ControlBorder,
                Location = new Point(10, yPosition),
                Size = new Size(PropertiesContainer_Panel.Width - 20, 1)
            };
            PropertiesContainer_Panel.Controls.Add(separator);
            yPosition += 15;

            // Z-Order controls
            AddPropertyLabel("Layer:", yPosition);
            Guna2Button bringToFrontBtn = new()
            {
                Text = "Front",
                Size = new Size(60, 25),
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 8)
            };
            bringToFrontBtn.Click += (s, e) => BringElementToFront(element);
            PropertiesContainer_Panel.Controls.Add(bringToFrontBtn);

            Guna2Button sendToBackBtn = new()
            {
                Text = "Back",
                Size = new Size(60, 25),
                Location = new Point(150, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 8)
            };
            sendToBackBtn.Click += (s, e) => SendElementToBack(element);
            PropertiesContainer_Panel.Controls.Add(sendToBackBtn);
        }
        private int AddTextElementProperties(ReportElement element, int yPosition, int rowHeight)
        {
            // Text content
            AddPropertyLabel("Text:", yPosition);
            AddPropertyTextBox("Text", element.Data?.ToString() ?? "", yPosition, (value) =>
            {
                element.Data = value;
                Canvas_Panel.Invalidate();
            });
            yPosition += rowHeight;

            // Font family
            AddPropertyLabel("Font:", yPosition);
            string currentFont = element.Properties.ContainsKey("FontFamily") ?
                element.Properties["FontFamily"].ToString() : "Segoe UI";
            AddPropertyComboBox("FontFamily", currentFont, yPosition,
                ["Segoe UI", "Arial", "Times New Roman", "Calibri", "Verdana"],
                (value) =>
                {
                    element.Properties["FontFamily"] = value;
                    Canvas_Panel.Invalidate();
                });
            yPosition += rowHeight;

            // Font size
            AddPropertyLabel("Size:", yPosition);
            float currentSize = element.Properties.ContainsKey("FontSize") ?
                Convert.ToSingle(element.Properties["FontSize"]) : 12f;
            AddPropertyNumericUpDown("FontSize", (decimal)currentSize, yPosition, (value) =>
            {
                element.Properties["FontSize"] = (float)value;
                Canvas_Panel.Invalidate();
            }, 6, 72);
            yPosition += rowHeight;

            // Font style
            AddPropertyLabel("Style:", yPosition);
            AddFontStyleCheckBoxes(element, yPosition);
            yPosition += rowHeight;

            // Text alignment
            AddPropertyLabel("Align:", yPosition);
            StringAlignment currentAlign = element.Properties.ContainsKey("Alignment") ?
                (StringAlignment)element.Properties["Alignment"] : StringAlignment.Near;
            AddPropertyComboBox("Alignment", currentAlign.ToString(), yPosition,
                ["Near", "Center", "Far"],
                (value) =>
                {
                    element.Properties["Alignment"] = Enum.Parse<StringAlignment>(value);
                    Canvas_Panel.Invalidate();
                });
            yPosition += rowHeight;

            // Text color
            AddPropertyLabel("Color:", yPosition);
            AddColorPicker(element, yPosition);
            yPosition += rowHeight;

            return yPosition;
        }
        private int AddChartElementProperties(ReportElement element, int yPosition, int rowHeight)
        {
            // Chart type
            AddPropertyLabel("Chart:", yPosition);
            MainMenu_Form.ChartDataType currentType = element.Data is MainMenu_Form.ChartDataType chartType ?
                chartType : MainMenu_Form.ChartDataType.TotalSales;

            // Only show available chart types from selected charts
            List<string> availableCharts = ReportConfig?.Filters?.SelectedChartTypes?
                .Select(ct => ct.ToString()).ToList() ?? [];

            if (availableCharts.Count > 0)
            {
                AddPropertyComboBox("ChartType", currentType.ToString(), yPosition,
                    availableCharts.ToArray(),
                    (value) =>
                    {
                        element.Data = Enum.Parse<MainMenu_Form.ChartDataType>(value);
                        Canvas_Panel.Invalidate();
                    });
            }
            yPosition += rowHeight;

            return yPosition;
        }
        private void AddPropertyLabel(string text, int yPosition)
        {
            Label label = new()
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                ForeColor = CustomColors.Text,
                Location = new Point(10, yPosition + 3),
                Size = new Size(70, 20)
            };
            PropertiesContainer_Panel.Controls.Add(label);
        }
        private void AddPropertyTextBox(string name, string value, int yPosition, Action<string> onChange)
        {
            Guna2TextBox textBox = new()
            {
                Text = value,
                Size = new Size(180, 26),
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9)
            };
            textBox.TextChanged += (s, e) =>
            {
                if (!_isUpdating)
                {
                    onChange(textBox.Text);
                    TriggerPreviewRefresh();
                }
            };
            PropertiesContainer_Panel.Controls.Add(textBox);
            _propertyControls[name] = textBox;
        }
        private void AddPropertyNumericUpDown(string name, decimal value, int yPosition, Action<decimal> onChange, decimal min = 0, decimal max = 9999)
        {
            Guna2NumericUpDown numericUpDown = new()
            {
                Size = new Size(100, 26),
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9),
                Minimum = min,
                Maximum = max,
                Value = value
            };
            numericUpDown.ValueChanged += (s, e) =>
            {
                if (!_isUpdating)
                {
                    onChange(numericUpDown.Value);
                    TriggerPreviewRefresh();
                }
            };
            PropertiesContainer_Panel.Controls.Add(numericUpDown);
            _propertyControls[name] = numericUpDown;
        }
        private void AddPropertyComboBox(string name, string value, int yPosition, string[] items, Action<string> onChange)
        {
            Guna2ComboBox comboBox = new()
            {
                Size = new Size(180, 26),
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboBox.Items.AddRange(items);
            comboBox.SelectedItem = value;
            comboBox.SelectedIndexChanged += (s, e) =>
            {
                if (!_isUpdating && comboBox.SelectedItem != null)
                {
                    onChange(comboBox.SelectedItem.ToString());
                    TriggerPreviewRefresh();
                }
            };
            PropertiesContainer_Panel.Controls.Add(comboBox);
            _propertyControls[name] = comboBox;
        }
        private void AddFontStyleCheckBoxes(ReportElement element, int yPosition)
        {
            FontStyle currentStyle = element.Properties.ContainsKey("FontStyle") ?
                (FontStyle)element.Properties["FontStyle"] : FontStyle.Regular;

            CheckBox boldCheck = new()
            {
                Text = "B",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(35, 20),
                Location = new Point(85, yPosition),
                Checked = currentStyle.HasFlag(FontStyle.Bold)
            };
            boldCheck.CheckedChanged += (s, e) => UpdateFontStyle(element);
            PropertiesContainer_Panel.Controls.Add(boldCheck);
            _propertyControls["Bold"] = boldCheck;

            CheckBox italicCheck = new()
            {
                Text = "I",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Size = new Size(35, 20),
                Location = new Point(125, yPosition),
                Checked = currentStyle.HasFlag(FontStyle.Italic)
            };
            italicCheck.CheckedChanged += (s, e) => UpdateFontStyle(element);
            PropertiesContainer_Panel.Controls.Add(italicCheck);
            _propertyControls["Italic"] = italicCheck;

            CheckBox underlineCheck = new()
            {
                Text = "U",
                Font = new Font("Segoe UI", 9, FontStyle.Underline),
                Size = new Size(35, 20),
                Location = new Point(165, yPosition),
                Checked = currentStyle.HasFlag(FontStyle.Underline)
            };
            underlineCheck.CheckedChanged += (s, e) => UpdateFontStyle(element);
            PropertiesContainer_Panel.Controls.Add(underlineCheck);
            _propertyControls["Underline"] = underlineCheck;
        }
        private void UpdateFontStyle(ReportElement element)
        {
            if (_isUpdating) return;

            FontStyle style = FontStyle.Regular;
            if (_propertyControls.ContainsKey("Bold") && ((CheckBox)_propertyControls["Bold"]).Checked)
            {
                style |= FontStyle.Bold;
            }

            if (_propertyControls.ContainsKey("Italic") && ((CheckBox)_propertyControls["Italic"]).Checked)
            {
                style |= FontStyle.Italic;
            }

            if (_propertyControls.ContainsKey("Underline") && ((CheckBox)_propertyControls["Underline"]).Checked)
            {
                style |= FontStyle.Underline;
            }

            element.Properties["FontStyle"] = style;
            Canvas_Panel.Invalidate();
            TriggerPreviewRefresh();
        }
        private void AddColorPicker(ReportElement element, int yPosition)
        {
            Color currentColor = element.Properties.ContainsKey("TextColor") ?
                (Color)element.Properties["TextColor"] : Color.Black;

            Panel colorPreview = new()
            {
                BackColor = currentColor,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(50, 22),
                Location = new Point(85, yPosition),
                Cursor = Cursors.Hand
            };

            colorPreview.Click += (s, e) =>
            {
                ColorDialog colorDialog = new()
                {
                    Color = colorPreview.BackColor,
                    FullOpen = true
                };
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    colorPreview.BackColor = colorDialog.Color;
                    element.Properties["TextColor"] = colorDialog.Color;
                    Canvas_Panel.Invalidate();
                    TriggerPreviewRefresh();
                }
            };

            PropertiesContainer_Panel.Controls.Add(colorPreview);
            _propertyControls["ColorPreview"] = colorPreview;

            Label colorLabel = new()
            {
                Text = "Click to change",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(140, yPosition + 3),
                Size = new Size(100, 20)
            };
            PropertiesContainer_Panel.Controls.Add(colorLabel);
        }

        /// <summary>
        /// Triggers a refresh of the preview if the preview form is visible.
        /// </summary>
        private void TriggerPreviewRefresh()
        {
            // The preview will automatically regenerate when the user navigates to that step
            // For now, we just ensure the canvas is updated
            NotifyParentValidationChanged();
        }
        private void BringElementToFront(ReportElement element)
        {
            if (ReportConfig?.Elements == null) return;

            int maxZOrder = ReportConfig.Elements.Max(e => e.ZOrder);
            element.ZOrder = maxZOrder + 1;
            Canvas_Panel.Invalidate();
        }
        private void SendElementToBack(ReportElement element)
        {
            if (ReportConfig?.Elements == null) return;

            // Shift all other elements up by 1
            foreach (var e in ReportConfig.Elements.Where(e => e != element))
            {
                e.ZOrder++;
            }
            element.ZOrder = 0;
            Canvas_Panel.Invalidate();
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
                UpdatePropertiesPanel();
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
                UpdatePropertiesPanel();
            }
        }
        private void DeleteSelected(object sender, EventArgs e)
        {
            if (_selectedElement != null)
            {
                ReportConfig?.RemoveElement(_selectedElement.Id);
                _selectedElement = null;
                Canvas_Panel.Invalidate();
                UpdatePropertiesPanel();
                NotifyParentValidationChanged();
            }
        }

        // Form implementation methods
        public bool IsValidForNextStep()
        {
            // Must have at least one element positioned on the canvas
            return ReportConfig?.Elements?.Count > 0;
        }
        public bool ValidateStep()
        {
            if (!IsValidForNextStep())
            {
                CustomMessageBox.Show(
                    "Empty Layout",
                    "Please add at least one element to your report layout.",
                    CustomMessageBoxIcon.Exclamation,
                    CustomMessageBoxButtons.Ok
                );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Synchronizes the canvas elements with the current chart selection.
        /// </summary>
        private void SynchronizeCanvasWithSelection()
        {
            // Check if selected charts have changed
            List<MainMenu_Form.ChartDataType> currentSelectedCharts = ReportConfig.Filters.SelectedChartTypes;

            // Get existing chart elements
            List<ReportElement> existingChartElements = ReportConfig.Elements
                .Where(e => e.Type == ReportElementType.Chart && e.Data is MainMenu_Form.ChartDataType)
                .ToList();

            // Extract chart types from existing elements
            HashSet<MainMenu_Form.ChartDataType> existingChartTypes = [.. existingChartElements.Select(e => (MainMenu_Form.ChartDataType)e.Data)];

            // Find charts that were removed from selection
            List<ReportElement> elementsToRemove = existingChartElements
                .Where(e => !currentSelectedCharts.Contains((MainMenu_Form.ChartDataType)e.Data))
                .ToList();

            // Find newly selected charts that don't have elements
            List<MainMenu_Form.ChartDataType> chartsToAdd = currentSelectedCharts
                .Where(ct => !existingChartTypes.Contains(ct))
                .ToList();

            // Remove elements for deselected charts
            foreach (ReportElement element in elementsToRemove)
            {
                ReportConfig.RemoveElement(element.Id);
            }

            // Add elements for newly selected charts
            if (chartsToAdd.Count > 0)
            {
                AddNewChartElements(chartsToAdd);
            }

            // If no elements exist at all, generate from template or selected charts
            if (ReportConfig.Elements.Count == 0)
            {
                if (!string.IsNullOrEmpty(ReportConfig.TemplateName))
                {
                    LoadElementsFromTemplate(currentSelectedCharts);
                }
                else if (currentSelectedCharts.Count > 0)
                {
                    // Generate elements for all selected charts
                    GenerateElementsFromTemplate();
                }
            }

            Canvas_Panel.Invalidate();
        }

        /// <summary>
        /// Loads elements from a template, filtered by selected charts.
        /// </summary>
        private void LoadElementsFromTemplate(List<MainMenu_Form.ChartDataType> selectedCharts)
        {
            ReportConfiguration template = ReportTemplates.CreateFromTemplate(ReportConfig.TemplateName);

            // Only add chart elements that match selected charts
            foreach (ReportElement element in template.Elements)
            {
                if (element.Type == ReportElementType.Chart && element.Data is MainMenu_Form.ChartDataType chartType)
                {
                    if (selectedCharts.Contains(chartType))
                    {
                        ReportConfig.AddElement(element.Clone());
                    }
                }
                else
                {
                    // Add non-chart elements from template
                    ReportConfig.AddElement(element.Clone());
                }
            }
        }

        /// <summary>
        /// Adds new chart elements to the canvas for the specified chart types.
        /// </summary>
        private void AddNewChartElements(List<MainMenu_Form.ChartDataType> chartTypes)
        {
            if (chartTypes == null || chartTypes.Count == 0) { return; }

            // Find a good position for new charts (avoid overlap)
            int x = 50;
            int y = 50;
            const int chartWidth = 350;
            const int chartHeight = 250;
            const int spacing = 20;

            // Get existing element bounds to avoid overlap
            List<Rectangle> existingBounds = ReportConfig.Elements
                .Where(e => e.IsVisible)
                .Select(e => e.Bounds)
                .ToList();

            foreach (MainMenu_Form.ChartDataType chartType in chartTypes)
            {
                // Find a non-overlapping position
                Rectangle proposedBounds = new(x, y, chartWidth, chartHeight);

                while (existingBounds.Any(b => b.IntersectsWith(proposedBounds)))
                {
                    x += chartWidth + spacing;
                    if (x + chartWidth > Canvas_Panel.Width - 50)
                    {
                        x = 50;
                        y += chartHeight + spacing;
                    }
                    proposedBounds = new Rectangle(x, y, chartWidth, chartHeight);
                }

                // Create the new element
                ReportElement element = new()
                {
                    Type = ReportElementType.Chart,
                    Data = chartType,
                    DisplayName = GetChartDisplayName(chartType),
                    Bounds = proposedBounds
                };

                ReportConfig.AddElement(element);
                existingBounds.Add(proposedBounds);

                // Move to next position for the next chart
                x += chartWidth + spacing;
                if (x + chartWidth > Canvas_Panel.Width - 50)
                {
                    x = 50;
                    y += chartHeight + spacing;
                }
            }
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

            foreach (MainMenu_Form.ChartDataType chartType in ReportConfig.Filters.SelectedChartTypes)
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
        private static string GetChartDisplayName(MainMenu_Form.ChartDataType chartType)
        {
            return chartType switch
            {
                MainMenu_Form.ChartDataType.TotalSales => "Total Sales",
                MainMenu_Form.ChartDataType.TotalPurchases => "Total Purchases",
                MainMenu_Form.ChartDataType.DistributionOfSales => "Sales Distribution",
                MainMenu_Form.ChartDataType.TotalExpensesVsSales => "Sales vs Expenses",
                MainMenu_Form.ChartDataType.GrowthRates => "Growth Rates",
                MainMenu_Form.ChartDataType.AverageTransactionValue => "Average Order Value",
                _ => chartType.ToString()
            };
        }

        // Helper methods for base functionality
        /// <summary>
        /// Notifies the parent form that validation state has changed.
        /// </summary>
        private void NotifyParentValidationChanged()
        {
            ParentReportForm?.OnChildFormValidationChanged();
        }

        /// <summary>
        /// Safely updates UI controls without triggering events.
        /// </summary>
        private void PerformUpdate(Action updateAction)
        {
            if (updateAction == null) { return; }

            _isUpdating = true;

            try
            {
                updateAction();
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}