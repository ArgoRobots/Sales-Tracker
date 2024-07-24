using System.Security.Cryptography;
using System.Text.Json;

namespace Sales_Tracker.Classes
{
    public static class EncryptionHelper
    {
        private static byte[] aesKey;
        private static byte[] aesIV;
        public const string EncryptionHeader = "ENCRYPTED";
        private static readonly string ConfigPath = "config.json";

        public static void Initialize()
        {
            try
            {
                Log.Write(1, "Initializing EncryptionHelper...");
                (aesKey, aesIV) = EnsureConfigurationExists();
                Log.Write(1, "EncryptionHelper initialized successfully.");
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error initializing EncryptionHelper: {ex.Message}");
                throw;
            }
        }

        public static byte[] AesKey => aesKey;
        public static byte[] AesIV => aesIV;

        public static void EncryptFile(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            try
            {
                using FileStream inputFileStream = new(inputFile, FileMode.Open, FileAccess.Read);
                using FileStream outputFileStream = new(outputFile, FileMode.Create, FileAccess.Write);
                using Aes aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                // Write the header indicating encryption
                using (StreamWriter writer = new(outputFileStream, leaveOpen: true))
                {
                    writer.WriteLine(EncryptionHeader);
                }

                using CryptoStream cryptoStream = new(outputFileStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                inputFileStream.CopyTo(cryptoStream);

                Log.Write(2, $"Encryption successful");
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error during encryption: {ex.Message}");
            }
        }
        public static void DecryptFile(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            using (FileStream inputFileStream = new(inputFile, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new(inputFileStream))
            {
                // Read the header to check if the file is encrypted
                string header = reader.ReadLine();

                if (header != EncryptionHeader)
                {
                    // Reset the stream position and copy the file as is if it's not encrypted
                    inputFileStream.Position = 0;
                    inputFileStream.CopyTo(new FileStream(outputFile, FileMode.Create, FileAccess.Write));
                    return;
                }

                // Move the stream position back to start reading the encrypted content
                inputFileStream.Position = header.Length + Environment.NewLine.Length;

                using FileStream outputFileStream = new(outputFile, FileMode.Create, FileAccess.Write);
                using Aes aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using CryptoStream cryptoStream = new(outputFileStream, aes.CreateDecryptor(), CryptoStreamMode.Write);
                inputFileStream.CopyTo(cryptoStream);
            }

            // Force garbage collection to ensure all file handles are released
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        public static byte[] GenerateRandomKey(int size)
        {
            byte[] key = new byte[size];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }
        public static byte[] GenerateRandomIV(int size)
        {
            byte[] iv = new byte[size];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }
        private static (byte[] Key, byte[] IV) EnsureConfigurationExists()
        {
            if (!File.Exists(ConfigPath))
            {
                byte[] key = GenerateRandomKey(32);
                byte[] iv = GenerateRandomIV(16);
                StoreKeys(key, iv);
                return (key, iv);
            }
            else
            {
                return RetrieveKeys();
            }
        }
        private static void StoreKeys(byte[] key, byte[] iv)
        {
            Config config = new()
            {
                EncryptedKey = ProtectData(key),
                EncryptedIV = ProtectData(iv)
            };

            string configText = JsonSerializer.Serialize(config);
            File.WriteAllText(ConfigPath, configText);
        }
        private static (byte[] Key, byte[] IV) RetrieveKeys()
        {
            string configText = File.ReadAllText(ConfigPath);
            Config config = JsonSerializer.Deserialize<Config>(configText);

            return (UnprotectData(config.EncryptedKey), UnprotectData(config.EncryptedIV));
        }
        private static byte[] ProtectData(byte[] data)
        {
            return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        }
        private static byte[] UnprotectData(byte[] data)
        {
            return ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
        }

        private class Config
        {
            public byte[] EncryptedKey { get; set; }
            public byte[] EncryptedIV { get; set; }
        }
    }
}