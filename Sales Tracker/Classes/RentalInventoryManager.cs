using Sales_Tracker.DataClasses;
using Newtonsoft.Json;

namespace Sales_Tracker.Classes
{
    /// Manages rental inventory operations including tracking availability and quantities.
    public static class RentalInventoryManager
    {
        private static List<RentalItem> _rentalInventory = [];
        private static readonly string _inventoryFilePath = Path.Combine(Directories.AppData_dir, "rental_inventory.json");

        /// Gets the current rental inventory list.
        public static List<RentalItem> RentalInventory => _rentalInventory;

        /// Loads rental inventory from file.
        public static void LoadInventory()
        {
            try
            {
                if (File.Exists(_inventoryFilePath))
                {
                    string json = File.ReadAllText(_inventoryFilePath);
                    _rentalInventory = JsonConvert.DeserializeObject<List<RentalItem>>(json) ?? [];
                }
                else
                {
                    _rentalInventory = [];
                }
            }
            catch (Exception ex)
            {
                Log.Error_ReadFile($"Failed to load rental inventory: {ex.Message}");
                _rentalInventory = [];
            }
        }

        /// <summary>
        /// Saves rental inventory to file.
        /// </summary>
        public static void SaveInventory()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_rentalInventory, Formatting.Indented);
                File.WriteAllText(_inventoryFilePath, json);
            }
            catch (Exception ex)
            {
                Log.Error_WriteToFile($"Failed to save rental inventory: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new rental item to inventory.
        /// </summary>
        public static bool AddRentalItem(RentalItem item)
        {
            if (_rentalInventory.Any(r => r.RentalItemID == item.RentalItemID))
            {
                return false; 
            }

            _rentalInventory.Add(item);
            SaveInventory();
            return true;
        }

        /// Removes a rental item from inventory.
        public static bool RemoveRentalItem(string rentalItemID)
        {
            RentalItem? item = _rentalInventory.FirstOrDefault(r => r.RentalItemID == rentalItemID);
            if (item != null)
            {
                _rentalInventory.Remove(item);
                SaveInventory();
                return true;
            }
            return false;
        }

        /// Gets a rental item by ID.
        public static RentalItem? GetRentalItem(string rentalItemID)
        {
            return _rentalInventory.FirstOrDefault(r => r.RentalItemID == rentalItemID);
        }

        /// Gets all rental items for a specific product.
        public static List<RentalItem> GetRentalItemsByProduct(string productID)
        {
            return _rentalInventory.Where(r => r.ProductID == productID).ToList();
        }

        /// Gets all available rental items.
        public static List<RentalItem> GetAvailableItems()
        {
            return _rentalInventory.Where(r => r.QuantityAvailable > 0).ToList();
        }

        /// Gets all rented rental items.
        public static List<RentalItem> GetRentedItems()
        {
            return _rentalInventory.Where(r => r.QuantityRented > 0).ToList();
        }

        /// Gets all items in maintenance.
        public static List<RentalItem> GetMaintenanceItems()
        {
            return _rentalInventory.Where(r => r.QuantityInMaintenance > 0).ToList();
        }

        /// Checks if a product has available rental inventory.
        public static bool IsProductAvailableForRent(string productID)
        {
            return _rentalInventory.Any(r => r.ProductID == productID && r.QuantityAvailable > 0);
        }

        /// Gets total available quantity for a product.
        public static int GetAvailableQuantity(string productID)
        {
            return _rentalInventory
                .Where(r => r.ProductID == productID)
                .Sum(r => r.QuantityAvailable);
        }

        /// Updates rental item status.
        public static bool UpdateItemStatus(string rentalItemID, RentalItem.AvailabilityStatus newStatus)
        {
            RentalItem? item = GetRentalItem(rentalItemID);
            if (item != null)
            {
                item.UpdateStatus(newStatus);
                SaveInventory();
                return true;
            }
            return false;
        }

        /// Processes a rental checkout.
        public static bool ProcessRental(string rentalItemID, int quantity, string customerID)
        {
            RentalItem? item = GetRentalItem(rentalItemID);
            if (item != null && item.RentOut(quantity, customerID))
            {
                SaveInventory();
                return true;
            }
            return false;
        }

        /// Processes a rental return.
        public static bool ProcessReturn(string rentalItemID, int quantity)
        {
            RentalItem? item = GetRentalItem(rentalItemID);
            if (item != null && item.ReturnItem(quantity))
            {
                SaveInventory();
                return true;
            }
            return false;
        }

        /// Gets inventory summary statistics.
        public static (int TotalItems, int TotalQuantity, int Available, int Rented, int Maintenance) GetInventorySummary()
        {
            return (
                TotalItems: _rentalInventory.Count,
                TotalQuantity: _rentalInventory.Sum(r => r.TotalQuantity),
                Available: _rentalInventory.Sum(r => r.QuantityAvailable),
                Rented: _rentalInventory.Sum(r => r.QuantityRented),
                Maintenance: _rentalInventory.Sum(r => r.QuantityInMaintenance)
            );
        }
    }
}