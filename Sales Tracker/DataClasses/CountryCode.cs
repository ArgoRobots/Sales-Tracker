using Sales_Tracker.UI;
using System.Text;

namespace Sales_Tracker.DataClasses
{
    /// <summary>
    /// Represents a country with its dialing code and phone number formatting rules.
    /// </summary>
    public class CountryCode
    {
        public string CountryName { get; set; }
        public string Code { get; set; }
        public string Format { get; set; } // Format pattern, e.g., "(XXX) XXX-XXXX"

        public CountryCode(string countryName, string code, string format = "")
        {
            CountryName = countryName;
            Code = code;
            Format = format;
        }

        public override string ToString()
        {
            return $"{CountryName} ({Code})";
        }

        /// <summary>
        /// Gets the search result display text with country code.
        /// </summary>
        public string ToSearchResultText()
        {
            return $"{Code} - {CountryName}";
        }

        /// <summary>
        /// Gets a list of common country codes with their formatting patterns.
        /// </summary>
        public static List<CountryCode> GetCountryCodes()
        {
            return new List<CountryCode>
            {
                new CountryCode("United States", "+1", "(XXX) XXX-XXXX"),
                new CountryCode("Canada", "+1", "(XXX) XXX-XXXX"),
                new CountryCode("United Kingdom", "+44", "XXXX XXX XXXX"),
                new CountryCode("Australia", "+61", "XXXX XXX XXX"),
                new CountryCode("Germany", "+49", "XXX XXXXXXXX"),
                new CountryCode("France", "+33", "X XX XX XX XX"),
                new CountryCode("Italy", "+39", "XXX XXX XXXX"),
                new CountryCode("Spain", "+34", "XXX XXX XXX"),
                new CountryCode("Netherlands", "+31", "XX XXXXXXXX"),
                new CountryCode("Belgium", "+32", "XXX XX XX XX"),
                new CountryCode("Switzerland", "+41", "XX XXX XX XX"),
                new CountryCode("Austria", "+43", "XXX XXXXXXX"),
                new CountryCode("Sweden", "+46", "XX XXX XX XX"),
                new CountryCode("Norway", "+47", "XXX XX XXX"),
                new CountryCode("Denmark", "+45", "XX XX XX XX"),
                new CountryCode("Finland", "+358", "XX XXX XXXX"),
                new CountryCode("Poland", "+48", "XXX XXX XXX"),
                new CountryCode("Czech Republic", "+420", "XXX XXX XXX"),
                new CountryCode("Greece", "+30", "XXX XXX XXXX"),
                new CountryCode("Portugal", "+351", "XXX XXX XXX"),
                new CountryCode("Ireland", "+353", "XX XXX XXXX"),
                new CountryCode("New Zealand", "+64", "XX XXX XXXX"),
                new CountryCode("Japan", "+81", "XX XXXX XXXX"),
                new CountryCode("South Korea", "+82", "XX XXXX XXXX"),
                new CountryCode("China", "+86", "XXX XXXX XXXX"),
                new CountryCode("India", "+91", "XXXXX XXXXX"),
                new CountryCode("Brazil", "+55", "(XX) XXXXX-XXXX"),
                new CountryCode("Mexico", "+52", "XXX XXX XXXX"),
                new CountryCode("Argentina", "+54", "XX XXXX-XXXX"),
                new CountryCode("South Africa", "+27", "XX XXX XXXX"),
                new CountryCode("Russia", "+7", "XXX XXX-XX-XX"),
                new CountryCode("Turkey", "+90", "XXX XXX XX XX"),
                new CountryCode("Saudi Arabia", "+966", "XX XXX XXXX"),
                new CountryCode("UAE", "+971", "XX XXX XXXX"),
                new CountryCode("Singapore", "+65", "XXXX XXXX"),
                new CountryCode("Malaysia", "+60", "XX XXXX XXXX"),
                new CountryCode("Thailand", "+66", "XX XXX XXXX"),
                new CountryCode("Indonesia", "+62", "XXX XXXX XXXX"),
                new CountryCode("Philippines", "+63", "XXX XXX XXXX"),
                new CountryCode("Vietnam", "+84", "XX XXXX XXXX"),
                new CountryCode("Israel", "+972", "XX XXX XXXX"),
                new CountryCode("Egypt", "+20", "XXX XXX XXXX")
            };
        }

