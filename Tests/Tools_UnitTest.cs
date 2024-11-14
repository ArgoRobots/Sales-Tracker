using Sales_Tracker.Classes;
using System.Windows.Forms;

namespace Tests
{
    [TestClass]
    public class Tools_UnitTest
    {
        [TestMethod]
        public void TestFormatDateAndTime()
        {
            DateTime testDate = new(2024, 8, 10, 14, 30, 45);

            string formattedDate = Tools.FormatDate(testDate);
            string formattedTime = Tools.FormatTime(testDate);

            Assert.AreEqual("2024-08-10", formattedDate);
            Assert.AreEqual("02:30:45.00", formattedTime);
        }

        [TestMethod]
        public void TestAddNumberForAStringThatAlreadyExists()
        {
            List<string> existingNames = ["Item", "Item (2)", "Item (3)"];
            string newName = Tools.AddNumberForAStringThatAlreadyExists("Item", existingNames);
            Assert.AreEqual("Item (4)", newName);
        }

        [TestMethod]
        public void TestRemoveNumAfterString()
        {
            string name = "Item (3)";
            string result = Tools.RemoveNumAfterString(name);
            Assert.AreEqual("Item", result);

            string nameWithoutNumber = "Item";
            string resultWithoutNumber = Tools.RemoveNumAfterString(nameWithoutNumber);
            Assert.AreEqual("Item", resultWithoutNumber);
        }

        [TestMethod]
        public void TestIsFormOpen()
        {
            Form testForm = new();
            testForm.Show();

            bool isOpen = Tools.IsFormOpen(typeof(Form));
            Assert.IsTrue(isOpen);

            testForm.Close();
            isOpen = Tools.IsFormOpen(typeof(Form));
            Assert.IsFalse(isOpen);
        }
    }
}
