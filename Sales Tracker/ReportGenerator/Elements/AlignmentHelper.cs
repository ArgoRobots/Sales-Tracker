namespace Sales_Tracker.ReportGenerator.Elements
{
    public static class AlignmentHelper
    {
        public static string ToDisplayText(StringAlignment alignment, bool isVertical = false)
        {
            return alignment switch
            {
                StringAlignment.Near => isVertical ? "Top" : "Left",
                StringAlignment.Center => isVertical ? "Middle" : "Center",
                StringAlignment.Far => isVertical ? "Bottom" : "Right",
                _ => isVertical ? "Middle" : "Left"
            };
        }
        public static StringAlignment FromDisplayText(string text, bool isVertical = false)
        {
            return text switch
            {
                "Left" or "Top" => StringAlignment.Near,
                "Center" or "Middle" => StringAlignment.Center,
                "Right" or "Bottom" => StringAlignment.Far,
                _ => StringAlignment.Center
            };
        }
    }
}