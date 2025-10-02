using Guna.UI2.WinForms;
using Sales_Tracker.ReportGenerator.Elements;
using Sales_Tracker.Theme;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// Second step in report generation - drag-and-drop canvas for arranging report elements.
    /// </summary>
    public partial class ReportLayoutDesigner_Form : Form
    {
        // Properties
        private static ReportLayoutDesigner_Form _instance;
        private int _initialFormWidth;
        private int _initialLeftPanelWidth;
        private int _initialRightPanelWidth;
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private BaseElement _selectedElement;
        private bool _isResizing = false;
        private ResizeHandle _activeResizeHandle;
        private Rectangle _originalBounds;
        private Rectangle _lastElementBounds;
        private bool _deferPropertyUpdate = false;
        private Timer _propertyUpdateTimer;
        private Bitmap _gridCache = null;
        private Size _lastCanvasSize = Size.Empty;

        // Property controls caching
        private string _currentElementId = null;
        private readonly Dictionary<string, Control> _propertyControls = [];
        private readonly Dictionary<string, Action> _updateActions = [];

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

        // Getters
        public static ReportLayoutDesigner_Form Instance => _instance;

        // Init.
        public ReportLayoutDesigner_Form(ReportGenerator_Form parentForm)
        {
            InitializeComponent();
            _instance = this;
            ParentReportForm = parentForm;

            SetupCanvas();
            SetupToolsPanel();
            StoreInitialSizes();
        }
        private void SetupCanvas()
        {
            // Enable double buffering for smooth rendering
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, Canvas_Panel, [true]);

            // Configure canvas panel for drag-and-drop
            Canvas_Panel.AllowDrop = true;
            Canvas_Panel.DragEnter += Canvas_Panel_DragEnter;
            Canvas_Panel.DragDrop += Canvas_Panel_DragDrop;
            Canvas_Panel.Paint += Canvas_Panel_Paint;
            Canvas_Panel.MouseDown += Canvas_Panel_MouseDown;
            Canvas_Panel.MouseMove += Canvas_Panel_MouseMove;
            Canvas_Panel.MouseUp += Canvas_Panel_MouseUp;
            Canvas_Panel.KeyDown += Canvas_Panel_KeyDown;

            // Set canvas backgrounds
            Canvas_Panel.BackColor = Color.White;

            // Initialize property update timer
            _propertyUpdateTimer = new Timer
            {
                Interval = 100  // 100ms delay
            };
            _propertyUpdateTimer.Tick += (s, e) =>
            {
                _propertyUpdateTimer.Stop();
                if (!_deferPropertyUpdate)
                {
                    UpdatePropertyValues();
                }
            };
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

            AddToolButton("Table", "Add transaction table", yPosition, AddTableElement);
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

            AddToolButton("Align Right", "Align selected elements to the right", yPosition, AlignRight);
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
            if (e.Data.GetDataPresent(typeof(ReportElementType)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void Canvas_Panel_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ReportElementType)))
            {
                if (e.Data.GetData(typeof(ReportElementType)) is ReportElementType elementType)
                {
                    Point dropLocation = Canvas_Panel.PointToClient(new Point(e.X, e.Y));
                    CreateElementAtLocation(elementType, dropLocation);
                }
            }
        }
        private void Canvas_Panel_Paint(object sender, PaintEventArgs e)
        {
            // Use clipping region for better performance
            e.Graphics.SetClip(e.ClipRectangle);

            // Draw cached grid if size hasn't changed
            if (_gridCache == null || _lastCanvasSize != Canvas_Panel.Size)
            {
                CreateGridCache();
                _lastCanvasSize = Canvas_Panel.Size;
            }

            if (_gridCache != null)
            {
                e.Graphics.DrawImage(_gridCache, 0, 0);
            }

            // Draw only visible elements
            Rectangle clipRect = e.ClipRectangle;
            DrawVisibleElements(e.Graphics, clipRect);
            DrawSelection(e.Graphics);
        }
        private void Canvas_Panel_MouseDown(object sender, MouseEventArgs e)
        {
            // Ensure canvas has focus to receive keyboard events
            Canvas_Panel.Focus();

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
                        _lastElementBounds = _selectedElement.Bounds;
                        _dragStartPoint = e.Location;
                        _deferPropertyUpdate = true;
                        return;
                    }
                }

                // Check if clicking on an element
                BaseElement clickedElement = GetElementAtPoint(e.Location);
                if (clickedElement != null)
                {
                    SelectElement(clickedElement);
                    _isDragging = true;
                    _dragStartPoint = e.Location;
                    _lastElementBounds = clickedElement.Bounds;
                    _deferPropertyUpdate = true;
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
                    Cursor newCursor = GetCursorForHandle(handle);
                    if (Canvas_Panel.Cursor != newCursor)
                    {
                        Canvas_Panel.Cursor = newCursor;
                    }
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
                        // Invalidate only the affected regions
                        InvalidateElementRegion(_selectedElement.Bounds);
                        _selectedElement.Bounds = newBounds;
                        InvalidateElementRegion(newBounds);

                        _dragStartPoint = e.Location;

                        // Defer property panel update
                        _propertyUpdateTimer.Stop();
                        _propertyUpdateTimer.Start();
                    }
                }
            }
        }
        private void Canvas_Panel_MouseUp(object sender, MouseEventArgs e)
        {
            bool wasInteracting = _isDragging || _isResizing;

            _isDragging = false;
            _isResizing = false;
            _activeResizeHandle = ResizeHandle.None;
            Canvas_Panel.Cursor = Cursors.Default;
            _deferPropertyUpdate = false;

            // Update properties panel once at the end
            if (wasInteracting && _selectedElement != null)
            {
                _propertyUpdateTimer.Stop();
                UpdatePropertyValues();
                NotifyParentValidationChanged();
            }
        }
        private void Canvas_Panel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && _selectedElement != null)
            {
                ReportConfig?.RemoveElement(_selectedElement.Id);
                _selectedElement = null;
                Canvas_Panel.Invalidate();
                HidePropertiesPanel();
                NotifyParentValidationChanged();
            }
        }

        // Canvas methods
        private void SelectElement(BaseElement element)
        {
            if (_selectedElement == element) { return; }

            Rectangle? oldSelectionBounds = null;
            if (_selectedElement != null)
            {
                _selectedElement.IsSelected = false;
                oldSelectionBounds = _selectedElement.Bounds;
            }

            _selectedElement = element;
            element.IsSelected = true;

            // Invalidate only the selection regions
            if (oldSelectionBounds.HasValue)
            {
                InvalidateElementRegion(oldSelectionBounds.Value);
            }
            InvalidateElementRegion(element.Bounds);

            if (!_deferPropertyUpdate)
            {
                // Create or show property controls for this element
                CreateOrShowPropertiesPanel();
            }
        }
        private void ClearSelection()
        {
            if (_selectedElement != null)
            {
                Rectangle oldBounds = _selectedElement.Bounds;
                _selectedElement.IsSelected = false;
                _selectedElement = null;

                InvalidateElementRegion(oldBounds);

                if (!_deferPropertyUpdate)
                {
                    HidePropertiesPanel();
                }
            }
        }

        /// <summary>
        /// Creates property controls if they don't exist, or shows existing ones.
        /// </summary>
        private void CreateOrShowPropertiesPanel()
        {
            if (_selectedElement == null)
            {
                HidePropertiesPanel();
                return;
            }

            ElementProperties_Label.Text = $"Selected: {_selectedElement.DisplayName}";
            ElementProperties_Label.Visible = true;

            // Check if we need to create new controls
            if (_currentElementId != _selectedElement.Id)
            {
                // Clear old controls
                PropertiesContainer_Panel.Controls.Clear();
                _propertyControls.Clear();
                _updateActions.Clear();

                _currentElementId = _selectedElement.Id;

                // Create new property controls
                int yPosition = 10;

                // Create common property controls
                CreateCommonPropertyControls(yPosition);
                yPosition += BaseElement.RowHeight * 5;

                // Add element-specific properties
                yPosition = CreateElementSpecificControls(yPosition);

                // Z-Order controls
                CreateZOrderControls(yPosition);

                // Only set the theme of it's controls, not the panel itself
                List<Control> controls = PropertiesContainer_Panel.Controls.Cast<Control>().ToList();
                ThemeManager.SetThemeForControls(controls);
            }

            // Update control values
            UpdatePropertyValues();
        }

        /// <summary>
        /// Creates common property controls that all elements have.
        /// </summary>
        private void CreateCommonPropertyControls(int startY)
        {
            int yPosition = startY;

            // Name property
            BaseElement.AddPropertyLabel(PropertiesContainer_Panel, "Name:", yPosition);
            Guna2TextBox nameTextBox = BaseElement.AddPropertyTextBox(PropertiesContainer_Panel, _selectedElement?.DisplayName ?? "", yPosition,
                value =>
                {
                    if (_selectedElement != null)
                    {
                        _selectedElement.DisplayName = value;
                        Canvas_Panel.Invalidate();
                        NotifyParentValidationChanged();
                    }
                });
            _propertyControls["Name"] = nameTextBox;
            _updateActions["Name"] = () => nameTextBox.Text = _selectedElement?.DisplayName ?? "";
            yPosition += BaseElement.RowHeight;

            // X position
            BaseElement.AddPropertyLabel(PropertiesContainer_Panel, "X:", yPosition);
            Guna2NumericUpDown xNumeric = BaseElement.AddPropertyNumericUpDown(PropertiesContainer_Panel, _selectedElement?.Bounds.X ?? 0, yPosition,
                value =>
                {
                    if (_selectedElement != null)
                    {
                        Rectangle bounds = _selectedElement.Bounds;
                        bounds.X = (int)value;
                        _selectedElement.Bounds = bounds;
                        Canvas_Panel.Invalidate();
                    }
                }, 0, 9999);
            _propertyControls["X"] = xNumeric;
            _updateActions["X"] = () => xNumeric.Value = _selectedElement?.Bounds.X ?? 0;
            yPosition += BaseElement.RowHeight;

            // Y position
            BaseElement.AddPropertyLabel(PropertiesContainer_Panel, "Y:", yPosition);
            Guna2NumericUpDown yNumeric = BaseElement.AddPropertyNumericUpDown(PropertiesContainer_Panel, _selectedElement?.Bounds.Y ?? 0, yPosition,
                value =>
                {
                    if (_selectedElement != null)
                    {
                        Rectangle bounds = _selectedElement.Bounds;
                        bounds.Y = (int)value;
                        _selectedElement.Bounds = bounds;
                        Canvas_Panel.Invalidate();
                    }
                }, 0, 9999);
            _propertyControls["Y"] = yNumeric;
            _updateActions["Y"] = () => yNumeric.Value = _selectedElement?.Bounds.Y ?? 0;
            yPosition += BaseElement.RowHeight;

            // Width
            BaseElement.AddPropertyLabel(PropertiesContainer_Panel, "Width:", yPosition);
            Guna2NumericUpDown widthNumeric = BaseElement.AddPropertyNumericUpDown(PropertiesContainer_Panel, _selectedElement?.Bounds.Width ?? 100, yPosition,
                value =>
                {
                    if (_selectedElement != null)
                    {
                        Rectangle bounds = _selectedElement.Bounds;
                        bounds.Width = Math.Max(50, (int)value);
                        _selectedElement.Bounds = bounds;
                        Canvas_Panel.Invalidate();
                    }
                }, 50, 9999);
            _propertyControls["Width"] = widthNumeric;
            _updateActions["Width"] = () => widthNumeric.Value = _selectedElement?.Bounds.Width ?? 100;
            yPosition += BaseElement.RowHeight;

            // Height
            BaseElement.AddPropertyLabel(PropertiesContainer_Panel, "Height:", yPosition);
            Guna2NumericUpDown heightNumeric = BaseElement.AddPropertyNumericUpDown(PropertiesContainer_Panel, _selectedElement?.Bounds.Height ?? 100, yPosition,
                value =>
                {
                    if (_selectedElement != null)
                    {
                        Rectangle bounds = _selectedElement.Bounds;
                        bounds.Height = Math.Max(30, (int)value);
                        _selectedElement.Bounds = bounds;
                        Canvas_Panel.Invalidate();
                    }
                }, 30, 9999);
            _propertyControls["Height"] = heightNumeric;
            _updateActions["Height"] = () => heightNumeric.Value = _selectedElement?.Bounds.Height ?? 100;
        }

        /// <summary>
        /// Creates element-specific property controls.
        /// </summary>
        private int CreateElementSpecificControls(int yPosition)
        {
            if (_selectedElement == null) { return yPosition; }

            // Let the element create its own specific controls
            // We'll need to modify the element classes to support this caching approach
            return _selectedElement.CreatePropertyControls(
                PropertiesContainer_Panel,
                yPosition,
                OnPropertyChanged);
        }

        /// <summary>
        /// Creates Z-order controls.
        /// </summary>
        private void CreateZOrderControls(int yPosition)
        {
            BaseElement.AddPropertyLabel(PropertiesContainer_Panel, "Layer:", yPosition);

            Guna2Button bringToFrontBtn = new()
            {
                Text = "Front",
                Size = new Size(60, 25),
                Location = new Point(85, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 8)
            };
            bringToFrontBtn.Click += (s, e) => BringElementToFront(_selectedElement);
            PropertiesContainer_Panel.Controls.Add(bringToFrontBtn);

            Guna2Button sendToBackBtn = new()
            {
                Text = "Back",
                Size = new Size(60, 25),
                Location = new Point(150, yPosition),
                BorderRadius = 2,
                Font = new Font("Segoe UI", 8)
            };
            sendToBackBtn.Click += (s, e) => SendElementToBack(_selectedElement);
            PropertiesContainer_Panel.Controls.Add(sendToBackBtn);
        }

        /// <summary>
        /// Updates the values of existing property controls.
        /// </summary>
        private void UpdatePropertyValues()
        {
            if (_selectedElement == null || _currentElementId != _selectedElement.Id)
            {
                CreateOrShowPropertiesPanel();
                return;
            }

            // Update all registered property controls
            foreach (Action updateAction in _updateActions.Values)
            {
                updateAction?.Invoke();
            }
        }

        /// <summary>
        /// Hides the properties panel when no element is selected.
        /// </summary>
        private void HidePropertiesPanel()
        {
            ElementProperties_Label.Text = "No element selected";
            ElementProperties_Label.Visible = true;
            PropertiesContainer_Panel.Controls.Clear();
            _propertyControls.Clear();
            _updateActions.Clear();
            _currentElementId = null;
        }
        private void OnPropertyChanged()
        {
            Canvas_Panel.Invalidate();
            TriggerPreviewRefresh();
        }
        private void TriggerPreviewRefresh()
        {
            NotifyParentValidationChanged();
        }
        private void BringElementToFront(BaseElement element)
        {
            if (ReportConfig?.Elements == null || element == null) { return; }

            int maxZOrder = ReportConfig.Elements.Max(e => e.ZOrder);
            element.ZOrder = maxZOrder + 1;
            Canvas_Panel.Invalidate();
        }
        private void SendElementToBack(BaseElement element)
        {
            if (ReportConfig?.Elements == null || element == null) { return; }

            // Shift all other elements up by 1
            foreach (BaseElement? e in ReportConfig.Elements.Where(e => e != element))
            {
                e.ZOrder++;
            }
            element.ZOrder = 0;
            Canvas_Panel.Invalidate();
        }
        private void InvalidateElementRegion(Rectangle bounds)
        {
            // Inflate the bounds slightly to include borders and handles
            Rectangle invalidateRect = bounds;
            invalidateRect.Inflate(10, 10);
            Canvas_Panel.Invalidate(invalidateRect);
        }
        private void DrawVisibleElements(Graphics g, Rectangle clipRect)
        {
            if (ReportConfig?.Elements == null) { return; }

            foreach (BaseElement element in ReportConfig.Elements.Where(e => e.IsVisible))
            {
                // Only draw elements that intersect with the clip rectangle
                if (clipRect.IntersectsWith(element.Bounds))
                {
                    element.DrawDesignerElement(g);
                }
            }
        }
        private void CreateGridCache()
        {
            _gridCache?.Dispose();
            _gridCache = new Bitmap(Canvas_Panel.Width, Canvas_Panel.Height);

            using Graphics g = Graphics.FromImage(_gridCache);
            g.Clear(Canvas_Panel.BackColor);
            DrawGrid(g);
        }
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
        private void DrawSelection(Graphics g)
        {
            if (_selectedElement != null)
            {
                using Pen pen = new(CustomColors.AccentBlue);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(pen, _selectedElement.Bounds);

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

        // Resize element methods
        private ResizeHandle GetResizeHandleAt(Point point)
        {
            if (_selectedElement == null) { return ResizeHandle.None; }

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
            if (_selectedElement == null) { return; }

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
                // Invalidate only the affected regions
                Rectangle unionRect = Rectangle.Union(_lastElementBounds, newBounds);
                unionRect.Inflate(10, 10);  // Add padding for resize handles
                Canvas_Panel.Invalidate(unionRect);

                _selectedElement.Bounds = newBounds;
                _lastElementBounds = newBounds;

                // Defer property update
                _propertyUpdateTimer.Stop();
                _propertyUpdateTimer.Start();
            }
        }

        // Element management
        private void CreateElementAtLocation(ReportElementType elementType, Point location)
        {
            BaseElement element = CreateElementByType(elementType, location);
            if (element != null)
            {
                ReportConfig?.AddElement(element);
                Canvas_Panel.Invalidate();
                NotifyParentValidationChanged();
            }
        }
        private BaseElement? CreateElementByType(ReportElementType elementType, Point location)
        {
            return elementType switch
            {
                ReportElementType.Chart => new ChartElement
                {
                    DisplayName = "Chart",
                    Bounds = new Rectangle(location, new Size(350, 250)),
                    ChartType = GetDefaultChartType()
                },
                ReportElementType.TextLabel => new TextLabelElement
                {
                    DisplayName = "Text Label",
                    Bounds = new Rectangle(location, new Size(200, 30)),
                    Text = "Sample Text"
                },
                ReportElementType.DateRange => new DateRangeElement
                {
                    DisplayName = "Date Range",
                    Bounds = new Rectangle(location, new Size(250, 30))
                },
                ReportElementType.Summary => new SummaryElement
                {
                    DisplayName = "Summary",
                    Bounds = new Rectangle(location, new Size(300, 120))
                },
                ReportElementType.TransactionTable => new TransactionTableElement
                {
                    DisplayName = "Transaction Table",
                    Bounds = new Rectangle(location, new Size(400, 200))
                },
                _ => null
            };
        }
        private MainMenu_Form.ChartDataType GetDefaultChartType()
        {
            // Use the first selected chart type, or default to TotalSales
            return ReportConfig?.Filters?.SelectedChartTypes?.FirstOrDefault() ?? MainMenu_Form.ChartDataType.TotalSales;
        }
        private BaseElement? GetElementAtPoint(Point point)
        {
            if (ReportConfig?.Elements == null) { return null; }

            // Check elements in reverse Z-order (top to bottom)
            IOrderedEnumerable<BaseElement> sortedElements = ReportConfig.Elements
                .Where(e => e.IsVisible)
                .OrderByDescending(e => e.ZOrder);

            foreach (BaseElement? element in sortedElements)
            {
                if (element.Bounds.Contains(point))
                {
                    return element;
                }
            }

            return null;
        }

        // Tool event handlers
        private void AddChartElement(object sender, EventArgs e)
        {
            CreateElementAtLocation(ReportElementType.Chart, new Point(50, 50));
        }
        private void AddTextElement(object sender, EventArgs e)
        {
            CreateElementAtLocation(ReportElementType.TextLabel, new Point(50, 220));
        }
        private void AddDateRangeElement(object sender, EventArgs e)
        {
            CreateElementAtLocation(ReportElementType.DateRange, new Point(50, 280));
        }
        private void AddSummaryElement(object sender, EventArgs e)
        {
            CreateElementAtLocation(ReportElementType.Summary, new Point(50, 320));
        }
        private void AddTableElement(object sender, EventArgs e)
        {
            CreateElementAtLocation(ReportElementType.TransactionTable, new Point(50, 450));
        }
        private void AlignLeft(object sender, EventArgs e)
        {
            if (_selectedElement != null)
            {
                Rectangle bounds = _selectedElement.Bounds;
                bounds.X = 20;
                _selectedElement.Bounds = bounds;
                Canvas_Panel.Invalidate();
                UpdatePropertyValues();
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
                UpdatePropertyValues();
            }
        }
        private void AlignRight(object sender, EventArgs e)
        {
            if (_selectedElement != null)
            {
                Rectangle bounds = _selectedElement.Bounds;
                bounds.X = Canvas_Panel.Width - bounds.Width - 20;
                _selectedElement.Bounds = bounds;
                Canvas_Panel.Invalidate();
                UpdatePropertyValues();
            }
        }
        private void DeleteSelected(object sender, EventArgs e)
        {
            if (_selectedElement != null)
            {
                ReportConfig?.RemoveElement(_selectedElement.Id);
                _selectedElement = null;
                Canvas_Panel.Invalidate();
                HidePropertiesPanel();
                NotifyParentValidationChanged();
            }
        }

        // Form implementation methods
        public bool IsValidForNextStep()
        {
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
            List<ChartElement> existingChartElements = ReportConfig.GetElementsOfType<ChartElement>();

            // Extract chart types from existing elements
            HashSet<MainMenu_Form.ChartDataType> existingChartTypes = [.. existingChartElements.Select(e => e.ChartType)];

            // Find charts that were removed from selection
            List<ChartElement> elementsToRemove = existingChartElements
                .Where(e => !currentSelectedCharts.Contains(e.ChartType))
                .ToList();

            // Find newly selected charts that don't have elements
            List<MainMenu_Form.ChartDataType> chartsToAdd = currentSelectedCharts
                .Where(ct => !existingChartTypes.Contains(ct))
                .ToList();

            // Remove elements for deselected charts
            foreach (ChartElement element in elementsToRemove)
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
                    GenerateElementsFromTemplate();
                }
            }

            Canvas_Panel.Invalidate();
        }
        private void LoadElementsFromTemplate(List<MainMenu_Form.ChartDataType> selectedCharts)
        {
            ReportConfiguration template = ReportTemplates.CreateFromTemplate(ReportConfig.TemplateName);

            // Only add chart elements that match selected charts
            foreach (BaseElement element in template.Elements)
            {
                if (element is ChartElement chartElement && selectedCharts.Contains(chartElement.ChartType))
                {
                    ReportConfig.AddElement(element.Clone());
                }
                else if (element is not ChartElement)
                {
                    // Add non-chart elements from template
                    ReportConfig.AddElement(element.Clone());
                }
            }
        }
        private void AddNewChartElements(List<MainMenu_Form.ChartDataType> chartTypes)
        {
            if (chartTypes == null || chartTypes.Count == 0) { return; }

            int x = 50;
            int y = 50;
            const int chartWidth = 350;
            const int chartHeight = 250;
            const int spacing = 20;

            List<Rectangle> existingBounds = ReportConfig.Elements
                .Where(e => e.IsVisible)
                .Select(e => e.Bounds)
                .ToList();

            foreach (MainMenu_Form.ChartDataType chartType in chartTypes)
            {
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

                ChartElement element = new()
                {
                    ChartType = chartType,
                    DisplayName = GetChartDisplayName(chartType),
                    Bounds = proposedBounds
                };

                ReportConfig.AddElement(element);
                existingBounds.Add(proposedBounds);

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

            int x = 50, y = 50;
            const int chartWidth = 350;
            const int chartHeight = 250;
            const int spacing = 20;
            const int maxWidth = 800;

            foreach (MainMenu_Form.ChartDataType chartType in ReportConfig.Filters.SelectedChartTypes)
            {
                ChartElement element = new()
                {
                    ChartType = chartType,
                    DisplayName = GetChartDisplayName(chartType),
                    Bounds = new Rectangle(x, y, chartWidth, chartHeight)
                };

                ReportConfig.AddElement(element);

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

        // Helper methods
        private void NotifyParentValidationChanged()
        {
            ParentReportForm?.OnChildFormValidationChanged();
        }
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