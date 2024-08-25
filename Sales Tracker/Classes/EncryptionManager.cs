using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sales_Tracker.Classes
{
    public static class EncryptionManager
    {
        private static byte[] aesKey;
        private static byte[] aesIV;
        public const string encryptedTag = "encrypted:", encryptedValue = "true", passwordTag = "key:";
        private static readonly string ConfigPath = "config.json";

        public static void Initialize()
        {
            try
            {
                (aesKey, aesIV) = EnsureConfigurationExists();
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error initializing EncryptionHelper: {ex.Message}");
                throw;
            }
        }

        public static byte[] AesKey => aesKey;
        public static byte[] AesIV => aesIV;

        // Streams
        public static MemoryStream EncryptStream(Stream inputStream, byte[] key, byte[] iv)
        {
            MemoryStream outputStream = new();

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                // Create the CryptoStream for encryption
                CryptoStream cryptoStream = new(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);

                // Copy the inputStream to the CryptoStream
                inputStream.CopyTo(cryptoStream);
                cryptoStream.FlushFinalBlock();
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error during encryption: {ex.Message}");
            }

            outputStream.Position = 0;
            return outputStream;
        }
        public static (MemoryStream?, string[]) DecryptFileToMemoryStream(string inputFile, byte[] key, byte[] iv)
        {
            try
            {
                // Read the file content as bytes
                byte[] fileContentBytes = File.ReadAllBytes(inputFile);

                string[] footerLines = [];

                // Find the position of the last two newline characters
                int newlineCount = 0;
                int footerStartIndex = -1;

                for (int i = fileContentBytes.Length - 1; i >= 0; i--)
                {
                    if (fileContentBytes[i] == '\n')
                    {
                        newlineCount++;
                        if (newlineCount == 2)
                        {
                            footerStartIndex = i + 1;
                            break;
                        }
                    }
                }

                // Ensure footerStartIndex was found
                if (footerStartIndex != -1)
                {
                    // Extract the footer content from the identified position
                    string footerContent = Encoding.UTF8.GetString(fileContentBytes, footerStartIndex, fileContentBytes.Length - footerStartIndex);
                    footerLines = footerContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                    fileContentBytes = fileContentBytes[..(footerStartIndex - 2)];
                }
                else
                {
                    Log.Write(0, "Footer start index not found.");
                }

                // Check if the length of the data is valid for decryption
                if (fileContentBytes.Length % 16 != 0)
                {
                    Log.Write(0, $"The input data length {fileContentBytes.Length} is not a multiple of the AES block size (16 bytes).");
                    return (null, footerLines);
                }

                // Create a MemoryStream from the byte array
                using MemoryStream inputMemoryStream = new(fileContentBytes);

                // Decrypt the content
                MemoryStream decryptedStream = new();
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using CryptoStream cryptoStream = new(inputMemoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                    cryptoStream.CopyTo(decryptedStream);
                }

                decryptedStream.Position = 0;
                return (decryptedStream, footerLines);
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error during decryption: {ex.Message}");
                return (null, []);
            }
        }

        // Strings
        public static string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msEncrypt = new();
            using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
            using StreamWriter swEncrypt = new(csEncrypt);

            swEncrypt.Write(plainText);
            swEncrypt.Flush();
            csEncrypt.FlushFinalBlock();

            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        public static string? DecryptString(string cipherText, byte[] key, byte[] iv)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            using MemoryStream msDecrypt = new(cipherTextBytes);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);

            try
            {
                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                Log.Write(0, $"Error during string decryption: {ex.Message}");
                return null;
            }
        }

        // Methods
        public static string? GetPasswordFromFile(string inputFile, byte[] key, byte[] iv)
        {
            if (!File.Exists(inputFile))
            {
                return null;
            }

            // Check if the file is encrypted and decrypt if necessary
            using FileStream fs = new(inputFile, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new(fs);

            // Read all lines to determine the encryption status from the second-to-last line
            List<string> lines = [];
            while (!reader.EndOfStream)
            {
                lines.Add(reader.ReadLine());
            }

            if (lines.Count < 2)
            {
                return null;
            }

            string secondLastLine = lines[^2];

            // Check if the file is encrypted by examining the second-to-last line
            if (secondLastLine.Split(':')[1] == encryptedValue)
            {
                (MemoryStream decryptedStream, string[] footerLines) = DecryptFileToMemoryStream(inputFile, key, iv);
                if (decryptedStream == null)
                {
                    return null;
                }

                // Get the second last line from the footer
                string password = DecryptString(footerLines[^1], key, iv);
                password = password.Split(':')[1];
                return string.IsNullOrEmpty(password) ? null : password;
            }
            else
            {
                string password = DecryptString(lines[^1], key, iv);
                return password.Split(":")[1];
            }
        }
        public static void DecryptAndWriteToFile(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            (MemoryStream decryptedStream, string[] footerLines) = DecryptFileToMemoryStream(inputFile, key, iv);
            if (decryptedStream == null)
            {
                return;
            }

            using FileStream outputFileStream = new(outputFile, FileMode.Create, FileAccess.Write);
            decryptedStream.CopyTo(outputFileStream);

            // Optionally, write the footer lines to the output file if needed
            using StreamWriter writer = new(outputFileStream, Encoding.UTF8, leaveOpen: true);
            foreach (string line in footerLines)
            {
                writer.WriteLine(line);
            }
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