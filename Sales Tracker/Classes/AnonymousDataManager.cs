using Newtonsoft.Json;

namespace Sales_Tracker.Classes
{
    public enum ExportDataField
    {
        ExportType,
        DurationMS,
        FileSize,
    }

    public enum ExportType
    {
        ExcelSheetsChart,
        GoogleSheetsChart,
        Backup,
        XLSX,
        Receipts
    }

    /// <summary>
    /// Manages collection and storage of anonymous data points for analytics purposes.
    /// </summary>
    public static class AnonymousDataManager
    {
        /// <summary>
        /// Adds export-related data points by appending directly to the file
        /// </summary>
        public static void AddExportData(Dictionary<ExportDataField, object> data)
        {
            ExportType exportType = (ExportType)data[ExportDataField.ExportType];
            long durationMs = (long)data[ExportDataField.DurationMS];

            Dictionary<string, object> dataPoint = new()
            {
                ["timestamp"] = Tools.FormatDateTime(DateTime.Now),
                ["ExportType"] = exportType.ToString(),
                ["DurationMS"] = Tools.FormatDuration(durationMs),
                ["FileSize"] = data[ExportDataField.FileSize]
            };

            string jsonLine = JsonConvert.SerializeObject(dataPoint, Formatting.Indented) + Environment.NewLine;
            File.AppendAllText(Directories.AnonymousUserDataCache_file, jsonLine);
        }
    }
}