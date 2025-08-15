using Guna.UI2.WinForms;

namespace Sales_Tracker.UI
{
    public static class DpiHelper
    {
        // I designed this app with my Windows display scale set to 150%
        private static readonly float DESIGN_DPI_SCALE = 1.5f;

        private static float? _cachedDpiScale = null;

        /// <summary>
        /// Gets the relative DPI scale factor compared to the design baseline DPI.
        /// </summary>
        public static float GetRelativeDpiScale()
        {
            // Return cached value if already calculated
            if (_cachedDpiScale.HasValue)
            {
                return _cachedDpiScale.Value;
            }

            // Calculate and cache the DPI scale
            _cachedDpiScale = CalculateDpiScale();
            return _cachedDpiScale.Value;
        }

        public static void ScaleComboBox(Guna2ComboBox comboBox)
        {
            float scale = GetRelativeDpiScale();

            comboBox.ItemHeight = (int)(comboBox.ItemHeight * scale);
        }

        /// <summary>
        /// Calculates the current DPI scale factor.
        /// </summary>
        private static float CalculateDpiScale()
        {
            try
            {
                using Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
                float currentDpiScale = graphics.DpiX / 96f;
                return currentDpiScale / DESIGN_DPI_SCALE;
            }
            catch
            {
                return 1.0f;
            }
        }
    }
}