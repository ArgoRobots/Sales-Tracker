using Guna.UI2.WinForms;
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
        public void TestOnlyAllowNumbersInTextBox()
        {
            Guna2TextBox textBox = new();
            KeyPressEventArgs keyPressEvent = new('5');

            Tools.OnlyAllowNumbersInTextBox(textBox, keyPressEvent);
            Assert.IsFalse(keyPressEvent.Handled);

            keyPressEvent = new KeyPressEventArgs('a');
            Tools.OnlyAllowNumbersInTextBox(textBox, keyPressEvent);
            Assert.IsTrue(keyPressEvent.Handled);
        }

        [TestMethod]
        public void TestOnlyAllowLettersInTextBox()
        {
            Guna2TextBox textBox = new();
            KeyPressEventArgs keyPressEvent = new('a');

            Tools.OnlyAllowLettersInTextBox(textBox, keyPressEvent);
            Assert.IsFalse(keyPressEvent.Handled);

            keyPressEvent = new KeyPressEventArgs('1');
            Tools.OnlyAllowLettersInTextBox(textBox, keyPressEvent);
            Assert.IsTrue(keyPressEvent.Handled);
        }

        [TestMethod]
        public void TestOnlyAllowNumbersAndOneDecimalInGunaTextBox()
        {
            Guna2TextBox textBox = new();
            KeyPressEventArgs keyPressEvent = new('5');

            Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox(textBox, keyPressEvent);
            Assert.IsFalse(keyPressEvent.Handled);

            keyPressEvent = new KeyPressEventArgs('.');
            Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox(textBox, keyPressEvent);
            Assert.IsFalse(keyPressEvent.Handled);

            textBox.Text = "3.14";
            keyPressEvent = new KeyPressEventArgs('.');
            Tools.OnlyAllowNumbersAndOneDecimalInGunaTextBox(textBox, keyPressEvent);
            Assert.IsTrue(keyPressEvent.Handled);
        }

        [TestMethod]
        public void TestOnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox()
        {
            Guna2TextBox textBox = new();
            KeyPressEventArgs keyPressEvent = new('5');

            Tools.OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox(textBox, keyPressEvent);
            Assert.IsFalse(keyPressEvent.Handled);

            keyPressEvent = new KeyPressEventArgs('-');
            Tools.OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox(textBox, keyPressEvent);
            Assert.IsFalse(keyPressEvent.Handled);

            textBox.Text = "-3.14";
            keyPressEvent = new KeyPressEventArgs('.');
            Tools.OnlyAllowNumbersAndOneDecimalAndOneMinusInGunaTextBox(textBox, keyPressEvent);
            Assert.IsTrue(keyPressEvent.Handled);
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

        [TestMethod]
        public void TestMakeSureTextIsNotSelectedAndCursorIsAtEnd()
        {
            Guna2TextBox textBox = new() { Text = "Hello World" };
            Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd(textBox, EventArgs.Empty);

            Assert.AreEqual(textBox.Text.Length, textBox.SelectionStart);
            Assert.AreEqual(0, textBox.SelectionLength);
        }
    }
}
