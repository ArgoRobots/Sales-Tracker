using Microsoft.Win32;
using System.Reflection;
using System.Resources;

namespace Sales_Tracker.Classes
{
    internal class ArgoFiles
    {
        public static readonly string ArgoCompanyFileExtension = ".ArgoCompany",
                                      TxtFileExtension = ".txt";

        public static void RegisterFileIcon(string extension, string resourceName, int iconIndex)
        {
            // Dynamically get the path of the currently running executable
            string applicationPath = Assembly.GetExecutingAssembly().Location;

            // Assuming you have a ResourceManager instance ready, 
            // or you can create/load it dynamically here
            ResourceManager rm = Properties.Resources.ResourceManager;

            // Extract icon from resources
            Icon icon = (Icon)rm.GetObject(resourceName);

            if (icon != null)
            {
                // Save the icon to a temporary file
                string tempIconPath = Path.Combine(Path.GetTempPath(), resourceName + ".ico");
                using (FileStream fs = new(tempIconPath, FileMode.Create))
                {
                    icon.Save(fs);
                }

                var className = $"ArgoSalesTracker{extension.Replace(".", "")}";
                using (var key = Registry.ClassesRoot.CreateSubKey(extension))
                {
                    key.SetValue("", className);
                }
                using (var key = Registry.ClassesRoot.CreateSubKey(className))
                {
                    key.SetValue("", $"Argo Sales Tracker {extension} File");
                }
                using (var key = Registry.ClassesRoot.CreateSubKey($@"{className}\DefaultIcon"))
                {
                    key.SetValue("", $"{tempIconPath},{iconIndex}");
                }
                using (var key = Registry.ClassesRoot.CreateSubKey($@"{className}\shell\open\command"))
                {
                    key.SetValue("", $"\"{applicationPath}\" \"%1\"");
                }
            }
        }
    }
}