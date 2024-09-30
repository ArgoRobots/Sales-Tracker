namespace Sales_Tracker.UI
{
    public class CenteredFlowLayoutPanel : Panel
    {
        // Properties
        private int _spacing = 10;

        // Getters and setters
        public int Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing != value)
                {
                    _spacing = value;
                    Invalidate();  // Redraw the control
                }
            }
        }

        // Init.
        public CenteredFlowLayoutPanel()
        {
            // Enable double buffering to reduce flicker
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);
            UpdateStyles();

            SetFlowLayoutPanel();
        }
        private void SetFlowLayoutPanel()
        {
            HorizontalScroll.Maximum = 0;
            HorizontalScroll.Enabled = false;
            HorizontalScroll.Visible = false;
            AutoScroll = true;
        }

        // Methods
        private void CenterControls()
        {
            if (Controls.Count == 0) { return; }

            int y = AutoScrollPosition.Y;
            int rowHeight = 0;
            int totalControls = Controls.Count;

            int clientWidth = ClientSize.Width;
            if (VerticalScroll.Visible)
            {
                clientWidth -= SystemInformation.VerticalScrollBarWidth;
            }

            int startIndex = 0;

            while (startIndex < totalControls)
            {
                int currentRowWidth = 0;
                int endIndex = startIndex;

                // Calculate how many controls fit in the current row
                while (endIndex < totalControls)
                {
                    Control ctrl = Controls[endIndex];
                    int ctrlWidth = ctrl.Width + _spacing;

                    if (currentRowWidth + ctrlWidth - _spacing > clientWidth && currentRowWidth > 0)
                    {
                        break;
                    }

                    currentRowWidth += ctrlWidth;
                    endIndex++;
                }

                // Center the row
                int x = Math.Max(0, (clientWidth - (currentRowWidth - _spacing)) / 2);

                // Layout the controls in the current row
                for (int i = startIndex; i < endIndex; i++)
                {
                    Control ctrl = Controls[i];
                    ctrl.Location = new Point(x, y);
                    x += ctrl.Width + _spacing;
                    rowHeight = Math.Max(rowHeight, ctrl.Height);
                }

                // Move to the next row
                y += rowHeight + _spacing;
                rowHeight = 0;
                startIndex = endIndex;
            }

            // Prevents the horizontal scrollbar from flashing in some cases
            SetFlowLayoutPanel();
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterControls();
        }
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            CenterControls();
        }
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            CenterControls();
        }
    }
}