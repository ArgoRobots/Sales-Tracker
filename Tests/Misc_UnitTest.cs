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
            LoadingPanel.InitLoadingPanel();

            // Show the loading panel
            LoadingPanel.ShowLoadingPanel(testForm);
            Assert.IsTrue(testForm.Controls.Contains(LoadingPanel.LoadingPanelInstance));

            // Hide the loading panel
            LoadingPanel.HideLoadingPanel(testForm);
            Assert.IsFalse(testForm.Controls.Contains(LoadingPanel.LoadingPanelInstance));
        }
    }
}