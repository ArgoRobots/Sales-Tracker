using Guna.UI2.WinForms;

namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Transaction table element for displaying tabular data.
    /// </summary>
    public class TransactionTableElement : BaseElement
    {
        public int MaxRows { get; set; } = 10;
        public bool ShowHeaders { get; set; } = true;
        public bool AlternateRowColors { get; set; } = true;
        public Color HeaderBackgroundColor { get; set; } = Color.Navy;
        public Color HeaderTextColor { get; set; } = Color.White;

        public override ReportElementType GetElementType() => ReportElementType.TransactionTable;
        public override BaseElement Clone()
        {
            return new TransactionTableElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                DisplayName = DisplayName + " (Copy)",
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                MaxRows = MaxRows,
                ShowHeaders = ShowHeaders,
                AlternateRowColors = AlternateRowColors,
                HeaderBackgroundColor = HeaderBackgroundColor,
                HeaderTextColor = HeaderTextColor
            };
        }
        public override void RenderElement(Graphics graphics, ReportConfiguration config)
        {
            // For now, render as a placeholder
            RenderPlaceholder(graphics, "Transaction Table");
        }
        public override void DrawDesignerElement(Graphics graphics)
        {
            using SolidBrush brush = new(Color.LightPink);
            using Pen pen = new(Color.Gray, 1);
            graphics.FillRectangle(brush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            using Font font = new("Segoe UI", 9);
            using SolidBrush textBrush = new(Color.Black);
            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(DisplayName ?? "Table", font, textBrush, Bounds, format);
        }
        public override int CreatePropertyControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            // Max rows
            AddPropertyLabel(container, "Max Rows:", yPosition);
            Guna2NumericUpDown numericUpDown = AddPropertyNumericUpDown(container, MaxRows, yPosition, value =>
            {
                MaxRows = (int)value;
                onPropertyChanged();
            }, 1, 100);
            numericUpDown.Left = 110;  // Shift it over to make space for the label
            yPosition += RowHeight;

            // Show headers checkbox
            AddPropertyCheckBoxWithLabel(container, "Headers", ShowHeaders, yPosition,
                value =>
                {
                    ShowHeaders = value;
                    onPropertyChanged();
                });
            yPosition += CheckBoxRowHeight;

            // Alternate row colors checkbox
            AddPropertyCheckBoxWithLabel(container, "Alt Colors", AlternateRowColors, yPosition,
                value =>
                {
                    AlternateRowColors = value;
                    onPropertyChanged();
                });
            yPosition += CheckBoxRowHeight;

            return yPosition;
        }
        private void RenderPlaceholder(Graphics graphics, string text)
        {
            using SolidBrush brush = new(Color.LightGray);
            using Pen pen = new(Color.Gray, 1);
            using Font font = new("Segoe UI", 10);
            using SolidBrush textBrush = new(Color.Black);

            graphics.FillRectangle(brush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(text, font, textBrush, Bounds, format);
        }
        protected override Color GetDesignerColor() => Color.LightPink;
    }
}