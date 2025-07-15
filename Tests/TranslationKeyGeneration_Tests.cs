using Newtonsoft.Json;
using Sales_Tracker.Classes;
using Sales_Tracker.UI;

namespace Tests
{
    [TestClass]
    public class TranslationKeyGeneration_Tests
    {
        private string? tempCacheDir;

        [TestInitialize]
        public void Setup()
        {
            // Create a temporary directory for cache files
            tempCacheDir = Path.Combine(Path.GetTempPath(), "TranslationTestCache_" + Guid.NewGuid());
            Directory.CreateDirectory(tempCacheDir);

            // Set up test directories
            Directories.Cache_dir = tempCacheDir;
            Directories.Translations_file = Path.Combine(tempCacheDir, "translations.json");
            Directories.English_file = Path.Combine(tempCacheDir, "english_texts.json");

            SetupTestTranslationCache();
            LanguageManager.InitLanguageManager();
            EncryptionManager.Initialize();
            Directories.SetUniversalDirectories();
            DotEnv.Load();
        }
        private static void SetupTestTranslationCache()
        {
            Dictionary<string, Dictionary<string, string>> translationCache = new()
            {
                {
                    "fr", new Dictionary<string, string>
                    {
                        { "single_string_ThePurchaseAlreadyExistsWouldYouLikeToAddThisPurchaseAnyways", "L'achat existe déjà. Voulez-vous ajouter cet achat quand même?" },
                        { "single_string_AmountChargedIsNotEqualToTheTotalPriceOfThePurchase", "Le montant facturé n'est pas égal au prix total de l'achat" },
                        { "single_string_TestMessageWithVariable", "Message de test avec variable" },
                        { "single_string_SimpleTestMessage", "Message de test simple" }
                    }
                },
                {
                    "de", new Dictionary<string, string>
                    {
                        { "single_string_ThePurchaseAlreadyExistsWouldYouLikeToAddThisPurchaseAnyways", "Der Kauf existiert bereits. Möchten Sie diesen Kauf trotzdem hinzufügen?" },
                        { "single_string_AmountChargedIsNotEqualToTheTotalPriceOfThePurchase", "Der berechnete Betrag entspricht nicht dem Gesamtpreis des Kaufs" },
                        { "single_string_TestMessageWithVariable", "Testnachricht mit Variable" },
                        { "single_string_SimpleTestMessage", "Einfache Testnachricht" }
                    }
                }
            };

            var combinedCache = new
            {
                TranslationCache = translationCache,
                StringControlCache = new Dictionary<string, string?>()
            };

            string jsonContent = JsonConvert.SerializeObject(combinedCache, Formatting.Indented);
            File.WriteAllText(Directories.Translations_file, jsonContent);
        }

        [TestMethod]
        public void TestKeyGeneration_ConsistentAcrossVariableFormats()
        {
            // Arrange - Different variable formats for the same message
            string[] messageVariants =
            [
                "The purchase #{purchaseNumber} already exists. Would you like to add this purchase anyways?",
                "The purchase {purchaseNumber} already exists. Would you like to add this purchase anyways?"
            ];

            string expectedKey = "single_string_ThePurchaseAlreadyExistsWouldYouLikeToAddThisPurchaseAnyways";

            // Act & Assert
            foreach (string? message in messageVariants)
            {
                string key = LanguageManager.GetStringKey(message);

                Assert.AreEqual(expectedKey, key,
                    $"Key generation failed for: '{message}'");
            }
        }

        [TestMethod]
        public void TestTranslateString_FindsTranslation()
        {
            // Arrange
            Sales_Tracker.Properties.Settings.Default.Language = "French";  // Set to French for testing

            // Test messages with variables that should find translations
            Dictionary<string, string> testCases = new()
            {
                { "The purchase #{purchaseNumber} already exists. Would you like to add this purchase anyways?",
                  "L'achat existe déjà. Voulez-vous ajouter cet achat quand même?" },
                { "Amount charged ${amount} is not equal to the total price of the purchase {total}",
                  "Le montant facturé n'est pas égal au prix total de l'achat" }
            };

            // Act & Assert
            foreach (KeyValuePair<string, string> testCase in testCases)
            {
                // Try to translate the normalized message
                string translation = LanguageManager.TranslateString(testCase.Key);

                Assert.AreEqual(testCase.Value, translation,
                    $"Translation failed for: '{testCase.Key}' (normalized: '{testCase.Key}')");
            }
        }

        [TestMethod]
        public void TestGetStringKey_GeneratesExpectedFormat()
        {
            // Arrange
            Dictionary<string, string?> testCases = new()
            {
                { "Simple message", "single_string_SimpleMessage" },
                { "Message with spaces", "single_string_MessageWithSpaces" },
                { "Message with punctuation!", "single_string_MessageWithPunctuation" },
                { "message with MIXED case", "single_string_MessageWithMixedCase" }
            };

            // Act & Assert
            foreach (KeyValuePair<string, string?> testCase in testCases)
            {
                string result = LanguageManager.GetStringKey(testCase.Key);
                Assert.AreEqual(testCase.Value, result,
                    $"Key generation failed for: '{testCase.Key}'");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Reset language to English
            Sales_Tracker.Properties.Settings.Default.Language = "English";

            // Clean up temporary cache directory
            if (Directory.Exists(tempCacheDir))
            {
                Directory.Delete(tempCacheDir, true);
            }
        }
    }
}