using Sales_Tracker.Classes;
using System.Windows.Forms;

namespace Tests
{
    [TestClass]
    public class Misc_UnitTest
    {
        [TestMethod]
        public void TestLoadingPanel()
        {
            Form testForm = new();
            LoadingPanel.InitBlankLoadingPanel();

            // Show the loading panel
            LoadingPanel.ShowBlankLoadingPanel(testForm);
            Assert.IsTrue(testForm.Controls.Contains(LoadingPanel.BlankLoadingPanelInstance));

            // Hide the loading panel
            LoadingPanel.HideBlankLoadingPanel(testForm);
            Assert.IsFalse(testForm.Controls.Contains(LoadingPanel.BlankLoadingPanelInstance));
        }
    }
}