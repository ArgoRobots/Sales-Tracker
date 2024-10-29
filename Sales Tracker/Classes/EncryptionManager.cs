using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// The EncryptionManager class provides functionality for encrypting and decrypting data, 
    /// including streams and strings, using AES encryption. This class also manages encryption keys 
    /// and initialization vectors (IVs), and includes methods for secure storage and retrieval.
    /// </summary>
    public static class EncryptionManager
    {
        // Properties
        private static byte[] _aesKey;
        private static byte[] _aesIV;
        public const string encryptedTag = "encrypted:", encryptedValue = "true", passwordTag = "key:";

        // Getters and setters
        public static byte[] AesKey => _aesKey;
        public static byte[] AesIV => _aesIV;

        /// <summary>
        /// Initializes the encryption manager by ensuring encryption key and IV configurations exist.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                (_aesKey, _aesIV) = EnsureConfigurationExists();
            }
            catch (Exception ex)
            {
                Log.Error_InitEncryptionHelper(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Encrypts the given input stream using AES encryption with the specified key and IV.
        /// </summary>
        /// <returns>An encrypted MemoryStream containing the encrypted data.</returns>
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
                Log.Error_Encryption(ex.Message);
            }

            outputStream.Position = 0;
            return outputStream;
        }

        /// <summary>
        /// Decrypts the specified encrypted file into a MemoryStream.
        /// </summary>
        /// <returns>A tuple containing the decrypted MemoryStream and any footer lines.</returns>
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
                    Log.Error_Decryption("Footer start index not found");
                }

                // Check if the length of the data is valid for decryption
                if (fileContentBytes.Length % 16 != 0)
                {
                    Log.Error_Decryption($"The input data length {fileContentBytes.Length} is not a multiple of the AES block size (16 bytes)");
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
                Log.Error_Decryption(ex.Message);
                return (null, []);
            }
        }

        /// <summary>
        /// Encrypts the specified plain text string using AES encryption with the provided key and IV.
        /// </summary>
        /// <returns>A Base64-encoded string containing the encrypted text.</returns>
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

        /// <summary>
        /// Decrypts the specified Base64-encoded cipher text string using AES decryption with the provided key and IV.
        /// </summary>
        /// <returns>The decrypted plain text, or null if decryption fails.</returns>
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
                Log.Error_Decryption(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Retrieves the password stored in the specified file, decrypting it if necessary.
        /// </summary>
        /// <returns>The decrypted password, or null if retrieval or decryption fails.</returns>
        public static string? GetPasswordFromFile(string inputFile, byte[] key, byte[] iv)
        {
            if (!File.Exists(inputFile))
            {
                return null;
            }

            // Read all lines from the file
            string[] lines = File.ReadAllLines(inputFile);
            if (lines.Length < 2)
            {
                return null;
            }

            string secondLastLine = lines[^2];
            string lastLine = lines[^1];
            string password;

            // Check if the file is encrypted
            if (secondLastLine.Split(':')[1] == encryptedValue)
            {
                // Decrypt footer
                (MemoryStream decryptedStream, string[] footerLines) = DecryptFileToMemoryStream(inputFile, key, iv);
                if (decryptedStream == null)
                {
                    return null;
                }
                password = footerLines[^1];
            }
            else
            {
                password = lastLine;
            }

            string decryptedPassword = DecryptString(password, key, iv).Split(':')[1];
            return string.IsNullOrEmpty(decryptedPassword) ? null : decryptedPassword;
        }

        /// <summary>
        /// Decrypts the specified encrypted input file and writes the decrypted content to the output file.
        /// </summary>
        public static void DecryptAndWriteToFile(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            (MemoryStream decryptedStream, string[] footerLines) = DecryptFileToMemoryStream(inputFile, key, iv);
            if (decryptedStream == null)
            {
                return;
            }

            using FileStream outputFileStream = new(outputFile, FileMode.Create, FileAccess.Write);
            decryptedStream.CopyTo(outputFileStream);

            // Write the footer lines to the output file
            using StreamWriter writer = new(outputFileStream, Encoding.UTF8, leaveOpen: true);
            foreach (string line in footerLines)
            {
                writer.WriteLine(line);
            }
        }

        /// <summary>
        /// Generates a random cryptographic key of the specified size.
        /// </summary>
        /// <returns>A byte array containing the generated key.</returns>
        public static byte[] GenerateRandomKey(int size)
        {
            byte[] key = new byte[size];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }

        /// <summary>
        /// Generates a random cryptographic initialization vector (IV) of the specified size.
        /// </summary>
        /// <returns>A byte array containing the generated IV.</returns>
        public static byte[] GenerateRandomIV(int size)
        {
            byte[] iv = new byte[size];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }

        /// <summary>
        /// Ensures that the cryptographic configuration exists. If it does not, generates a new key and IV, stores them, and returns them.
        /// </summary>
        /// <returns>A tuple containing the cryptographic key and IV.</returns>
        private static (byte[] Key, byte[] IV) EnsureConfigurationExists()
        {
            if (!File.Exists(Directories.Config_file))
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

        /// <summary>
        /// Stores the provided cryptographic key and IV in a configuration file after encrypting them.
        /// </summary>
        private static void StoreKeys(byte[] key, byte[] iv)
        {
            Config config = new()
            {
                EncryptedKey = ProtectData(key),
                EncryptedIV = ProtectData(iv)
            };

            string configText = JsonSerializer.Serialize(config);
            File.WriteAllText(Directories.Config_file, configText);
        }

        /// <summary>
        /// Retrieves the stored cryptographic key and IV from the configuration file, decrypting them in the process.
        /// </summary>
        /// <returns>A tuple containing the decrypted cryptographic key and IV.</returns>
        private static (byte[] Key, byte[] IV) RetrieveKeys()
        {
            string configText = File.ReadAllText(Directories.Config_file);
            Config config = JsonSerializer.Deserialize<Config>(configText);

            return (UnprotectData(config.EncryptedKey), UnprotectData(config.EncryptedIV));
        }

        /// <summary>
        /// Encrypts the provided data for storage.
        /// </summary>
        /// <returns>A byte array containing the encrypted data.</returns>
        private static byte[] ProtectData(byte[] data)
        {
            return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        }

        /// <summary>
        /// Decrypts the provided encrypted data.
        /// </summary>
        /// <returns>A byte array containing the decrypted data.</returns>
        private static byte[] UnprotectData(byte[] data)
        {
            return ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
        }

        private class Config
        {
            // Properties
            private byte[] _encryptedKey;
            private byte[] _encryptedIV;

            // Getters and setters
            public byte[] EncryptedKey
            {
                get => _encryptedKey;
                set => _encryptedKey = value;
            }
            public byte[] EncryptedIV
            {
                get => _encryptedIV;
                set => _encryptedIV = value;
            }
        }
    }
}