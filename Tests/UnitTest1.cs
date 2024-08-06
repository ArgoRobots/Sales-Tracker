using Sales_Tracker.Classes;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestEncryptionDecryption()
        {
            EncryptionHelper.Initialize();
            string originalText = "MySecretPassword";
            byte[] key = EncryptionHelper.AesKey;
            byte[] iv = EncryptionHelper.AesIV;

            // Encrypt the string
            string encryptedText = EncryptionHelper.EncryptString(originalText, key, iv);

            // Decrypt the string
            string? decryptedText = EncryptionHelper.DecryptString(encryptedText, key, iv);

            // Validate the result
            Assert.AreEqual(originalText, decryptedText, "Encryption and decryption failed.");
        }
    }
}