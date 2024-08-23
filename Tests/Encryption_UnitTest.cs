using Sales_Tracker.Classes;
using System.Security.Cryptography;
using System.Text;

namespace Tests
{
    [TestClass]
    public class Encryption_Tests
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
    }
}