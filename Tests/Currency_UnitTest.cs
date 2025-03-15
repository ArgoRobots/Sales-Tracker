using Sales_Tracker.Classes;

namespace Tests
{
    [TestClass]
    public class Currency_UnitTest
    {
        [TestMethod]
        public void TestGetCurrencyTypesList()
        {
            // Act
            List<string> currencyTypes = Currency.GetCurrencyTypesList();

            // Assert
            Assert.IsNotNull(currencyTypes);
            Assert.IsTrue(currencyTypes.Count > 0);

            // Verify that all enum values are present
            var enumValues = Enum.GetNames(typeof(Currency.CurrencyTypes));
            CollectionAssert.AreEquivalent(enumValues, currencyTypes.ToArray());

            // Verify specific currencies are included
            Assert.IsTrue(currencyTypes.Contains("USD"));
            Assert.IsTrue(currencyTypes.Contains("EUR"));
            Assert.IsTrue(currencyTypes.Contains("JPY"));
        }

        [TestMethod]
        public void TestSameCurrencyExchangeRate()
        {
            decimal rate = Currency.GetExchangeRate("USD", "USD", "2025-01-01", false);
            Assert.AreEqual(1m, rate, "Exchange rate for same currency should be 1");
        }

        [TestMethod]
        public void TestGetExchangeRateRealAPI()
        {
            decimal rate = Currency.GetExchangeRate("USD", "EUR", "2025-01-10", false);

            // Just check if the result is a valid positive number
            Assert.IsTrue(rate > 0, "Should return a positive exchange rate");
        }
    }
}