using Guna.UI2.WinForms;
using Sales_Tracker.UI;
using System.Reflection;
using System.Windows.Forms;

namespace Tests
{
    [TestClass]
    public class CustomTooltip_Tests
    {
        private Button testButton;

        [TestInitialize]
        public void Setup()
        {
            testButton = new Button
            {
                Name = "TestButton",
                Text = "Test Button"
            };

            CustomColors.SetColors();
        }

        [TestMethod]
        public void TestSetToolTip_CreatesNewTooltipWithCorrectProperties()
        {
            // Arrange
            string title = "Test Title";
            string message = "Test Message";

            // Act
            CustomTooltip.SetToolTip(testButton, title, message);

            // Get the tooltip through reflection since the dictionary is private
            FieldInfo? tooltipsField = typeof(CustomTooltip).GetField("tooltips",
                BindingFlags.NonPublic | BindingFlags.Static);
            Dictionary<Control, Guna2HtmlToolTip>? tooltips = (Dictionary<Control, Guna2HtmlToolTip>)tooltipsField.GetValue(null);
            Guna2HtmlToolTip tooltip = tooltips[testButton];

            // Assert
            Assert.IsNotNull(tooltip);
            Assert.AreEqual(title, tooltip.ToolTipTitle);
            Assert.AreEqual(message, tooltip.GetToolTip(testButton));
        }

        [TestCleanup]
        public void Cleanup()
        {
            testButton.Parent?.Dispose();
            testButton.Dispose();
        }
    }

    /// <summary>
    /// Extension method to help raise events on controls
    /// </summary>
    public static class ControlExtensions
    {
        public static void RaiseEvent(this Control control, string eventName, EventArgs e)
        {
            FieldInfo? eventField = control.GetType().GetField(eventName, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            MulticastDelegate? eventDelegate = eventField?.GetValue(control) as MulticastDelegate;

            eventDelegate?.DynamicInvoke(control, e);
        }
    }
}