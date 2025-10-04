using Guna.UI2.WinForms;
using Sales_Tracker.ReportGenerator.Elements;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;
using System.Drawing.Drawing2D;
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
        private readonly List<BaseElement> _selectedElements = [];
        private bool _isMultiSelecting = false;
        private Rectangle _selectionRectangle;
        private Point _selectionStartPoint;

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
            StoreInitialSizes();
            SetToolTips();
            UpdateLayoutButtonStates();
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
        private void StoreInitialSizes()
        {
            _initialFormWidth = Width;
            _initialLeftPanelWidth = LeftTools_Panel.Width;
            _initialRightPanelWidth = RightCanvas_Panel.Width;
        }
        private void SetToolTips()
        {
            CustomTooltip.SetToolTip(AddChartElement_Button, "", "Add a chart to the report");
            CustomTooltip.SetToolTip(AddTableElement_Button, "", "Add a text label");
            CustomTooltip.SetToolTip(AddDateElement_Button, "", "Add date range display");
            CustomTooltip.SetToolTip(AddSummaryElement_Button, "", "Add summary statistics");
            CustomTooltip.SetToolTip(AddTableElement_Button, "", "Add transaction table");

            CustomTooltip.SetToolTip(AlignLeft_Button, "", "Align left");
            CustomTooltip.SetToolTip(AlignCenter_Button, "", "Align center");
            CustomTooltip.SetToolTip(AlignRight_Button, "", "Align right");
            CustomTooltip.SetToolTip(AlignTop_Button, "", "Align top");
            CustomTooltip.SetToolTip(AlignMiddle_Button, "", "Align middle");
            CustomTooltip.SetToolTip(AlignBottom_Button, "", "Align bottom");
            CustomTooltip.SetToolTip(DistributeHorizontally_Button, "", "Distribute horizontally");
            CustomTooltip.SetToolTip(DistributeVertically_Button, "", "Distribute vertically");
            CustomTooltip.SetToolTip(MakeSameWidth_Button, "", "Make same width");
            CustomTooltip.SetToolTip(MakeSameHeight_Button, "", "make same height");
            CustomTooltip.SetToolTip(MakeSameSize_Button, "", "Make same size");
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
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Multi-selection shortcuts
            if (keyData == (Keys.Control | Keys.A))
            {
                SelectAllElements();
                return true;
            }

            // Duplicate shortcut
            if (keyData == (Keys.Control | Keys.D))
            {
                DuplicateSelected();
                return true;
            }

            // Delete shortcut
            if (keyData == Keys.Delete)
            {
                DeleteSelected();
                return true;
            }

            // Alignment shortcuts
            if (keyData == (Keys.Control | Keys.Left))
            {
                AlignSelectedLeft();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Right))
            {
                AlignSelectedRight();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Up))
            {
                AlignSelectedTop();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Down))
            {
                AlignSelectedBottom();
                return true;
            }

            // Grid toggle
            if (keyData == (Keys.Control | Keys.G))
            {
                ToggleSnapToGrid();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Canvas event handlers
        private Point _mouseDownPoint;
        private const int DRAG_THRESHOLD = 5;
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

            // Draw elements
            Rectangle clipRect = e.ClipRectangle;
            DrawVisibleElements(e.Graphics, clipRect);

            DrawSelection(e.Graphics);
        }
        private void Canvas_Panel_MouseDown(object sender, MouseEventArgs e)
        {
            Canvas_Panel.Focus();

            if (e.Button == MouseButtons.Left)
            {
                bool ctrlPressed = (ModifierKeys & Keys.Control) == Keys.Control;

                // Check for resize handles on any selected element
                if (_selectedElements.Count > 0)
                {
                    foreach (BaseElement element in _selectedElements)
                    {
                        ResizeHandle handle = GetResizeHandleAt(e.Location, element);
                        if (handle != ResizeHandle.None)
                        {
                            _isResizing = true;
                            _activeResizeHandle = handle;
                            _selectedElement = element;
                            _originalBounds = element.Bounds;
                            _lastElementBounds = element.Bounds;
                            _dragStartPoint = e.Location;
                            _deferPropertyUpdate = true;
                            return;
                        }
                    }
                }
                // Check single selected element
                else if (_selectedElement != null)
                {
                    ResizeHandle handle = GetResizeHandleAt(e.Location, _selectedElement);
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

                BaseElement clickedElement = GetElementAtPoint(e.Location);

                if (clickedElement != null)
                {
                    if (ctrlPressed)
                    {
                        // Toggle selection
                        if (_selectedElements.Contains(clickedElement))
                        {
                            clickedElement.IsSelected = false;
                            _selectedElements.Remove(clickedElement);
                            if (_selectedElement == clickedElement)
                            {
                                _selectedElement = _selectedElements.FirstOrDefault();
                            }

                            Canvas_Panel.Invalidate();
                            UpdatePropertiesForSelection();
                            UpdateLayoutButtonStates();
                        }
                        else
                        {
                            SelectElement(clickedElement, true);
                        }
                    }
                    else
                    {
                        // Single selection or start drag
                        if (!_selectedElements.Contains(clickedElement))
                        {
                            SelectElement(clickedElement);
                        }
                        _isDragging = true;
                        _dragStartPoint = e.Location;
                    }
                }
                else
                {
                    // Clicked on empty area
                    _mouseDownPoint = e.Location;
                }
            }
        }
        private void Canvas_Panel_MouseMove(object sender, MouseEventArgs e)
        {
            // Handle cursor changes for resize handles when not dragging
            if (!_isResizing && !_isDragging && !_isMultiSelecting)
            {
                Cursor newCursor = Cursors.Default;

                // Check resize handles on selected elements
                if (_selectedElements.Count > 0)
                {
                    foreach (BaseElement element in _selectedElements)
                    {
                        ResizeHandle handle = GetResizeHandleAt(e.Location, element);
                        if (handle != ResizeHandle.None)
                        {
                            newCursor = GetCursorForHandle(handle);
                            break;
                        }
                    }
                }
                else if (_selectedElement != null)
                {
                    ResizeHandle handle = GetResizeHandleAt(e.Location, _selectedElement);
                    newCursor = GetCursorForHandle(handle);
                }

                if (Canvas_Panel.Cursor != newCursor)
                {
                    Canvas_Panel.Cursor = newCursor;
                }

                // Check if we should start rectangle selection from mouse down on empty area
                if (e.Button == MouseButtons.Left && !_isDragging && !_isMultiSelecting &&
                    _mouseDownPoint != Point.Empty && GetElementAtPoint(_mouseDownPoint) == null)
                {
                    // Check if mouse has moved beyond threshold
                    int deltaX = Math.Abs(e.X - _mouseDownPoint.X);
                    int deltaY = Math.Abs(e.Y - _mouseDownPoint.Y);

                    if (deltaX > DRAG_THRESHOLD || deltaY > DRAG_THRESHOLD)
                    {
                        // Start rectangle selection
                        bool ctrlPressed = (ModifierKeys & Keys.Control) == Keys.Control;
                        if (!ctrlPressed)
                        {
                            ClearAllSelections();
                        }

                        _isMultiSelecting = true;
                        _selectionStartPoint = _mouseDownPoint;
                        _selectionRectangle = new Rectangle(_mouseDownPoint, Size.Empty);
                        _mouseDownPoint = Point.Empty;  // Clear to prevent re-triggering
                    }
                }
            }

            // Handle multi-selection rectangle
            if (_isMultiSelecting)
            {
                // Update selection rectangle
                int x = Math.Min(_selectionStartPoint.X, e.X);
                int y = Math.Min(_selectionStartPoint.Y, e.Y);
                int width = Math.Abs(e.X - _selectionStartPoint.X);
                int height = Math.Abs(e.Y - _selectionStartPoint.Y);

                Rectangle oldRect = _selectionRectangle;
                _selectionRectangle = new Rectangle(x, y, width, height);

                // Invalidate the selection rectangle areas
                if (oldRect.Width > 0 || oldRect.Height > 0)
                {
                    oldRect.Inflate(2, 2);
                    Canvas_Panel.Invalidate(oldRect);
                }

                Rectangle newRect = _selectionRectangle;
                newRect.Inflate(2, 2);
                Canvas_Panel.Invalidate(newRect);

                UpdateSelectionFromRectangle();
            }
            // Handle resizing
            else if (_isResizing && _selectedElement != null)
            {
                ResizeElement(e.Location);
            }
            // Handle dragging multiple elements
            else if (_isDragging && _selectedElements.Count > 0)
            {
                // Move all selected elements together
                int deltaX = e.X - _dragStartPoint.X;
                int deltaY = e.Y - _dragStartPoint.Y;

                bool canMove = true;

                // Check if all elements can move
                foreach (BaseElement element in _selectedElements)
                {
                    Rectangle newBounds = element.Bounds;
                    newBounds.X += deltaX;
                    newBounds.Y += deltaY;

                    if (newBounds.X < 0 || newBounds.Right > Canvas_Panel.Width ||
                        newBounds.Y < 0 || newBounds.Bottom > Canvas_Panel.Height)
                    {
                        canMove = false;
                        break;
                    }
                }

                if (canMove)
                {
                    foreach (BaseElement element in _selectedElements)
                    {
                        Rectangle oldBounds = element.Bounds;
                        Rectangle newBounds = element.Bounds;
                        newBounds.X += deltaX;
                        newBounds.Y += deltaY;

                        // Apply grid snapping if enabled
                        if (_snapToGrid)
                        {
                            newBounds = SnapToGrid(newBounds);
                        }

                        element.Bounds = newBounds;

                        InvalidateElementRegion(oldBounds);
                        InvalidateElementRegion(newBounds);
                    }
                    _dragStartPoint = e.Location;

                    // Defer property update
                    _propertyUpdateTimer.Stop();
                    _propertyUpdateTimer.Start();
                }
            }
            // Handle dragging single element
            else if (_isDragging && _selectedElement != null)
            {
                int deltaX = e.X - _dragStartPoint.X;
                int deltaY = e.Y - _dragStartPoint.Y;

                Rectangle newBounds = _selectedElement.Bounds;
                newBounds.X += deltaX;
                newBounds.Y += deltaY;

                // Keep within canvas bounds
                if (newBounds.X >= 0 && newBounds.Right <= Canvas_Panel.Width &&
                    newBounds.Y >= 0 && newBounds.Bottom <= Canvas_Panel.Height)
                {
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
        private void Canvas_Panel_MouseUp(object sender, MouseEventArgs e)
        {
            bool wasInteracting = _isDragging || _isResizing || _isMultiSelecting;

            // If mouse up on empty area without dragging (small movement), clear selections
            if (e.Button == MouseButtons.Left && _mouseDownPoint != Point.Empty && !_isMultiSelecting)
            {
                bool ctrlPressed = (ModifierKeys & Keys.Control) == Keys.Control;
                if (!ctrlPressed)
                {
                    ClearAllSelections();
                }
            }

            _isDragging = false;
            _isResizing = false;
            _isMultiSelecting = false;
            _activeResizeHandle = ResizeHandle.None;
            _mouseDownPoint = Point.Empty;
            Canvas_Panel.Cursor = Cursors.Default;
            _deferPropertyUpdate = false;

            // Clear selection rectangle
            if (_selectionRectangle.Width > 0 || _selectionRectangle.Height > 0)
            {
                Rectangle clearRect = _selectionRectangle;
                clearRect.Inflate(2, 2);
                Canvas_Panel.Invalidate(clearRect);
                _selectionRectangle = Rectangle.Empty;
            }

            // Force full invalidation after multi-select to show resize handles for single selection
            if (wasInteracting)
            {
                Canvas_Panel.Invalidate();
            }

            // Update properties panel once at the end
            if (wasInteracting && (_selectedElement != null || _selectedElements.Count > 0))
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
                DeleteSelected();
            }
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

        // Alignment tool methods
        private void AlignSelectedLeft()
        {
            if (_selectedElements.Count < 2) { return; }

            int leftMost = _selectedElements.Min(e => e.Bounds.X);

            foreach (BaseElement element in _selectedElements)
            {
                Rectangle bounds = element.Bounds;
                bounds.X = leftMost;
                element.Bounds = bounds;
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }
        private void AlignSelectedCenter()
        {
            if (_selectedElements.Count < 2) { return; }

            // Find the center of all selected elements
            int leftMost = _selectedElements.Min(e => e.Bounds.X);
            int rightMost = _selectedElements.Max(e => e.Bounds.Right);
            int centerX = (leftMost + rightMost) / 2;

            foreach (BaseElement element in _selectedElements)
            {
                Rectangle bounds = element.Bounds;
                bounds.X = centerX - bounds.Width / 2;
                element.Bounds = bounds;
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }
        private void AlignSelectedRight()
        {
            if (_selectedElements.Count < 2) { return; }

            int rightMost = _selectedElements.Max(e => e.Bounds.Right);

            foreach (BaseElement element in _selectedElements)
            {
                Rectangle bounds = element.Bounds;
                bounds.X = rightMost - bounds.Width;
                element.Bounds = bounds;
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }
        private void AlignSelectedTop()
        {
            if (_selectedElements.Count < 2) { return; }

            int topMost = _selectedElements.Min(e => e.Bounds.Y);

            foreach (BaseElement element in _selectedElements)
            {
                Rectangle bounds = element.Bounds;
                bounds.Y = topMost;
                element.Bounds = bounds;
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }
        private void AlignSelectedMiddle()
        {
            if (_selectedElements.Count < 2) { return; }

            // Find the vertical center of all selected elements
            int topMost = _selectedElements.Min(e => e.Bounds.Y);
            int bottomMost = _selectedElements.Max(e => e.Bounds.Bottom);
            int centerY = (topMost + bottomMost) / 2;

            foreach (BaseElement element in _selectedElements)
            {
                Rectangle bounds = element.Bounds;
                bounds.Y = centerY - bounds.Height / 2;
                element.Bounds = bounds;
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }
        private void AlignSelectedBottom()
        {
            if (_selectedElements.Count < 2) { return; }

            int bottomMost = _selectedElements.Max(e => e.Bounds.Bottom);

            foreach (BaseElement element in _selectedElements)
            {
                Rectangle bounds = element.Bounds;
                bounds.Y = bottomMost - bounds.Height;
                element.Bounds = bounds;
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }

        // Distribution tool methods
        private void DistributeHorizontally()
        {
            if (_selectedElements.Count < 3) { return; }

            // Sort elements by X position
            List<BaseElement> sortedElements = _selectedElements.OrderBy(e => e.Bounds.X).ToList();

            int leftMost = sortedElements.First().Bounds.X;
            int rightMost = sortedElements.Last().Bounds.Right;
            int totalWidth = sortedElements.Sum(e => e.Bounds.Width);
            int totalSpace = rightMost - leftMost - totalWidth;

            if (sortedElements.Count > 1)
            {
                float spacing = (float)totalSpace / (sortedElements.Count - 1);
                float currentX = leftMost;

                foreach (BaseElement element in sortedElements)
                {
                    Rectangle bounds = element.Bounds;
                    bounds.X = (int)currentX;
                    element.Bounds = bounds;
                    currentX += bounds.Width + spacing;
                }
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }
        private void DistributeVertically()
        {
            if (_selectedElements.Count < 3) { return; }

            // Sort elements by Y position
            List<BaseElement> sortedElements = _selectedElements.OrderBy(e => e.Bounds.Y).ToList();

            int topMost = sortedElements.First().Bounds.Y;
            int bottomMost = sortedElements.Last().Bounds.Bottom;
            int totalHeight = sortedElements.Sum(e => e.Bounds.Height);
            int totalSpace = bottomMost - topMost - totalHeight;

            if (sortedElements.Count > 1)
            {
                float spacing = (float)totalSpace / (sortedElements.Count - 1);
                float currentY = topMost;

                foreach (BaseElement element in sortedElements)
                {
                    Rectangle bounds = element.Bounds;
                    bounds.Y = (int)currentY;
                    element.Bounds = bounds;
                    currentY += bounds.Height + spacing;
                }
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }

        // Sizing tool methods
        private void MakeSameWidth()
        {
            if (_selectedElements.Count < 2) { return; }

            // Use the first selected element's width as the reference
            int referenceWidth = _selectedElements.First().Bounds.Width;

            foreach (BaseElement element in _selectedElements)
            {
                Rectangle bounds = element.Bounds;
                bounds.Width = referenceWidth;
                element.Bounds = bounds;
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }
        private void MakeSameHeight()
        {
            if (_selectedElements.Count < 2) { return; }

            // Use the first selected element's height as the reference
            int referenceHeight = _selectedElements.First().Bounds.Height;

            foreach (BaseElement element in _selectedElements)
            {
                Rectangle bounds = element.Bounds;
                bounds.Height = referenceHeight;
                element.Bounds = bounds;
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }
        private void MakeSameSize()
        {
            if (_selectedElements.Count < 2) { return; }

            // Use the first selected element's size as the reference
            Size referenceSize = _selectedElements.First().Bounds.Size;

            foreach (BaseElement element in _selectedElements)
            {
                Rectangle bounds = element.Bounds;
                bounds.Size = referenceSize;
                element.Bounds = bounds;
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }

        // Z-Order tool methods
        private void BringElementToFront()
        {
            if (_selectedElements.Count == 0 || ReportConfig?.Elements == null) { return; }

            int maxZOrder = ReportConfig.Elements.Max(e => e.ZOrder);

            foreach (BaseElement? element in _selectedElements.OrderBy(e => e.ZOrder))
            {
                element.ZOrder = ++maxZOrder;
            }

            Canvas_Panel.Invalidate();
        }
        private void SendElementToBack()
        {
            if (_selectedElements.Count == 0 || ReportConfig?.Elements == null) { return; }

            // Shift all non-selected elements up
            int shiftAmount = _selectedElements.Count;
            foreach (BaseElement? element in ReportConfig.Elements.Where(e => !_selectedElements.Contains(e)))
            {
                element.ZOrder += shiftAmount;
            }

            // Set selected elements to bottom
            int zOrder = 0;
            foreach (BaseElement? element in _selectedElements.OrderBy(e => e.ZOrder))
            {
                element.ZOrder = zOrder++;
            }

            Canvas_Panel.Invalidate();
        }

        /// <summary>
        /// Updates the enabled state of layout buttons based on the number of selected elements.
        /// </summary>
        private void UpdateLayoutButtonStates()
        {
            int selectedCount = _selectedElements.Count;

            // Alignment buttons require at least 2 selected elements
            bool canAlign = selectedCount >= 2;
            AlignLeft_Button.Enabled = canAlign;
            AlignCenter_Button.Enabled = canAlign;
            AlignRight_Button.Enabled = canAlign;
            AlignTop_Button.Enabled = canAlign;
            AlignMiddle_Button.Enabled = canAlign;
            AlignBottom_Button.Enabled = canAlign;

            // Distribution buttons require at least 3 selected elements
            bool canDistribute = selectedCount >= 3;
            DistributeHorizontally_Button.Enabled = canDistribute;
            DistributeVertically_Button.Enabled = canDistribute;

            // Size buttons require at least 2 selected elements
            bool canResize = selectedCount >= 2;
            MakeSameWidth_Button.Enabled = canResize;
            MakeSameHeight_Button.Enabled = canResize;
            MakeSameSize_Button.Enabled = canResize;
        }

        // Grid Snapping
        private const int GRID_SIZE = 10;
        private bool _snapToGrid = false;
        private void ToggleSnapToGrid()
        {
            _snapToGrid = !_snapToGrid;
        }
        private Point SnapToGrid(Point point)
        {
            if (!_snapToGrid) { return point; }

            int x = (point.X / GRID_SIZE) * GRID_SIZE;
            int y = (point.Y / GRID_SIZE) * GRID_SIZE;
            return new Point(x, y);
        }
        private Rectangle SnapToGrid(Rectangle rect)
        {
            if (!_snapToGrid) { return rect; }

            Point snappedLocation = SnapToGrid(rect.Location);
            int width = ((rect.Width + GRID_SIZE / 2) / GRID_SIZE) * GRID_SIZE;
            int height = ((rect.Height + GRID_SIZE / 2) / GRID_SIZE) * GRID_SIZE;

            return new Rectangle(snappedLocation, new Size(width, height));
        }

        // Selection
        private void SelectElement(BaseElement element, bool addToSelection = false)
        {
            if (element == null) { return; }

            if (!addToSelection && !_isMultiSelecting)
            {
                // Clear existing selection
                foreach (BaseElement el in _selectedElements)
                {
                    el.IsSelected = false;
                }
                _selectedElements.Clear();
            }

            if (!_selectedElements.Contains(element))
            {
                element.IsSelected = true;
                _selectedElements.Add(element);
                _selectedElement = element;
            }

            Canvas_Panel.Invalidate();
            UpdatePropertiesForSelection();
            UpdateLayoutButtonStates();
        }
        private void SelectAllElements()
        {
            if (ReportConfig?.Elements == null) { return; }

            ClearAllSelections();

            foreach (BaseElement element in ReportConfig.Elements)
            {
                SelectElement(element, true);
            }

            Canvas_Panel.Invalidate();
            UpdateLayoutButtonStates();
        }
        private void DrawSelection(Graphics g)
        {
            if (_selectedElements.Count > 1)
            {
                // Draw selection rectangles for all selected elements
                DrawSelectionForElement(g, _selectedElements.ToArray());
            }
            else if (_selectedElement != null)
            {
                // Draw selection rectangle for the selected element
                if (_isMultiSelecting)
                {
                    DrawSelectionForElement(g, _selectedElement);
                }
                else
                {
                    DrawSelectionForElement(g, _selectedElement);
                    DrawResizeHandles(g, _selectedElement.Bounds);
                }
            }

            // Draw selection rectangle if clicking and draging for multi-select
            if (_isMultiSelecting)
            {
                using Pen selectionPen = new(CustomColors.AccentBlue, 2);
                selectionPen.DashStyle = DashStyle.Dash;
                g.DrawRectangle(selectionPen, _selectionRectangle);

                using SolidBrush selectionBrush = new(Color.FromArgb(30, CustomColors.AccentBlue));
                g.FillRectangle(selectionBrush, _selectionRectangle);
            }
        }
        private static void DrawSelectionForElement(Graphics g, params BaseElement[] elements)
        {
            foreach (BaseElement element in elements)
            {
                using Pen pen = new(CustomColors.AccentBlue, 3);
                pen.DashStyle = DashStyle.Solid;
                g.DrawRectangle(pen, element.Bounds);
            }
        }
        private void ClearAllSelections()
        {
            foreach (BaseElement element in _selectedElements)
            {
                element.IsSelected = false;
            }

            _selectedElements.Clear();
            _selectedElement = null;
            Canvas_Panel.Invalidate();
            HidePropertiesPanel();
            UpdateLayoutButtonStates();
        }
        private void UpdatePropertiesForSelection()
        {
            if (_selectedElements.Count == 0)
            {
                HidePropertiesPanel();
            }
            else if (_selectedElements.Count == 1)
            {
                CreateOrShowPropertiesPanel();
            }
            else
            {
                // Show multi-selection info
                ElementProperties_Label.Text = $"Selected: {_selectedElements.Count} elements";

                PropertiesContainer_Panel.Controls.Clear();
            }
        }
        private void UpdateSelectionFromRectangle()
        {
            if (ReportConfig?.Elements == null) { return; }

            // Track which elements should be selected based on rectangle intersection
            HashSet<BaseElement> elementsInRectangle = [];

            foreach (BaseElement element in ReportConfig.Elements)
            {
                if (_selectionRectangle.IntersectsWith(element.Bounds))
                {
                    elementsInRectangle.Add(element);
                }
            }

            // Process elements that should be selected but aren't
            foreach (BaseElement element in elementsInRectangle)
            {
                if (!_selectedElements.Contains(element))
                {
                    element.IsSelected = true;
                    _selectedElements.Add(element);
                    InvalidateElementRegion(element.Bounds);
                }
            }

            // Process elements that should be unselected
            List<BaseElement> toRemove = _selectedElements
                .Where(e => !elementsInRectangle.Contains(e))
                .ToList();

            foreach (BaseElement element in toRemove)
            {
                element.IsSelected = false;
                _selectedElements.Remove(element);
                InvalidateElementRegion(element.Bounds);
            }

            // Update the primary selected element
            if (_selectedElements.Count > 0 && !_selectedElements.Contains(_selectedElement))
            {
                _selectedElement = _selectedElements.First();
            }
            else if (_selectedElements.Count == 0)
            {
                _selectedElement = null;
            }

            UpdateLayoutButtonStates();
            UpdatePropertiesForSelection();
        }

        // Alignment tool event handlers
        private void AlignLeft_Button_Click(object sender, EventArgs e) => AlignSelectedLeft();
        private void AlignCenter_Button_Click(object sender, EventArgs e) => AlignSelectedCenter();
        private void AlignRight_Button_Click(object sender, EventArgs e) => AlignSelectedRight();
        private void AlignTop_Button_Click(object sender, EventArgs e) => AlignSelectedTop();
        private void AlignMiddle_Button_Click(object sender, EventArgs e) => AlignSelectedMiddle();
        private void AlignBottom_Button_Click(object sender, EventArgs e) => AlignSelectedBottom();
        private void DistributeHorizontally_Button_Click(object sender, EventArgs e) => DistributeHorizontally();
        private void DistributeVertically_Button_Click(object sender, EventArgs e) => DistributeVertically();
        private void MakeSameWidth_Button_Click(object sender, EventArgs e) => MakeSameWidth();
        private void MakeSameHeight_Button_Click(object sender, EventArgs e) => MakeSameHeight();
        private void MakeSameSize_Button_Click(object sender, EventArgs e) => MakeSameSize();

        // Property panel methods
        private void CreateOrShowPropertiesPanel()
        {
            if (_selectedElement == null)
            {
                HidePropertiesPanel();
                return;
            }

            ElementProperties_Label.Text = $"Selected: {_selectedElement.DisplayName}";

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
                CreateElementSpecificControls(yPosition);

                // Only set the theme of it's controls, not the panel itself
                List<Control> controls = PropertiesContainer_Panel.Controls.Cast<Control>().ToList();
                ThemeManager.SetThemeForControls(controls);

                // Find all checkboxes inside PropertiesContainer_Panel
                foreach (Guna2CustomCheckBox cb in PropertiesContainer_Panel.Controls.OfType<Guna2CustomCheckBox>())
                {
                    cb.UncheckedState.FillColor = CustomColors.MainBackground;
                }
            }

            UpdatePropertyValues();
        }
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
        private void HidePropertiesPanel()
        {
            ElementProperties_Label.Text = "No element selected";
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

        // Canvas methods
        private void TriggerPreviewRefresh()
        {
            NotifyParentValidationChanged();
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
            pen.DashStyle = DashStyle.Dot;

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

        // Resize element methods
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
        private static void DrawResizeHandles(Graphics g, Rectangle bounds)
        {
            const int handleSize = 12;
            using SolidBrush brush = new(CustomColors.AccentBlue);

            // Corner handles
            g.FillRectangle(brush, bounds.Left - handleSize / 2, bounds.Top - handleSize / 2, handleSize, handleSize);
            g.FillRectangle(brush, bounds.Right - handleSize / 2, bounds.Top - handleSize / 2, handleSize, handleSize);
            g.FillRectangle(brush, bounds.Left - handleSize / 2, bounds.Bottom - handleSize / 2, handleSize, handleSize);
            g.FillRectangle(brush, bounds.Right - handleSize / 2, bounds.Bottom - handleSize / 2, handleSize, handleSize);
        }
        private static ResizeHandle GetResizeHandleAt(Point point, BaseElement element)
        {
            if (element == null) { return ResizeHandle.None; }

            const int handleSize = 8;
            Rectangle bounds = element.Bounds;

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
            return new Rectangle(bounds.Left + handleSize, bounds.Bottom - handleSize / 2, bounds.Width - 2 * handleSize, handleSize).Contains(point)
                ? ResizeHandle.Bottom
                : ResizeHandle.None;
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

        // Element management
        private void CreateElementAtLocation(ReportElementType elementType, Point location)
        {
            BaseElement element = CreateElementByType(elementType, location);
            if (element != null)
            {
                ReportConfig?.AddElement(element);
                SelectElement(element);
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
        private void DuplicateSelected()
        {
            if (_selectedElements.Count == 0 || ReportConfig == null) { return; }

            List<BaseElement> duplicates = [];

            foreach (BaseElement element in _selectedElements)
            {
                BaseElement duplicate = element.Clone();

                // Offset the duplicate slightly
                Rectangle bounds = duplicate.Bounds;
                bounds.Offset(20, 20);
                duplicate.Bounds = bounds;

                ReportConfig.AddElement(duplicate);
                duplicates.Add(duplicate);
            }

            // Select the duplicated elements
            ClearAllSelections();
            foreach (BaseElement duplicate in duplicates)
            {
                SelectElement(duplicate, true);
            }

            Canvas_Panel.Invalidate();
            OnPropertyChanged();
        }
        private void DeleteSelected()
        {
            // Handle both old single-selection and multi-selection
            if (_selectedElements.Count == 0 && _selectedElement == null)
            {
                return;
            }

            if (ReportConfig == null)
            {
                return;
            }

            // Collect all elements to delete
            List<BaseElement> elementsToDelete = [];

            if (_selectedElements.Count > 0)
            {
                elementsToDelete.AddRange(_selectedElements);
            }
            else if (_selectedElement != null)
            {
                elementsToDelete.Add(_selectedElement);
            }

            // Delete all selected elements
            foreach (BaseElement element in elementsToDelete)
            {
                // If deleting a chart element, also remove it from SelectedChartTypes
                if (element is ChartElement chartElement)
                {
                    ReportConfig.Filters.SelectedChartTypes.Remove(chartElement.ChartType);
                }

                ReportConfig.RemoveElement(element.Id);
            }

            // Clear selection
            ClearAllSelections();
            Canvas_Panel.Invalidate();
            OnPropertyChanged();
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