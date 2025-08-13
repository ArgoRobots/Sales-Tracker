namespace Sales_Tracker.UI
{
    public static class DpiHelper
    {
        // I designed this app with my Windows display scale set to 150%
        private static readonly float DESIGN_DPI_SCALE = 1.5f;

        /// <summary>
        /// Gets the relative DPI scale factor compared to the design baseline DPI.
        /// Returns a scale factor where higher system DPI results in larger scaling values for UI elements.
        /// </summary>
        public static float GetRelativeDpiScale()
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