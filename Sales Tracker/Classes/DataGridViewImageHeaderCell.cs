namespace Sales_Tracker.Classes
{
    internal class DataGridViewImageHeaderCell(Image image, string headerText, string messageBoxText) : DataGridViewColumnHeaderCell
    {
        public Image HeaderImage { get; set; } = new Bitmap(image, new Size(15, 15));
        public int ImageOffsetX { get; set; } = -20;
        public int ImageOffsetY { get; set; } = 5;
        public string HeaderText { get; set; } = headerText;
        public string MessageBoxText { get; set; } = messageBoxText;
        private bool isImageHovered = false;
        private static readonly Color HoverBackgroundColor = Color.FromArgb(187, 187, 187);
        private const int Padding = 3;

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            // Exclude default text painting
            paintParts &= ~DataGridViewPaintParts.ContentForeground;

            base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

            if (HeaderImage != null)
            {
                Size imgSize = new(15, 15);
                Point imgLocation = new(cellBounds.Right - imgSize.Width + ImageOffsetX - Padding, cellBounds.Y + ImageOffsetY);

                // Draw background color if image is hovered
                if (isImageHovered)
                {
                    graphics.FillRectangle(new SolidBrush(HoverBackgroundColor), new Rectangle(imgLocation.X - Padding, imgLocation.Y - Padding, imgSize.Width + Padding * 2, imgSize.Height + Padding * 2));
                }

                graphics.DrawImage(HeaderImage, new Rectangle(imgLocation, imgSize));
            }

            if (!string.IsNullOrEmpty(HeaderText))
            {
                // Define text formatting options
                StringFormat stringFormat = new()
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.LineLimit
                };

                // Define the area where the text will be drawn
                RectangleF textBounds = new(
                    cellBounds.X + 5,
                    cellBounds.Y,
                    cellBounds.Width - (HeaderImage?.Width ?? 0) + ImageOffsetX - 5,
                    cellBounds.Height
                );

                // Use the cell style's ForeColor for text drawing, so it updates when the theme changes
                using Brush textBrush = new SolidBrush(cellStyle.ForeColor);
                graphics.DrawString(HeaderText, cellStyle.Font, textBrush, textBounds, stringFormat);
            }
        }
        protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (HeaderImage != null)
            {
                Rectangle cellBounds = DataGridView.GetCellDisplayRectangle(e.ColumnIndex, -1, true);
                Size imgSize = new(15, 15);
                Rectangle imgBounds = new(
                    cellBounds.Right - imgSize.Width + ImageOffsetX - Padding,
                    cellBounds.Y + ImageOffsetY - Padding,
                    imgSize.Width + Padding * 2,
                    imgSize.Height + Padding * 2
                );

                Point mouseLocation = new(e.X + cellBounds.X, e.Y + cellBounds.Y);

                bool isCurrentlyHovered = imgBounds.Contains(mouseLocation);
                if (isCurrentlyHovered != isImageHovered)
                {
                    isImageHovered = isCurrentlyHovered;
                    DataGridView.InvalidateCell(this);
                }
            }
        }
        protected override void OnMouseLeave(int rowIndex)
        {
            base.OnMouseLeave(rowIndex);
            if (isImageHovered)
            {
                isImageHovered = false;
                DataGridView.InvalidateCell(this);
            }
        }
        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (HeaderImage != null)
            {
                Rectangle cellBounds = DataGridView.GetCellDisplayRectangle(e.ColumnIndex, -1, true);
                Size imgSize = new(15, 15);
                Rectangle imgBounds = new(
                    cellBounds.Right - imgSize.Width + ImageOffsetX - Padding,
                    cellBounds.Y + ImageOffsetY - Padding,
                    imgSize.Width + Padding * 2,
                    imgSize.Height + Padding * 2
                );

                // Adjust the mouse location relative to the header cell's coordinate system
                Point clickLocation = new(e.X + cellBounds.X, e.Y + cellBounds.Y);

                if (imgBounds.Contains(clickLocation))
                {
                    CustomMessageBox.Show("Argo Sales Tracker", MessageBoxText, CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                }
            }
        }
    }
}