        /// <summary>
        /// Formats a phone number according to the country's format pattern.
        /// </summary>
        public string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(Format) || string.IsNullOrEmpty(phoneNumber))
                return phoneNumber;

            // Remove all non-digit characters
            string digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(digitsOnly))
                return phoneNumber;

            StringBuilder formatted = new StringBuilder();
            int digitIndex = 0;

            foreach (char c in Format)
            {
                if (c == 'X')
                {
                    if (digitIndex < digitsOnly.Length)
                    {
                        formatted.Append(digitsOnly[digitIndex]);
                        digitIndex++;
                    }
                    else
                    {
                        break; // No more digits to format
                    }
                }
                else
                {
                    // Add formatting character if there are still digits to be formatted
                    if (digitIndex < digitsOnly.Length)
                    {
                        formatted.Append(c);
                    }
                }
            }

            // If there are remaining digits, append them
            if (digitIndex < digitsOnly.Length)
            {
                formatted.Append(digitsOnly.Substring(digitIndex));
            }

            return formatted.ToString();
        }

        /// <summary>
        /// Gets SearchResults for country codes with flags for use in SearchBox.
        /// </summary>
        public static List<SearchResult> GetCountryCodeSearchResults()
        {
            List<SearchResult> results = new();
            List<CountryCode> countryCodes = GetCountryCodes();

            // Create a dictionary to map country names to their flags from the Country class
            Dictionary<string, Image?> countryFlags = Country.CountrySearchResults
                .Where(sr => sr.Name != SearchBox.AddLine && !string.IsNullOrEmpty(sr.Name))
                .ToDictionary(sr => sr.Name, sr => sr.Flag, StringComparer.OrdinalIgnoreCase);

            // Add popular countries first
            string[] popularCountries = { "United States", "Canada", "United Kingdom" };
            foreach (string popularCountry in popularCountries)
            {
                CountryCode? country = countryCodes.FirstOrDefault(c => c.CountryName == popularCountry);
                if (country != null)
                {
                    countryFlags.TryGetValue(country.CountryName, out Image? flag);
                    results.Add(new SearchResult(country.ToSearchResultText(), flag, 0));
                }
            }

            // Add separator line
            results.Add(new SearchResult(SearchBox.AddLine, null, 0));

            // Add all other countries
            foreach (CountryCode country in countryCodes)
            {
                if (!popularCountries.Contains(country.CountryName))
                {
                    countryFlags.TryGetValue(country.CountryName, out Image? flag);
                    results.Add(new SearchResult(country.ToSearchResultText(), flag, 0));
                }
            }

            return results;
        }

        /// <summary>
        /// Parses a search result text to extract the country code.
        /// </summary>
        public static string ParseCountryCodeFromSearchResult(string searchResultText)
        {
            if (string.IsNullOrWhiteSpace(searchResultText))
                return "+1";

            // Format is "+X - CountryName" or "+XX - CountryName"
            int dashIndex = searchResultText.IndexOf(" - ");
            if (dashIndex > 0)
            {
                return searchResultText.Substring(0, dashIndex).Trim();
            }

            return searchResultText;
        }

        /// <summary>
        /// Gets a CountryCode object from a search result text or country code string.
        /// </summary>
        public static CountryCode? GetCountryCodeFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return GetCountryCodes().FirstOrDefault(c => c.Code == "+1");

            List<CountryCode> codes = GetCountryCodes();

            // Try to parse as search result text ("+X - CountryName")
            string code = ParseCountryCodeFromSearchResult(text);

            // Find by code
            return codes.FirstOrDefault(c => c.Code == code) ?? codes.FirstOrDefault(c => c.Code == "+1");
        }
    }
}
