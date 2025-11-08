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
                    // Only add formatting character if we have digits to format
                    if (digitIndex > 0 && digitIndex < digitsOnly.Length)
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
    }
}
