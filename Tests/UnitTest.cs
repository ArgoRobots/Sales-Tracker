using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Tests
{
    [TestClass]
    public class EncryptionHelperTests
    {
        [TestMethod]
        public void TestEncryptionDecryption()
        {
            EncryptionManager.Initialize();
            string originalText = "MySecretPassword";
            byte[] key = EncryptionManager.AesKey;
            byte[] iv = EncryptionManager.AesIV;

            // Encrypt the string
            string encryptedText = EncryptionManager.EncryptString(originalText, key, iv);

            // Decrypt the string
            string? decryptedText = EncryptionManager.DecryptString(encryptedText, key, iv);

            // Validate the result
            Assert.AreEqual(originalText, decryptedText, "Encryption and decryption failed.");
        }

        [TestMethod]
        public void TestEncryptDecryptStream()
        {
            EncryptionManager.Initialize();
            string originalText = "This is a stream test.";
            byte[] key = EncryptionManager.AesKey;
            byte[] iv = EncryptionManager.AesIV;

            // Convert string to stream
            MemoryStream inputStream = new(Encoding.UTF8.GetBytes(originalText));

            // Encrypt the stream
            MemoryStream encryptedStream = EncryptionManager.EncryptStream(inputStream, key, iv);

            // Reset position for reading the encrypted stream
            encryptedStream.Position = 0;

            // Decrypt the stream
            MemoryStream decryptedStream = new();
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                using CryptoStream cryptoStream = new(encryptedStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                cryptoStream.CopyTo(decryptedStream);
            }

            // Reset position for reading the decrypted stream
            decryptedStream.Position = 0;

            // Convert decrypted stream back to string
            StreamReader reader = new(decryptedStream);
            string decryptedText = reader.ReadToEnd();

            // Validate the result
            Assert.AreEqual(originalText, decryptedText, "Stream encryption and decryption failed.");
        }

        [TestMethod]
        public void TestGenerateRandomKeyAndIV()
        {
            byte[] key = EncryptionManager.GenerateRandomKey(32);
            byte[] iv = EncryptionManager.GenerateRandomIV(16);

            // Validate key and IV sizes
            Assert.AreEqual(32, key.Length, "Key size is incorrect.");
            Assert.AreEqual(16, iv.Length, "IV size is incorrect.");
        }

        [TestMethod]
        public void TestEncryptDecryptWithGeneratedKeys()
        {
            string originalText = "RandomKeyAndIVTest";
            byte[] key = EncryptionManager.GenerateRandomKey(32);
            byte[] iv = EncryptionManager.GenerateRandomIV(16);

            // Encrypt the string
            string encryptedText = EncryptionManager.EncryptString(originalText, key, iv);

            // Decrypt the string
            string? decryptedText = EncryptionManager.DecryptString(encryptedText, key, iv);

            // Validate the result
            Assert.AreEqual(originalText, decryptedText, "Encryption and decryption with generated keys failed.");
        }

        [TestMethod]
        public void TestFileAndDirectoryOperations()
        {
            string testDir = Path.Combine(Path.GetTempPath(), "TestDirectory");
            string testFile = Path.Combine(testDir, "TestFile.txt");

            // Test Directory Creation
            bool created = Directories.CreateDirectory(testDir, false);
            Assert.IsTrue(created);

            // Test File Creation
            bool fileCreated = Directories.CreateFile(testFile);
            Assert.IsTrue(fileCreated);
            Assert.IsTrue(File.Exists(testFile));

            // Test File Copy
            string copyFile = Path.Combine(testDir, "CopyTestFile.txt");
            bool fileCopied = Directories.CopyFile(testFile, copyFile);
            Assert.IsTrue(fileCopied);
            Assert.IsTrue(File.Exists(copyFile));

            // Test File Deletion
            bool fileDeleted = Directories.DeleteFile(copyFile);
            Assert.IsTrue(fileDeleted);
            Assert.IsFalse(File.Exists(copyFile));

            // Test Directory Deletion
            bool dirDeleted = Directories.DeleteDirectory(testDir, true);
            Assert.IsTrue(dirDeleted);
            Assert.IsFalse(Directory.Exists(testDir));
        }

        [TestMethod]
        public void TestDataFileManagerValueOperations()
        {
            string testFile = Path.Combine(Path.GetTempPath(), "TestConfig.txt");

            // Set Value
            DataFileManager.SetValue(testFile, DataFileManager.GlobalAppDataSettings.RPTutorial, "true");
            string? value = DataFileManager.GetValue(testFile, DataFileManager.GlobalAppDataSettings.RPTutorial);
            Assert.AreEqual("true", value);

            // Append Value
            DataFileManager.AppendValue(testFile, DataFileManager.GlobalAppDataSettings.RecentProjects, "Project1");
            DataFileManager.AppendValue(testFile, DataFileManager.GlobalAppDataSettings.RecentProjects, "Project2");
            string? appendedValue = DataFileManager.GetValue(testFile, DataFileManager.GlobalAppDataSettings.RecentProjects);
            Assert.AreEqual("Project1,Project2", appendedValue);
        }

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
        public void TestOnlyAllowNumbersInTextBox()
        {
            // Setup
            Guna2TextBox textBox = new();
            KeyPressEventArgs keyPressEvent = new('5');

            // Call the method
            Tools.OnlyAllowNumbersInTextBox(textBox, keyPressEvent);

            // Verify the character is allowed
            Assert.IsFalse(keyPressEvent.Handled);

            // Test with a letter
            keyPressEvent = new KeyPressEventArgs('a');
            Tools.OnlyAllowNumbersInTextBox(textBox, keyPressEvent);

            // Verify the character is not allowed
            Assert.IsTrue(keyPressEvent.Handled);
        }

        [TestMethod]
        public void TestOnlyAllowLettersInTextBox()
        {
            // Setup
            Guna2TextBox textBox = new();
            KeyPressEventArgs keyPressEvent = new('a');

            // Call the method
            Tools.OnlyAllowLettersInTextBox(textBox, keyPressEvent);

            // Verify the character is allowed
            Assert.IsFalse(keyPressEvent.Handled);

            // Test with a number
            keyPressEvent = new KeyPressEventArgs('1');
            Tools.OnlyAllowLettersInTextBox(textBox, keyPressEvent);

            // Verify the character is not allowed
            Assert.IsTrue(keyPressEvent.Handled);
        }
    }
}