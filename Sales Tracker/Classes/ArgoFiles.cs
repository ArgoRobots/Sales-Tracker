using Microsoft.Win32;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Sales_Tracker.Classes
{
    internal partial class ArgoFiles
    {
        public static readonly string ArgoCompanyFileExtension = ".ArgoSales",
                                      TxtFileExtension = ".txt",
                                      JsonFileExtension = ".json",
                                      XlsxFileExtension = ".xlsx",
                                      PngFileExtension = ".png";

        // Refresh Explorer to show changes in the file icons
        [LibraryImport("shell32.dll", SetLastError = true)]
        private static partial void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        const uint SHCNE_ASSOCCHANGED = 0x8000000;
        const uint SHCNF_IDLIST = 0x0;

        public static void RegisterFileIcon(string extension, Icon icon, int iconIndex)
        {
            // Dynamically get the path of the currently running executable
            string applicationPath = Assembly.GetExecutingAssembly().Location;

            if (icon != null)
            {
                // Save the icon to a temporary file
                string tempIconPath = Path.Combine(Path.GetTempPath(), extension.Replace(".", "") + ".ico");
                using (FileStream fs = new(tempIconPath, FileMode.Create))
                {
                    icon.Save(fs);
                }

                string className = $"ArgoSalesTracker{extension.Replace(".", "")}";

                // Create registry entries for file association
                using (RegistryKey extensionKey = Registry.ClassesRoot.CreateSubKey(extension))
                {
                    extensionKey.SetValue("", className);
                }

                using (RegistryKey classKey = Registry.ClassesRoot.CreateSubKey(className))
                {
                    classKey.SetValue("", $"Argo Sales Tracker File");
                }

                using (RegistryKey defaultIconKey = Registry.ClassesRoot.CreateSubKey($@"{className}\DefaultIcon"))
                {
                    defaultIconKey.SetValue("", $"{tempIconPath},{iconIndex}");
                }

                using (RegistryKey commandKey = Registry.ClassesRoot.CreateSubKey($@"{className}\shell\open\command"))
                {
                    commandKey.SetValue("", $"\"{applicationPath}\" \"%1\"");
                }

                // Notify the shell to refresh the icons
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}