using Sales_Tracker.Classes;
using Sales_Tracker.ReportGenerator.Elements;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// Handles storage and retrieval of custom report templates.
    /// </summary>
    public static class CustomTemplateStorage
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(),
                new ColorConverter(),
                new MarginsConverter()
            }
        };

        static CustomTemplateStorage()
        {
            // Ensure templates directory exists
            if (!Directory.Exists(Directories.ReportTemplates_dir))
            {
                Directory.CreateDirectory(Directories.ReportTemplates_dir);
            }
        }

        /// <summary>
        /// Saves a custom template with the specified name.
        /// </summary>
        public static bool SaveTemplate(string templateName, ReportConfiguration config)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(templateName))
                {
                    return false;
                }

                // Create a copy of the configuration to save
                CustomTemplate template = new()
                {
                    Name = templateName,
                    Created = DateTime.Now,
                    LastModified = DateTime.Now,
                    PageSize = config.PageSize,
                    PageOrientation = config.PageOrientation,
                    ShowHeader = config.ShowHeader,
                    ShowFooter = config.ShowFooter,
                    ShowPageNumbers = config.ShowPageNumbers,
                    BackgroundColor = config.BackgroundColor,
                    PageMargins = new Margins(
                        config.PageMargins.Left,
                        config.PageMargins.Top,
                        config.PageMargins.Right,
                        config.PageMargins.Bottom
                    ),
                    Filters = new ReportFilters
                    {
                        TransactionType = config.Filters.TransactionType,
                        IncludeReturns = config.Filters.IncludeReturns,
                        IncludeLosses = config.Filters.IncludeLosses,
                        DatePresetName = config.Filters.DatePresetName,
                        SelectedChartTypes = [.. config.Filters.SelectedChartTypes]
                    },
                    Elements = config.Elements.Select(e => SerializeElement(e)).ToList()
                };

                string fileName = SanitizeFileName(templateName) + ".json";
                string filePath = Path.Combine(Directories.ReportTemplates_dir, fileName);

                string json = JsonSerializer.Serialize(template, _jsonOptions);
                File.WriteAllText(filePath, json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads a custom template by name.
        /// </summary>
        public static ReportConfiguration? LoadTemplate(string templateName)
        {
            try
            {
                string fileName = SanitizeFileName(templateName) + ".json";
                string filePath = Path.Combine(Directories.ReportTemplates_dir, fileName);

                if (!File.Exists(filePath))
                {
                    return null;
                }

                string json = File.ReadAllText(filePath);
                CustomTemplate template = JsonSerializer.Deserialize<CustomTemplate>(json, _jsonOptions);

                if (template == null)
                {
                    return null;
                }

                // Convert back to ReportConfiguration
                ReportConfiguration config = new()
                {
                    Title = template.Name,
                    PageSize = template.PageSize,
                    PageOrientation = template.PageOrientation,
                    ShowHeader = template.ShowHeader,
                    ShowFooter = template.ShowFooter,
                    ShowPageNumbers = template.ShowPageNumbers,
                    BackgroundColor = template.BackgroundColor,
                    PageMargins = new Margins(
                        template.PageMargins.Left,
                        template.PageMargins.Top,
                        template.PageMargins.Right,
                        template.PageMargins.Bottom
                    ),
                    Filters = new ReportFilters
                    {
                        TransactionType = template.Filters.TransactionType,
                        IncludeReturns = template.Filters.IncludeReturns,
                        IncludeLosses = template.Filters.IncludeLosses,
                        DatePresetName = template.Filters.DatePresetName,
                        SelectedChartTypes = [.. template.Filters.SelectedChartTypes]
                    }
                };

                // Deserialize elements
                foreach (SerializedElement serializedElement in template.Elements)
                {
                    BaseElement element = DeserializeElement(serializedElement);
                    if (element != null)
                    {
                        config.AddElement(element);
                    }
                }

                return config;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a list of all saved custom template names.
        /// </summary>
        public static List<string> GetCustomTemplateNames()
        {
            try
            {
                if (!Directory.Exists(Directories.ReportTemplates_dir))
                {
                    return [];
                }

                return Directory.GetFiles(Directories.ReportTemplates_dir, "*.json")
                   .Select(Path.GetFileNameWithoutExtension)
                   .Where(name => name != null)
                   .OrderBy(name => name)
                   .ToList()!;
            }
            catch
            {
                return [];
            }
        }

        /// <summary>
        /// Deletes a custom template by name.
        /// </summary>
        public static bool DeleteTemplate(string templateName)
        {
            try
            {
                string fileName = SanitizeFileName(templateName) + ".json";
                string filePath = Path.Combine(Directories.ReportTemplates_dir, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a template with the given name already exists.
        /// </summary>
        public static bool TemplateExists(string templateName)
        {
            string fileName = SanitizeFileName(templateName) + ".json";
            string filePath = Path.Combine(Directories.ReportTemplates_dir, fileName);
            return File.Exists(filePath);
        }
        private static string SanitizeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }
        private static SerializedElement SerializeElement(BaseElement element)
        {
            SerializedElement serialized = new()
            {
                ElementType = element.GetType().Name,
                Bounds = element.Bounds,
                ZOrder = element.ZOrder,
                IsVisible = element.IsVisible,

                // Serialize type-specific properties
                Properties = element switch
                {
                    ChartElement chart => new Dictionary<string, object>
                    {
                        ["ChartType"] = chart.ChartType.ToString()
                    },
                    LabelElement label => new Dictionary<string, object>
                    {
                        ["Text"] = label.Text ?? "",
                        ["FontFamily"] = label.FontFamily ?? "Segoe UI",
                        ["FontSize"] = label.FontSize,
                        ["FontStyle"] = label.FontStyle.ToString(),
                        ["TextColor"] = ColorToHex(label.TextColor),
                        ["HAlignment"] = label.HAlignment.ToString(),
                        ["VAlignment"] = label.VAlignment.ToString()
                    },
                    DateRangeElement => [],
                    SummaryElement => [],
                    TableElement table => new Dictionary<string, object>
                    {
                        ["TransactionType"] = table.TransactionType.ToString(),
                        ["IncludeReturns"] = table.IncludeReturns,
                        ["IncludeLosses"] = table.IncludeLosses
                    },
                    ImageElement image => new Dictionary<string, object>
                    {
                        ["ImagePath"] = image.ImagePath ?? ""
                    },
                    _ => []
                }
            };

            return serialized;
        }
        private static BaseElement? DeserializeElement(SerializedElement serialized)
        {
            BaseElement element = serialized.ElementType switch
            {
                nameof(ChartElement) => new ChartElement
                {
                    ChartType = Enum.Parse<MainMenu_Form.ChartDataType>(
                        serialized.Properties.GetValueOrDefault("ChartType", "TotalRevenue").ToString())
                },
                nameof(LabelElement) => new LabelElement
                {
                    Text = serialized.Properties.GetValueOrDefault("Text", "").ToString(),
                    FontFamily = serialized.Properties.GetValueOrDefault("FontFamily",
                        serialized.Properties.GetValueOrDefault("FontName", "Segoe UI")).ToString(),
                    FontSize = Convert.ToSingle(serialized.Properties.GetValueOrDefault("FontSize", 12f)),
                    FontStyle = Enum.Parse<FontStyle>(
                        serialized.Properties.GetValueOrDefault("FontStyle", "Regular").ToString()),
                    TextColor = HexToColor(serialized.Properties.GetValueOrDefault("TextColor", "#000000").ToString()),
                    HAlignment = Enum.Parse<StringAlignment>(
                        serialized.Properties.GetValueOrDefault("HAlignment",
                        serialized.Properties.GetValueOrDefault("Alignment", "Center")).ToString()),
                    VAlignment = Enum.Parse<StringAlignment>(
                        serialized.Properties.GetValueOrDefault("VAlignment", "Center").ToString())
                },
                nameof(DateRangeElement) => new DateRangeElement(),
                nameof(SummaryElement) => new SummaryElement(),
                nameof(TableElement) => new TableElement
                {
                    TransactionType = Enum.Parse<TransactionType>(
                        serialized.Properties.GetValueOrDefault("TransactionType", "Both").ToString()),
                    IncludeReturns = Convert.ToBoolean(serialized.Properties.GetValueOrDefault("IncludeReturns", true)),
                    IncludeLosses = Convert.ToBoolean(serialized.Properties.GetValueOrDefault("IncludeLosses", true))
                },
                nameof(ImageElement) => new ImageElement
                {
                    ImagePath = serialized.Properties.GetValueOrDefault("ImagePath", "").ToString()
                },
                _ => null
            };

            if (element != null)
            {
                element.Bounds = serialized.Bounds;
                element.ZOrder = serialized.ZOrder;
                element.IsVisible = serialized.IsVisible;
            }

            return element;
        }
        private static string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        private static Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");
            return Color.FromArgb(
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16)
            );
        }

        // Custom JSON converter for Color
        private class ColorConverter : JsonConverter<Color>
        {
            public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string hex = reader.GetString();
                return HexToColor(hex);
            }
            public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(ColorToHex(value));
            }
        }

        // Custom JSON converter for Margins
        private class MarginsConverter : JsonConverter<Margins>
        {
            public override Margins Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                int left = 0, top = 0, right = 0, bottom = 0;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return new Margins(left, top, right, bottom);
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read();

                        switch (propertyName)
                        {
                            case "Left":
                                left = reader.GetInt32();
                                break;
                            case "Top":
                                top = reader.GetInt32();
                                break;
                            case "Right":
                                right = reader.GetInt32();
                                break;
                            case "Bottom":
                                bottom = reader.GetInt32();
                                break;
                        }
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, Margins value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteNumber("Left", value.Left);
                writer.WriteNumber("Top", value.Top);
                writer.WriteNumber("Right", value.Right);
                writer.WriteNumber("Bottom", value.Bottom);
                writer.WriteEndObject();
            }
        }

        // Internal classes for serialization
        private class CustomTemplate
        {
            public string Name { get; set; }
            public DateTime Created { get; set; }
            public DateTime LastModified { get; set; }
            public PageSize PageSize { get; set; }
            public PageOrientation PageOrientation { get; set; }
            public bool ShowHeader { get; set; }
            public bool ShowFooter { get; set; }
            public bool ShowPageNumbers { get; set; }
            public Color BackgroundColor { get; set; }
            public Margins PageMargins { get; set; }
            public ReportFilters Filters { get; set; }
            public List<SerializedElement> Elements { get; set; }
        }

        private class SerializedElement
        {
            public string ElementType { get; set; }
            public Rectangle Bounds { get; set; }
            public int ZOrder { get; set; }
            public bool IsVisible { get; set; }
            public Dictionary<string, object> Properties { get; set; }
        }
    }
}
