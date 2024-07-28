namespace Sales_Tracker.Classes
{
    internal class DataGridViewImageHeaderCell(Image image, string headerText, string messageBoxText) : DataGridViewColumnHeaderCell
    {
        public Image HeaderImage { get; set; } = new Bitmap(image, new Size(15, 15));
        public int ImageOffsetX { get; set; } = -20;
        public int ImageOffsetY { get; set; } = 5;
        public string HeaderText { get; set; } = headerText;
        public string MessageBoxText { get; set; } = messageBoxText;

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            // Exclude default text painting
            paintParts &= ~DataGridViewPaintParts.ContentForeground;

            base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

            if (HeaderImage != null)
            {
                Point imgLocation = new(cellBounds.Right - HeaderImage.Width + ImageOffsetX, cellBounds.Y + ImageOffsetY);
                graphics.DrawImage(HeaderImage, imgLocation);
            }

            if (!string.IsNullOrEmpty(HeaderText))
            {
                // Define text formatting options
                StringFormat stringFormat = new()
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter,  // Adds ellipsis if text is too long
                    FormatFlags = StringFormatFlags.LineLimit  // Ensures text doesn't exceed the cell bounds
                };

                // Define the area where the text will be drawn
                RectangleF textBounds = new(
                    cellBounds.X + 5,
                    cellBounds.Y,
                    cellBounds.Width - HeaderImage.Width + ImageOffsetX - 5,
                    cellBounds.Height
                );

                // Use the cells ForeColor for text drawing
                using Brush textBrush = new SolidBrush(cellStyle.ForeColor);
                graphics.DrawString(HeaderText, cellStyle.Font, textBrush, textBounds, stringFormat);
            }
        }
        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (HeaderImage != null)
            {
                Rectangle cellBounds = DataGridView.GetCellDisplayRectangle(e.ColumnIndex, -1, true);
                Rectangle imgBounds = new(
                    cellBounds.Right - HeaderImage.Width + ImageOffsetX,
                    cellBounds.Y + ImageOffsetY,
                    HeaderImage.Width,
                    HeaderImage.Height
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