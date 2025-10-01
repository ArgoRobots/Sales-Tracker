namespace Sales_Tracker.ReportGenerator.Elements
{
    /// <summary>
    /// Text label element for displaying text content.
    /// </summary>
    public class TextLabelElement : BaseElement
    {
        public string Text { get; set; } = "Sample Text";
        public string FontFamily { get; set; } = "Segoe UI";
        public float FontSize { get; set; } = 12f;
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;
        public Color TextColor { get; set; } = Color.Black;
        public StringAlignment Alignment { get; set; } = StringAlignment.Near;
        public StringAlignment VerticalAlignment { get; set; } = StringAlignment.Center;

        public override ReportElementType GetElementType() => ReportElementType.TextLabel;
        public override BaseElement Clone()
        {
            return new TextLabelElement
            {
                Id = Guid.NewGuid().ToString(),
                Bounds = Bounds,
                DisplayName = DisplayName + " (Copy)",
                ZOrder = ZOrder,
                IsSelected = false,
                IsVisible = IsVisible,
                Text = Text,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontStyle = FontStyle,
                TextColor = TextColor,
                Alignment = Alignment,
                VerticalAlignment = VerticalAlignment
            };
        }
        public override void RenderElement(Graphics graphics, ReportConfiguration config)
        {
            try
            {
                using Font font = new(FontFamily, FontSize, FontStyle);
                using SolidBrush brush = new(TextColor);

                StringFormat format = new()
                {
                    Alignment = Alignment,
                    LineAlignment = VerticalAlignment,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                graphics.DrawString(Text, font, brush, Bounds, format);
            }
            catch (ArgumentException)
            {
                // Fallback if font family is not available
                using Font fallbackFont = new("Segoe UI", FontSize, FontStyle);
                using SolidBrush fallbackBrush = new(TextColor);

                StringFormat fallbackFormat = new()
                {
                    Alignment = Alignment,
                    LineAlignment = VerticalAlignment,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                graphics.DrawString(Text, fallbackFont, fallbackBrush, Bounds, fallbackFormat);
            }
        }
        public override void DrawDesignerElement(Graphics graphics)
        {
            // Draw background
            using SolidBrush bgBrush = new(Color.LightYellow);
            using Pen pen = new(Color.Gray, 1);
            graphics.FillRectangle(bgBrush, Bounds);
            graphics.DrawRectangle(pen, Bounds);

            // Draw text with actual settings
            RenderElement(graphics, null);
        }
        public override int CreatePropertyControls(Panel container, int yPosition, Action onPropertyChanged)
        {
            const int rowHeight = 35;

            // Text content
            AddPropertyLabel(container, "Text:", yPosition);
            AddPropertyTextBox(container, Text, yPosition, value =>
            {
                Text = value;
                onPropertyChanged();
            });
            yPosition += rowHeight;

            // Font family
            AddPropertyLabel(container, "Font:", yPosition);
            AddPropertyComboBox(container, FontFamily, yPosition,
                ["Segoe UI", "Arial", "Times New Roman", "Calibri", "Verdana"],
                value =>
                {
                    FontFamily = value;
                    onPropertyChanged();
                });
            yPosition += rowHeight;

            // Font size
            AddPropertyLabel(container, "Size:", yPosition);
            AddPropertyNumericUpDown(container, (decimal)FontSize, yPosition, value =>
            {
                FontSize = (float)value;
                onPropertyChanged();
            }, 6, 72);
            yPosition += rowHeight;

            // Font style checkboxes
            AddPropertyLabel(container, "Style:", yPosition);
            AddFontStyleCheckBoxes(container, yPosition, onPropertyChanged);
            yPosition += rowHeight;

            // Text alignment
            AddPropertyLabel(container, "Align:", yPosition);
            AddPropertyComboBox(container, Alignment.ToString(), yPosition,
                ["Near", "Center", "Far"],
                value =>
                {
                    Alignment = Enum.Parse<StringAlignment>(value);
                    onPropertyChanged();
                });
            yPosition += rowHeight;

            // Text color
            AddPropertyLabel(container, "Color:", yPosition);
            AddColorPicker(container, yPosition, onPropertyChanged);
            yPosition += rowHeight;

            return yPosition;
        }
        private void AddFontStyleCheckBoxes(Panel container, int yPosition, Action onPropertyChanged)
        {
            CheckBox boldCheck = new()
            {
                Text = "B",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(35, 20),
                Location = new Point(85, yPosition),
                Checked = FontStyle.HasFlag(FontStyle.Bold)
            };
            boldCheck.CheckedChanged += (s, e) =>
            {
                UpdateFontStyle(boldCheck, FontStyle.Bold);
                onPropertyChanged();
            };
            container.Controls.Add(boldCheck);

            CheckBox italicCheck = new()
            {
                Text = "I",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Size = new Size(35, 20),
                Location = new Point(125, yPosition),
                Checked = FontStyle.HasFlag(FontStyle.Italic)
            };
            italicCheck.CheckedChanged += (s, e) =>
            {
                UpdateFontStyle(italicCheck, FontStyle.Italic);
                onPropertyChanged();
            };
            container.Controls.Add(italicCheck);

            CheckBox underlineCheck = new()
            {
                Text = "U",
                Font = new Font("Segoe UI", 9, FontStyle.Underline),
                Size = new Size(35, 20),
                Location = new Point(165, yPosition),
                Checked = FontStyle.HasFlag(FontStyle.Underline)
            };
            underlineCheck.CheckedChanged += (s, e) =>
            {
                UpdateFontStyle(underlineCheck, FontStyle.Underline);
                onPropertyChanged();
            };
            container.Controls.Add(underlineCheck);
        }
        private void UpdateFontStyle(CheckBox checkBox, FontStyle style)
        {
            if (checkBox.Checked)
                FontStyle |= style;
            else
                FontStyle &= ~style;
        }
        private void AddColorPicker(Panel container, int yPosition, Action onPropertyChanged)
        {
            Panel colorPreview = new()
            {
                BackColor = TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(50, 22),
                Location = new Point(85, yPosition),
                Cursor = Cursors.Hand
            };

            colorPreview.Click += (s, e) =>
            {
                ColorDialog colorDialog = new()
                {
                    Color = TextColor,
                    FullOpen = true
                };

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    TextColor = colorDialog.Color;
                    colorPreview.BackColor = TextColor;
                    onPropertyChanged();
                }
            };

            container.Controls.Add(colorPreview);

            Label colorLabel = new()
            {
                Text = "Click to change",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(140, yPosition + 3),
                Size = new Size(100, 20)
            };
            container.Controls.Add(colorLabel);
        }
        protected override Color GetDesignerColor() => Color.LightYellow;
    }
}