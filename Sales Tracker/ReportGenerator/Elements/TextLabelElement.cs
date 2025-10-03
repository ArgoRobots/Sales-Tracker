using Guna.UI2.WinForms;
using Guna.UI2.WinForms.Enums;
using Sales_Tracker.Theme;

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
        public StringAlignment Alignment { get; set; } = StringAlignment.Center;
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
            // Text content
            AddPropertyLabel(container, "Text:", yPosition);
            AddPropertyTextBox(container, Text, yPosition, value =>
            {
                Text = value;
                onPropertyChanged();
            });
            yPosition += RowHeight;

            // Font family
            AddPropertyLabel(container, "Font:", yPosition);
            AddPropertyComboBox(container, FontFamily, yPosition,
                ["Segoe UI", "Arial", "Times New Roman", "Calibri", "Verdana"],
                value =>
                {
                    FontFamily = value;
                    onPropertyChanged();
                });
            yPosition += RowHeight;

            // Font size
            AddPropertyLabel(container, "Size:", yPosition);
            AddPropertyNumericUpDown(container, (decimal)FontSize, yPosition, value =>
            {
                FontSize = (float)value;
                onPropertyChanged();
            }, 6, 72);
            yPosition += RowHeight;

            // Font style toggle buttons
            AddPropertyLabel(container, "Style:", yPosition);
            AddFontStyleToggleButtons(container, yPosition, onPropertyChanged);
            yPosition += RowHeight;

            // Text alignment
            AddPropertyLabel(container, "Align:", yPosition);
            AddPropertyComboBox(container, AlignmentToDisplayText(Alignment), yPosition,
                ["Left", "Center", "Right"],
                value =>
                {
                    Alignment = DisplayTextToAlignment(value);
                    onPropertyChanged();
                });
            yPosition += RowHeight;

            // Vertical alignment
            AddPropertyLabel(container, "V-Align:", yPosition);
            AddPropertyComboBox(container, VerticalAlignmentToDisplayText(VerticalAlignment), yPosition,
                ["Top", "Middle", "Bottom"],
                value =>
                {
                    VerticalAlignment = DisplayTextToVerticalAlignment(value);
                    onPropertyChanged();
                });
            yPosition += RowHeight;

            // Text color
            AddPropertyLabel(container, "Color:", yPosition);
            AddColorPicker(container, yPosition, onPropertyChanged);
            yPosition += RowHeight;

            return yPosition;
        }
        private static string AlignmentToDisplayText(StringAlignment alignment)
        {
            return alignment switch
            {
                StringAlignment.Near => "Left",
                StringAlignment.Center => "Center",
                StringAlignment.Far => "Right",
                _ => "Center"
            };
        }
        private static StringAlignment DisplayTextToAlignment(string displayText)
        {
            return displayText switch
            {
                "Left" => StringAlignment.Near,
                "Center" => StringAlignment.Center,
                "Right" => StringAlignment.Far,
                _ => StringAlignment.Center
            };
        }
        private static string VerticalAlignmentToDisplayText(StringAlignment alignment)
        {
            return alignment switch
            {
                StringAlignment.Near => "Top",
                StringAlignment.Center => "Middle",
                StringAlignment.Far => "Bottom",
                _ => "Middle"
            };
        }
        private static StringAlignment DisplayTextToVerticalAlignment(string displayText)
        {
            return displayText switch
            {
                "Top" => StringAlignment.Near,
                "Middle" => StringAlignment.Center,
                "Bottom" => StringAlignment.Far,
                _ => StringAlignment.Center
            };
        }
        private void AddFontStyleToggleButtons(Panel container, int yPosition, Action onPropertyChanged)
        {
            int xPosition = 85;
            const int buttonWidth = 35;
            const int buttonHeight = 30;
            const int spacing = 5;

            // Calculate vertical position to center with label
            int buttonY = yPosition + 2;

            // Bold button
            Guna2Button boldButton = new()
            {
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(xPosition, buttonY),
                Text = "B",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BorderRadius = 2,
                ButtonMode = ButtonMode.ToogleButton,
                Checked = FontStyle.HasFlag(FontStyle.Bold),
                FillColor = Color.White,
                ForeColor = Color.Black,
                CheckedState =
                {
                    FillColor = CustomColors.AccentBlue,
                    ForeColor = Color.White
                }
            };
            boldButton.CheckedChanged += (s, e) =>
            {
                if (boldButton.Checked)
                {
                    FontStyle |= FontStyle.Bold;
                }
                else
                {
                    FontStyle &= ~FontStyle.Bold;
                }

                onPropertyChanged();
            };
            container.Controls.Add(boldButton);
            xPosition += buttonWidth + spacing;

            // Italic button
            Guna2Button italicButton = new()
            {
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(xPosition, buttonY),
                Text = "I",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                BorderRadius = 2,
                ButtonMode = ButtonMode.ToogleButton,
                Checked = FontStyle.HasFlag(FontStyle.Italic),
                FillColor = Color.White,
                ForeColor = Color.Black,
                CheckedState =
                {
                    FillColor = CustomColors.AccentBlue,
                    ForeColor = Color.White
                }
            };
            italicButton.CheckedChanged += (s, e) =>
            {
                if (italicButton.Checked)
                {
                    FontStyle |= FontStyle.Italic;
                }
                else
                {
                    FontStyle &= ~FontStyle.Italic;
                }

                onPropertyChanged();
            };
            container.Controls.Add(italicButton);
            xPosition += buttonWidth + spacing;

            // Underline button
            Guna2Button underlineButton = new()
            {
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(xPosition, buttonY),
                Text = "U",
                Font = new Font("Segoe UI", 9, FontStyle.Underline),
                BorderRadius = 2,
                ButtonMode = ButtonMode.ToogleButton,
                Checked = FontStyle.HasFlag(FontStyle.Underline),
                FillColor = Color.White,
                ForeColor = Color.Black,
                CheckedState =
                {
                    FillColor = CustomColors.AccentBlue,
                    ForeColor = Color.White
                }
            };
            underlineButton.CheckedChanged += (s, e) =>
            {
                if (underlineButton.Checked)
                {
                    FontStyle |= FontStyle.Underline;
                }
                else
                {
                    FontStyle &= ~FontStyle.Underline;
                }

                onPropertyChanged();
            };
            container.Controls.Add(underlineButton);
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