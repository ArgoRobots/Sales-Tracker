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
        /// Gets a list of all country codes with their formatting patterns.
        /// Matches all countries from Country.cs
        /// </summary>
        public static List<CountryCode> GetCountryCodes()
        {
            return new List<CountryCode>
            {
                // Popular countries (will be shown at top)
                new CountryCode("United States", "+1", "(XXX) XXX-XXXX"),
                new CountryCode("China", "+86", "XXX XXXX XXXX"),
                new CountryCode("Germany", "+49", "XXX XXXXXXXX"),
                new CountryCode("Japan", "+81", "XX XXXX XXXX"),
                new CountryCode("United Kingdom", "+44", "XXXX XXX XXXX"),
                new CountryCode("France", "+33", "X XX XX XX XX"),
                new CountryCode("Italy", "+39", "XXX XXX XXXX"),
                new CountryCode("Canada", "+1", "(XXX) XXX-XXXX"),

                // All other countries alphabetically
                new CountryCode("Afghanistan", "+93", "XX XXX XXXX"),
                new CountryCode("Albania", "+355", "XX XXX XXXX"),
                new CountryCode("Algeria", "+213", "XXX XX XX XX"),
                new CountryCode("Andorra", "+376", "XXX XXX"),
                new CountryCode("Angola", "+244", "XXX XXX XXX"),
                new CountryCode("Antigua and Barbuda", "+1-268", "XXX-XXXX"),
                new CountryCode("Argentina", "+54", "XX XXXX-XXXX"),
                new CountryCode("Armenia", "+374", "XX XXX XXX"),
                new CountryCode("Australia", "+61", "XXXX XXX XXX"),
                new CountryCode("Austria", "+43", "XXX XXXXXXX"),
                new CountryCode("Azerbaijan", "+994", "XX XXX XX XX"),
                new CountryCode("Bahamas", "+1-242", "XXX-XXXX"),
                new CountryCode("Bahrain", "+973", "XXXX XXXX"),
                new CountryCode("Bangladesh", "+880", "XXXX-XXXXXX"),
                new CountryCode("Barbados", "+1-246", "XXX-XXXX"),
                new CountryCode("Belarus", "+375", "XX XXX-XX-XX"),
                new CountryCode("Belgium", "+32", "XXX XX XX XX"),
                new CountryCode("Belize", "+501", "XXX-XXXX"),
                new CountryCode("Benin", "+229", "XX XX XX XX"),
                new CountryCode("Bhutan", "+975", "XX XXX XXX"),
                new CountryCode("Bolivia", "+591", "X XXX XXXX"),
                new CountryCode("Bosnia and Herzegovina", "+387", "XX XXX-XXX"),
                new CountryCode("Botswana", "+267", "XX XXX XXX"),
                new CountryCode("Brazil", "+55", "(XX) XXXXX-XXXX"),
                new CountryCode("Brunei", "+673", "XXX XXXX"),
                new CountryCode("Bulgaria", "+359", "XX XXX XXXX"),
                new CountryCode("Burkina Faso", "+226", "XX XX XX XX"),
                new CountryCode("Burundi", "+257", "XX XX XXXX"),
                new CountryCode("Cabo Verde", "+238", "XXX XX XX"),
                new CountryCode("Cambodia", "+855", "XX XXX XXX"),
                new CountryCode("Cameroon", "+237", "X XX XX XX XX"),
                new CountryCode("Central African Republic", "+236", "XX XX XX XX"),
                new CountryCode("Chad", "+235", "XX XX XX XX"),
                new CountryCode("Chile", "+56", "X XXXX XXXX"),
                new CountryCode("Colombia", "+57", "XXX XXX XXXX"),
                new CountryCode("Comoros", "+269", "XXX XX XX"),
                new CountryCode("Costa Rica", "+506", "XXXX XXXX"),
                new CountryCode("Croatia", "+385", "XX XXX XXXX"),
                new CountryCode("Cuba", "+53", "X XXX XXXX"),
                new CountryCode("Cyprus", "+357", "XX XXXXXX"),
                new CountryCode("Czechia", "+420", "XXX XXX XXX"),
                new CountryCode("Denmark", "+45", "XX XX XX XX"),
                new CountryCode("Djibouti", "+253", "XX XX XX XX"),
                new CountryCode("Dominica", "+1-767", "XXX-XXXX"),
                new CountryCode("Dominican Republic", "+1-809", "XXX-XXXX"),
                new CountryCode("Ecuador", "+593", "XX XXX XXXX"),
                new CountryCode("Egypt", "+20", "XXX XXX XXXX"),
                new CountryCode("El Salvador", "+503", "XXXX XXXX"),
                new CountryCode("Equatorial Guinea", "+240", "XXX XXX XXX"),
                new CountryCode("Eritrea", "+291", "X XXX XXX"),
                new CountryCode("Estonia", "+372", "XXXX XXXX"),
                new CountryCode("Eswatini", "+268", "XXXX XXXX"),
                new CountryCode("Ethiopia", "+251", "XX XXX XXXX"),
                new CountryCode("Fiji", "+679", "XXX XXXX"),
                new CountryCode("Finland", "+358", "XX XXX XXXX"),
                new CountryCode("Gabon", "+241", "X XX XX XX"),
                new CountryCode("Gambia", "+220", "XXX XXXX"),
                new CountryCode("Georgia", "+995", "XXX XXX XXX"),
                new CountryCode("Ghana", "+233", "XX XXX XXXX"),
                new CountryCode("Greece", "+30", "XXX XXX XXXX"),
                new CountryCode("Grenada", "+1-473", "XXX-XXXX"),
                new CountryCode("Guatemala", "+502", "XXXX XXXX"),
                new CountryCode("Guinea", "+224", "XXX XX XX XX"),
                new CountryCode("Guinea-Bissau", "+245", "XXX XXXX"),
                new CountryCode("Guyana", "+592", "XXX XXXX"),
                new CountryCode("Haiti", "+509", "XXXX XXXX"),
                new CountryCode("Honduras", "+504", "XXXX XXXX"),
                new CountryCode("Hungary", "+36", "XX XXX XXXX"),
                new CountryCode("Iceland", "+354", "XXX XXXX"),
                new CountryCode("India", "+91", "XXXXX XXXXX"),
                new CountryCode("Indonesia", "+62", "XXX XXXX XXXX"),
                new CountryCode("Iran", "+98", "XXX XXX XXXX"),
                new CountryCode("Iraq", "+964", "XXX XXX XXXX"),
                new CountryCode("Ireland", "+353", "XX XXX XXXX"),
                new CountryCode("Israel", "+972", "XX XXX XXXX"),
                new CountryCode("Ivory Coast", "+225", "XX XX XX XXXX"),
                new CountryCode("Jamaica", "+1-876", "XXX-XXXX"),
                new CountryCode("Jordan", "+962", "X XXXX XXXX"),
                new CountryCode("Kazakhstan", "+7", "XXX XXX XX XX"),
                new CountryCode("Kenya", "+254", "XXX XXXXXX"),
                new CountryCode("Kiribati", "+686", "XXXX XXXX"),
                new CountryCode("Kuwait", "+965", "XXXX XXXX"),
                new CountryCode("Kyrgyzstan", "+996", "XXX XXXXXX"),
                new CountryCode("Lao", "+856", "XX XX XXX XXX"),
                new CountryCode("Latvia", "+371", "XX XXX XXX"),
                new CountryCode("Lebanon", "+961", "XX XXX XXX"),
                new CountryCode("Lesotho", "+266", "XX XXX XXX"),
                new CountryCode("Liberia", "+231", "XX XXX XXXX"),
                new CountryCode("Libya", "+218", "XX XXX XXXX"),
                new CountryCode("Liechtenstein", "+423", "XXX XXXX"),
                new CountryCode("Lithuania", "+370", "XXX XXXXX"),
                new CountryCode("Luxembourg", "+352", "XXX XXX XXX"),
                new CountryCode("Madagascar", "+261", "XX XX XXX XX"),
                new CountryCode("Malawi", "+265", "X XXXX XXXX"),
                new CountryCode("Malaysia", "+60", "XX XXXX XXXX"),
                new CountryCode("Maldives", "+960", "XXX XXXX"),
                new CountryCode("Mali", "+223", "XX XX XX XX"),
                new CountryCode("Malta", "+356", "XXXX XXXX"),
                new CountryCode("Marshall Islands", "+692", "XXX-XXXX"),
                new CountryCode("Mauritania", "+222", "XX XX XX XX"),
                new CountryCode("Mauritius", "+230", "XXXX XXXX"),
                new CountryCode("Mexico", "+52", "XXX XXX XXXX"),
                new CountryCode("Micronesia", "+691", "XXX XXXX"),
                new CountryCode("Moldova", "+373", "XX XXX XXX"),
                new CountryCode("Monaco", "+377", "XX XX XX XX"),
                new CountryCode("Mongolia", "+976", "XX XX XXXX"),
                new CountryCode("Montenegro", "+382", "XX XXX XXX"),
                new CountryCode("Morocco", "+212", "XX XXX XXXX"),
                new CountryCode("Mozambique", "+258", "XX XXX XXXX"),
                new CountryCode("Myanmar", "+95", "XX XXX XXXX"),
                new CountryCode("Namibia", "+264", "XX XXX XXXX"),
                new CountryCode("Nauru", "+674", "XXX XXXX"),
                new CountryCode("Nepal", "+977", "XX XXX XXXX"),
                new CountryCode("Netherlands", "+31", "XX XXXXXXXX"),
                new CountryCode("New Zealand", "+64", "XX XXX XXXX"),
                new CountryCode("Nicaragua", "+505", "XXXX XXXX"),
                new CountryCode("Niger", "+227", "XX XX XX XX"),
                new CountryCode("Nigeria", "+234", "XXX XXX XXXX"),
                new CountryCode("North Korea", "+850", "XXX XXXX XXXX"),
                new CountryCode("North Macedonia", "+389", "XX XXX XXX"),
                new CountryCode("Norway", "+47", "XXX XX XXX"),
                new CountryCode("Oman", "+968", "XXXX XXXX"),
                new CountryCode("Pakistan", "+92", "XXX XXX XXXX"),
                new CountryCode("Palau", "+680", "XXX XXXX"),
                new CountryCode("Panama", "+507", "XXXX XXXX"),
                new CountryCode("Papua New Guinea", "+675", "XXX XXXX"),
                new CountryCode("Paraguay", "+595", "XXX XXX XXX"),
                new CountryCode("Peru", "+51", "XXX XXX XXX"),
                new CountryCode("Philippines", "+63", "XXX XXX XXXX"),
                new CountryCode("Poland", "+48", "XXX XXX XXX"),
                new CountryCode("Portugal", "+351", "XXX XXX XXX"),
                new CountryCode("Qatar", "+974", "XXXX XXXX"),
                new CountryCode("Romania", "+40", "XXX XXX XXX"),
                new CountryCode("Russia", "+7", "XXX XXX-XX-XX"),
                new CountryCode("Rwanda", "+250", "XXX XXX XXX"),
                new CountryCode("Saint Kitts and Nevis", "+1-869", "XXX-XXXX"),
                new CountryCode("Saint Lucia", "+1-758", "XXX-XXXX"),
                new CountryCode("Saint Vincent and the Grenadines", "+1-784", "XXX-XXXX"),
                new CountryCode("Samoa", "+685", "XX XXXX"),
                new CountryCode("San Marino", "+378", "XXXX XXXXXX"),
                new CountryCode("Sao Tome and Principe", "+239", "XXX XXXX"),
                new CountryCode("Saudi Arabia", "+966", "XX XXX XXXX"),
                new CountryCode("Senegal", "+221", "XX XXX XX XX"),
                new CountryCode("Serbia", "+381", "XX XXX XXXX"),
                new CountryCode("Seychelles", "+248", "X XXX XXX"),
                new CountryCode("Sierra Leone", "+232", "XX XXXXXX"),
                new CountryCode("Singapore", "+65", "XXXX XXXX"),
                new CountryCode("Slovakia", "+421", "XXX XXX XXX"),
                new CountryCode("Slovenia", "+386", "XX XXX XXX"),
                new CountryCode("Solomon Islands", "+677", "XXXXX"),
                new CountryCode("Somalia", "+252", "XX XXX XXX"),
                new CountryCode("South Africa", "+27", "XX XXX XXXX"),
                new CountryCode("South Korea", "+82", "XX XXXX XXXX"),
                new CountryCode("South Sudan", "+211", "XX XXX XXXX"),
                new CountryCode("Spain", "+34", "XXX XXX XXX"),
                new CountryCode("Sri Lanka", "+94", "XX XXX XXXX"),
                new CountryCode("Sudan", "+249", "XX XXX XXXX"),
                new CountryCode("Suriname", "+597", "XXX XXXX"),
                new CountryCode("Sweden", "+46", "XX XXX XX XX"),
                new CountryCode("Switzerland", "+41", "XX XXX XX XX"),
                new CountryCode("Syria", "+963", "XX XXXX XXXX"),
                new CountryCode("Taiwan", "+886", "XXXX XXXX"),
                new CountryCode("Tajikistan", "+992", "XX XXX XXXX"),
                new CountryCode("Tanzania", "+255", "XXX XXX XXX"),
                new CountryCode("Thailand", "+66", "XX XXX XXXX"),
                new CountryCode("The Democratic Republic of the Congo", "+243", "XXX XXX XXX"),
                new CountryCode("The Republic of the Congo", "+242", "XX XXX XXXX"),
                new CountryCode("Timor-Leste", "+670", "XXX XXXX"),
                new CountryCode("Togo", "+228", "XX XX XX XX"),
                new CountryCode("Tonga", "+676", "XXXXX"),
                new CountryCode("Trinidad and Tobago", "+1-868", "XXX-XXXX"),
                new CountryCode("Tunisia", "+216", "XX XXX XXX"),
                new CountryCode("Turkey", "+90", "XXX XXX XX XX"),
                new CountryCode("Turkmenistan", "+993", "XX XXXXXX"),
                new CountryCode("Tuvalu", "+688", "XXXXX"),
                new CountryCode("Uganda", "+256", "XXX XXXXXX"),
                new CountryCode("Ukraine", "+380", "XX XXX XX XX"),
                new CountryCode("United Arab Emirates", "+971", "XX XXX XXXX"),
                new CountryCode("Uruguay", "+598", "X XXX XXXX"),
                new CountryCode("Uzbekistan", "+998", "XX XXX XX XX"),
                new CountryCode("Vanuatu", "+678", "XXXXX"),
                new CountryCode("Venezuela", "+58", "XXX XXX XXXX"),
                new CountryCode("Vietnam", "+84", "XX XXXX XXXX"),
                new CountryCode("Western Sahara", "+212", "XX XXX XXXX"),
                new CountryCode("Yemen", "+967", "XXX XXX XXX"),
                new CountryCode("Zambia", "+260", "XX XXX XXXX"),
                new CountryCode("Zimbabwe", "+263", "XX XXX XXXX")
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

            // Add popular countries first (matching Country.cs order)
            string[] popularCountries = { "United States", "China", "Germany", "Japan", "United Kingdom", "France", "Italy", "Canada" };
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
