using ClosedXML.Excel;
using Guna.UI2.WinForms;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using Sales_Tracker.DataClasses;
using Sales_Tracker.UI;
using System.Diagnostics;

namespace Sales_Tracker.Classes
{
    /// <summary>
    /// Handles the import and export of data to and from .xlsx spreadsheets, including data for 
    /// accountants, companies, categories, products, and transactoins.
    /// </summary>
    internal class ExcelSheetManager
    {
        // Constants
        private const string _decimalFormatPattern = "#,##0.00";
        private const string _currencyFormatPattern = "\"$\"#,##0.00";
        private const string _numberFormatPattern = "0";

        // Import spreadsheet methods
        public static bool ImportAccountantsData(IXLWorksheet worksheet, bool includeHeader)
        {
            return ImportSimpleListData(
                worksheet,
                includeHeader,
                MainMenu_Form.Instance.AccountantList,
                MainMenu_Form.SelectedOption.Accountants,
                "Accountant");
        }
        public static bool ImportCompaniesData(IXLWorksheet worksheet, bool includeHeader)
        {
            return ImportSimpleListData(
                worksheet,
                includeHeader,
                MainMenu_Form.Instance.CompanyList,
                MainMenu_Form.SelectedOption.Companies,
                "Company");
        }

        /// <summary>
        /// Generic method to import simple list data like accountants or companies
        /// </summary>
        private static bool ImportSimpleListData(
            IXLWorksheet worksheet,
            bool includeHeader,
            List<string> existingList,
            MainMenu_Form.SelectedOption optionType,
            string itemTypeName)
        {
            IEnumerable<IXLRow> rowsToProcess = includeHeader
                ? worksheet.RowsUsed()
                : worksheet.RowsUsed().Skip(1);

            bool wasSomethingImported = false;

            HashSet<string> existingItems = new(existingList.Count);
            foreach (string item in existingList)
            {
                existingItems.Add(item.ToLowerInvariant());
            }

            HashSet<string> addedDuringImport = [];

            foreach (IXLRow row in rowsToProcess)
            {
                string itemName = row.Cell(1).GetValue<string>();
                string itemNameLower = itemName.ToLowerInvariant();

                if (existingItems.Contains(itemNameLower))
                {
                    CustomMessageBox.Show(
                        $"{itemTypeName} already exists",
                        $"The {itemTypeName.ToLowerInvariant()} {itemName} already exists and will not be imported",
                        CustomMessageBoxIcon.Question, CustomMessageBoxButtons.Ok);
                }
                else
                {
                    existingList.Add(itemName);
                    addedDuringImport.Add(itemNameLower);
                    wasSomethingImported = true;
                }
            }

            MainMenu_Form.SaveListToFile(existingList, optionType);
            return wasSomethingImported;
        }
        public static bool ImportProductsData(IXLWorksheet worksheet, bool purchase, bool includeHeader)
        {
            IEnumerable<IXLRow> rowsToProcess = includeHeader
                ? worksheet.RowsUsed()
                : worksheet.RowsUsed().Skip(1);

            bool wasSomethingImported = false;

            List<Category> list = purchase
                ? MainMenu_Form.Instance.CategoryPurchaseList
                : MainMenu_Form.Instance.CategorySaleList;

            Dictionary<string, HashSet<string>> existingProducts = [];
            foreach (Category category in list)
            {
                existingProducts[category.Name] = [.. category.ProductList.Select(p => p.Name.ToLowerInvariant())];
            }

            Dictionary<string, HashSet<string>> addedDuringImport = [];

            // Read product data from the worksheet and add it to the category purchase list
            foreach (IXLRow row in rowsToProcess)
            {
                string productId = row.Cell(1).GetValue<string>();
                string productName = row.Cell(2).GetValue<string>();
                string categoryName = row.Cell(3).GetValue<string>();
                string countryOfOrigin = row.Cell(4).GetValue<string>();
                string companyOfOrigin = row.Cell(5).GetValue<string>();

                // Skip if any essential field is empty
                if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(categoryName))
                {
                    continue;
                }

                // Process US country variants
                countryOfOrigin = NormalizeCountryName(countryOfOrigin);

                // Validate country exists in system
                if (!ValidateCountry(countryOfOrigin))
                {
                    continue;
                }

                // Ensure company exists
                EnsureCompanyExists(companyOfOrigin);

                // Find or create the category
                Category category = FindOrCreateCategory(
                    list,
                    categoryName,
                    existingProducts,
                    addedDuringImport);

                string productNameLower = productName.ToLowerInvariant();

                // Check if product already exists
                if (ProductExists(
                    existingProducts,
                    categoryName,
                    productNameLower,
                    productName,
                    purchase))
                {
                    continue;
                }

                // Create the product and add it to the category's ProductList
                AddProductToCategory(
                    category,
                    productId,
                    productName,
                    productNameLower,
                    countryOfOrigin,
                    companyOfOrigin,
                    addedDuringImport,
                    categoryName);

                wasSomethingImported = true;
            }

            MainMenu_Form.Instance.SaveCategoriesToFile(purchase
                ? MainMenu_Form.SelectedOption.CategoryPurchases
                : MainMenu_Form.SelectedOption.CategorySales);

            return wasSomethingImported;
        }
        private static string NormalizeCountryName(string countryOfOrigin)
        {
            if (string.IsNullOrWhiteSpace(countryOfOrigin))
            {
                return countryOfOrigin;
            }

            // Create a case-insensitive comparison dictionary for country variants
            Dictionary<string, string> countryVariants = new(StringComparer.OrdinalIgnoreCase)
            {
                // United States variants
                ["US"] = "United States",
                ["USA"] = "United States",
                ["U.S."] = "United States",
                ["U.S.A."] = "United States",
                ["United States of America"] = "United States",
                ["America"] = "United States",
                ["States"] = "United States",
                ["The States"] = "United States",

                // China variants
                ["CN"] = "China",
                ["CHN"] = "China",
                ["PRC"] = "China",
                ["People's Republic of China"] = "China",
                ["Mainland China"] = "China",

                // Germany variants
                ["DE"] = "Germany",
                ["DEU"] = "Germany",
                ["Deutschland"] = "Germany",
                ["Federal Republic of Germany"] = "Germany",

                // Japan variants
                ["JP"] = "Japan",
                ["JPN"] = "Japan",
                ["Nippon"] = "Japan",

                // United Kingdom variants
                ["GB"] = "United Kingdom",
                ["GBR"] = "United Kingdom",
                ["UK"] = "United Kingdom",
                ["U.K."] = "United Kingdom",
                ["Great Britain"] = "United Kingdom",
                ["Britain"] = "United Kingdom",
                ["England"] = "United Kingdom",
                ["Scotland"] = "United Kingdom",
                ["Wales"] = "United Kingdom",
                ["Northern Ireland"] = "United Kingdom",

                // France variants
                ["FR"] = "France",
                ["FRA"] = "France",
                ["French Republic"] = "France",

                // Italy variants
                ["IT"] = "Italy",
                ["ITA"] = "Italy",
                ["Italian Republic"] = "Italy",

                // Canada variants
                ["CA"] = "Canada",
                ["CAN"] = "Canada",

                // Afghanistan variants
                ["AF"] = "Afghanistan",
                ["AFG"] = "Afghanistan",

                // Albania variants
                ["AL"] = "Albania",
                ["ALB"] = "Albania",

                // Algeria variants
                ["DZ"] = "Algeria",
                ["DZA"] = "Algeria",

                // Andorra variants
                ["AD"] = "Andorra",
                ["AND"] = "Andorra",

                // Angola variants
                ["AO"] = "Angola",
                ["AGO"] = "Angola",

                // Antigua and Barbuda variants
                ["AG"] = "Antigua and Barbuda",
                ["ATG"] = "Antigua and Barbuda",

                // Argentina variants
                ["AR"] = "Argentina",
                ["ARG"] = "Argentina",
                ["Argentine Republic"] = "Argentina",

                // Armenia variants
                ["AM"] = "Armenia",
                ["ARM"] = "Armenia",

                // Australia variants
                ["AU"] = "Australia",
                ["AUS"] = "Australia",
                ["Commonwealth of Australia"] = "Australia",

                // Austria variants
                ["AT"] = "Austria",
                ["AUT"] = "Austria",
                ["Republic of Austria"] = "Austria",

                // Azerbaijan variants
                ["AZ"] = "Azerbaijan",
                ["AZE"] = "Azerbaijan",

                // Bahamas variants
                ["BS"] = "Bahamas",
                ["BHS"] = "Bahamas",

                // Bahrain variants
                ["BH"] = "Bahrain",
                ["BHR"] = "Bahrain",

                // Bangladesh variants
                ["BD"] = "Bangladesh",
                ["BGD"] = "Bangladesh",

                // Barbados variants
                ["BB"] = "Barbados",
                ["BRB"] = "Barbados",

                // Belarus variants
                ["BY"] = "Belarus",
                ["BLR"] = "Belarus",
                ["Republic of Belarus"] = "Belarus",

                // Belgium variants
                ["BE"] = "Belgium",
                ["BEL"] = "Belgium",
                ["Kingdom of Belgium"] = "Belgium",

                // Belize variants
                ["BZ"] = "Belize",
                ["BLZ"] = "Belize",

                // Benin variants
                ["BJ"] = "Benin",
                ["BEN"] = "Benin",

                // Bhutan variants
                ["BT"] = "Bhutan",
                ["BTN"] = "Bhutan",

                // Bolivia variants
                ["BO"] = "Bolivia",
                ["BOL"] = "Bolivia",

                // Bosnia and Herzegovina variants
                ["BA"] = "Bosnia and Herzegovina",
                ["BIH"] = "Bosnia and Herzegovina",

                // Botswana variants
                ["BW"] = "Botswana",
                ["BWA"] = "Botswana",

                // Brazil variants
                ["BR"] = "Brazil",
                ["BRA"] = "Brazil",
                ["Federative Republic of Brazil"] = "Brazil",

                // Brunei variants
                ["BN"] = "Brunei",
                ["BRN"] = "Brunei",

                // Bulgaria variants
                ["BG"] = "Bulgaria",
                ["BGR"] = "Bulgaria",
                ["Republic of Bulgaria"] = "Bulgaria",

                // Burkina Faso variants
                ["BF"] = "Burkina Faso",
                ["BFA"] = "Burkina Faso",

                // Burundi variants
                ["BI"] = "Burundi",
                ["BDI"] = "Burundi",

                // Cabo Verde variants
                ["CV"] = "Cabo Verde",
                ["CPV"] = "Cabo Verde",
                ["Cape Verde"] = "Cabo Verde",

                // Cambodia variants
                ["KH"] = "Cambodia",
                ["KHM"] = "Cambodia",

                // Cameroon variants
                ["CM"] = "Cameroon",
                ["CMR"] = "Cameroon",

                // Central African Republic variants
                ["CF"] = "Central African Republic",
                ["CAF"] = "Central African Republic",

                // Chad variants
                ["TD"] = "Chad",
                ["TCD"] = "Chad",

                // Chile variants
                ["CL"] = "Chile",
                ["CHL"] = "Chile",

                // Colombia variants
                ["CO"] = "Colombia",
                ["COL"] = "Colombia",

                // Comoros variants
                ["KM"] = "Comoros",
                ["COM"] = "Comoros",

                // Costa Rica variants
                ["CR"] = "Costa Rica",
                ["CRI"] = "Costa Rica",

                // Croatia variants
                ["HR"] = "Croatia",
                ["HRV"] = "Croatia",
                ["Republic of Croatia"] = "Croatia",

                // Cuba variants
                ["CU"] = "Cuba",
                ["CUB"] = "Cuba",

                // Cyprus variants
                ["CY"] = "Cyprus",
                ["CYP"] = "Cyprus",

                // Czechia variants
                ["CZ"] = "Czechia",
                ["CZE"] = "Czechia",
                ["Czech Republic"] = "Czechia",

                // Denmark variants
                ["DK"] = "Denmark",
                ["DNK"] = "Denmark",
                ["Kingdom of Denmark"] = "Denmark",

                // Djibouti variants
                ["DJ"] = "Djibouti",
                ["DJI"] = "Djibouti",

                // Dominica variants
                ["DM"] = "Dominica",
                ["DMA"] = "Dominica",

                // Dominican Republic variants
                ["DO"] = "Dominican Republic",
                ["DOM"] = "Dominican Republic",

                // Ecuador variants
                ["EC"] = "Ecuador",
                ["ECU"] = "Ecuador",

                // Egypt variants
                ["EG"] = "Egypt",
                ["EGY"] = "Egypt",
                ["Arab Republic of Egypt"] = "Egypt",

                // El Salvador variants
                ["SV"] = "El Salvador",
                ["SLV"] = "El Salvador",

                // Equatorial Guinea variants
                ["GQ"] = "Equatorial Guinea",
                ["GNQ"] = "Equatorial Guinea",

                // Eritrea variants
                ["ER"] = "Eritrea",
                ["ERI"] = "Eritrea",

                // Estonia variants
                ["EE"] = "Estonia",
                ["EST"] = "Estonia",
                ["Republic of Estonia"] = "Estonia",

                // Eswatini variants
                ["SZ"] = "Eswatini",
                ["SWZ"] = "Eswatini",
                ["Swaziland"] = "Eswatini",

                // Ethiopia variants
                ["ET"] = "Ethiopia",
                ["ETH"] = "Ethiopia",

                // Fiji variants
                ["FJ"] = "Fiji",
                ["FJI"] = "Fiji",

                // Finland variants
                ["FI"] = "Finland",
                ["FIN"] = "Finland",
                ["Republic of Finland"] = "Finland",

                // Gabon variants
                ["GA"] = "Gabon",
                ["GAB"] = "Gabon",

                // Gambia variants
                ["GM"] = "Gambia",
                ["GMB"] = "Gambia",

                // Georgia variants
                ["GE"] = "Georgia",
                ["GEO"] = "Georgia",

                // Ghana variants
                ["GH"] = "Ghana",
                ["GHA"] = "Ghana",

                // Greece variants
                ["GR"] = "Greece",
                ["GRC"] = "Greece",
                ["Hellenic Republic"] = "Greece",

                // Grenada variants
                ["GD"] = "Grenada",
                ["GRD"] = "Grenada",

                // Guatemala variants
                ["GT"] = "Guatemala",
                ["GTM"] = "Guatemala",

                // Guinea variants
                ["GN"] = "Guinea",
                ["GIN"] = "Guinea",

                // Guinea-Bissau variants
                ["GW"] = "Guinea-Bissau",
                ["GNB"] = "Guinea-Bissau",

                // Guyana variants
                ["GY"] = "Guyana",
                ["GUY"] = "Guyana",

                // Haiti variants
                ["HT"] = "Haiti",
                ["HTI"] = "Haiti",

                // Honduras variants
                ["HN"] = "Honduras",
                ["HND"] = "Honduras",

                // Hungary variants
                ["HU"] = "Hungary",
                ["HUN"] = "Hungary",

                // Iceland variants
                ["IS"] = "Iceland",
                ["ISL"] = "Iceland",

                // India variants
                ["IN"] = "India",
                ["IND"] = "India",
                ["Republic of India"] = "India",
                ["Bharat"] = "India",

                // Indonesia variants
                ["ID"] = "Indonesia",
                ["IDN"] = "Indonesia",

                // Iran variants
                ["IR"] = "Iran",
                ["IRN"] = "Iran",

                // Iraq variants
                ["IQ"] = "Iraq",
                ["IRQ"] = "Iraq",

                // Ireland variants
                ["IE"] = "Ireland",
                ["IRL"] = "Ireland",
                ["Republic of Ireland"] = "Ireland",

                // Israel variants
                ["IL"] = "Israel",
                ["ISR"] = "Israel",
                ["State of Israel"] = "Israel",

                // Ivory Coast variants
                ["CI"] = "Ivory Coast",
                ["CIV"] = "Ivory Coast",
                ["Côte d'Ivoire"] = "Ivory Coast",

                // Jamaica variants
                ["JM"] = "Jamaica",
                ["JAM"] = "Jamaica",

                // Jordan variants
                ["JO"] = "Jordan",
                ["JOR"] = "Jordan",

                // Kazakhstan variants
                ["KZ"] = "Kazakhstan",
                ["KAZ"] = "Kazakhstan",

                // Kenya variants
                ["KE"] = "Kenya",
                ["KEN"] = "Kenya",

                // Kiribati variants
                ["KI"] = "Kiribati",
                ["KIR"] = "Kiribati",

                // Kuwait variants
                ["KW"] = "Kuwait",
                ["KWT"] = "Kuwait",

                // Kyrgyzstan variants
                ["KG"] = "Kyrgyzstan",
                ["KGZ"] = "Kyrgyzstan",

                // Lao variants
                ["LA"] = "Lao",
                ["LAO"] = "Lao",
                ["Laos"] = "Lao",

                // Latvia variants
                ["LV"] = "Latvia",
                ["LVA"] = "Latvia",
                ["Republic of Latvia"] = "Latvia",

                // Lebanon variants
                ["LB"] = "Lebanon",
                ["LBN"] = "Lebanon",

                // Lesotho variants
                ["LS"] = "Lesotho",
                ["LSO"] = "Lesotho",

                // Liberia variants
                ["LR"] = "Liberia",
                ["LBR"] = "Liberia",

                // Libya variants
                ["LY"] = "Libya",
                ["LBY"] = "Libya",

                // Liechtenstein variants
                ["LI"] = "Liechtenstein",
                ["LIE"] = "Liechtenstein",

                // Lithuania variants
                ["LT"] = "Lithuania",
                ["LTU"] = "Lithuania",
                ["Republic of Lithuania"] = "Lithuania",

                // Luxembourg variants
                ["LU"] = "Luxembourg",
                ["LUX"] = "Luxembourg",

                // Madagascar variants
                ["MG"] = "Madagascar",
                ["MDG"] = "Madagascar",

                // Malawi variants
                ["MW"] = "Malawi",
                ["MWI"] = "Malawi",

                // Malaysia variants
                ["MY"] = "Malaysia",
                ["MYS"] = "Malaysia",

                // Maldives variants
                ["MV"] = "Maldives",
                ["MDV"] = "Maldives",

                // Mali variants
                ["ML"] = "Mali",
                ["MLI"] = "Mali",

                // Malta variants
                ["MT"] = "Malta",
                ["MLT"] = "Malta",

                // Marshall Islands variants
                ["MH"] = "Marshall Islands",
                ["MHL"] = "Marshall Islands",

                // Mauritania variants
                ["MR"] = "Mauritania",
                ["MRT"] = "Mauritania",

                // Mauritius variants
                ["MU"] = "Mauritius",
                ["MUS"] = "Mauritius",

                // Mexico variants
                ["MX"] = "Mexico",
                ["MEX"] = "Mexico",
                ["United Mexican States"] = "Mexico",

                // Micronesia variants
                ["FM"] = "Micronesia",
                ["FSM"] = "Micronesia",

                // Moldova variants
                ["MD"] = "Moldova",
                ["MDA"] = "Moldova",
                ["Republic of Moldova"] = "Moldova",

                // Monaco variants
                ["MC"] = "Monaco",
                ["MCO"] = "Monaco",

                // Mongolia variants
                ["MN"] = "Mongolia",
                ["MNG"] = "Mongolia",

                // Montenegro variants
                ["ME"] = "Montenegro",
                ["MNE"] = "Montenegro",

                // Morocco variants
                ["MA"] = "Morocco",
                ["MAR"] = "Morocco",

                // Mozambique variants
                ["MZ"] = "Mozambique",
                ["MOZ"] = "Mozambique",

                // Myanmar variants
                ["MM"] = "Myanmar",
                ["MMR"] = "Myanmar",
                ["Burma"] = "Myanmar",

                // Namibia variants
                ["NA"] = "Namibia",
                ["NAM"] = "Namibia",

                // Nauru variants
                ["NR"] = "Nauru",
                ["NRU"] = "Nauru",

                // Nepal variants
                ["NP"] = "Nepal",
                ["NPL"] = "Nepal",

                // Netherlands variants
                ["NL"] = "Netherlands",
                ["NLD"] = "Netherlands",
                ["Holland"] = "Netherlands",
                ["Kingdom of the Netherlands"] = "Netherlands",

                // New Zealand variants
                ["NZ"] = "New Zealand",
                ["NZL"] = "New Zealand",
                ["Aotearoa"] = "New Zealand",

                // Nicaragua variants
                ["NI"] = "Nicaragua",
                ["NIC"] = "Nicaragua",

                // Niger variants
                ["NE"] = "Niger",
                ["NER"] = "Niger",

                // Nigeria variants
                ["NG"] = "Nigeria",
                ["NGA"] = "Nigeria",

                // North Korea variants
                ["KP"] = "North Korea",
                ["PRK"] = "North Korea",
                ["Democratic People's Republic of Korea"] = "North Korea",
                ["DPRK"] = "North Korea",

                // North Macedonia variants
                ["MK"] = "North Macedonia",
                ["MKD"] = "North Macedonia",
                ["Macedonia"] = "North Macedonia",

                // Norway variants
                ["NO"] = "Norway",
                ["NOR"] = "Norway",
                ["Kingdom of Norway"] = "Norway",

                // Oman variants
                ["OM"] = "Oman",
                ["OMN"] = "Oman",

                // Pakistan variants
                ["PK"] = "Pakistan",
                ["PAK"] = "Pakistan",

                // Palau variants
                ["PW"] = "Palau",
                ["PLW"] = "Palau",

                // Panama variants
                ["PA"] = "Panama",
                ["PAN"] = "Panama",

                // Papua New Guinea variants
                ["PG"] = "Papua New Guinea",
                ["PNG"] = "Papua New Guinea",

                // Paraguay variants
                ["PY"] = "Paraguay",
                ["PRY"] = "Paraguay",

                // Peru variants
                ["PE"] = "Peru",
                ["PER"] = "Peru",

                // Philippines variants
                ["PH"] = "Philippines",
                ["PHL"] = "Philippines",

                // Poland variants
                ["PL"] = "Poland",
                ["POL"] = "Poland",
                ["Republic of Poland"] = "Poland",

                // Portugal variants
                ["PT"] = "Portugal",
                ["PRT"] = "Portugal",
                ["Portuguese Republic"] = "Portugal",

                // Qatar variants
                ["QA"] = "Qatar",
                ["QAT"] = "Qatar",

                // Romania variants
                ["RO"] = "Romania",
                ["ROU"] = "Romania",

                // Russia variants
                ["RU"] = "Russia",
                ["RUS"] = "Russia",
                ["Russian Federation"] = "Russia",
                ["USSR"] = "Russia",
                ["Soviet Union"] = "Russia",

                // Rwanda variants
                ["RW"] = "Rwanda",
                ["RWA"] = "Rwanda",

                // Saint Kitts and Nevis variants
                ["KN"] = "Saint Kitts and Nevis",
                ["KNA"] = "Saint Kitts and Nevis",

                // Saint Lucia variants
                ["LC"] = "Saint Lucia",
                ["LCA"] = "Saint Lucia",

                // Saint Vincent and the Grenadines variants
                ["VC"] = "Saint Vincent and the Grenadines",
                ["VCT"] = "Saint Vincent and the Grenadines",

                // Samoa variants
                ["WS"] = "Samoa",
                ["WSM"] = "Samoa",

                // San Marino variants
                ["SM"] = "San Marino",
                ["SMR"] = "San Marino",

                // Sao Tome and Principe variants
                ["ST"] = "Sao Tome and Principe",
                ["STP"] = "Sao Tome and Principe",

                // Saudi Arabia variants
                ["SA"] = "Saudi Arabia",
                ["SAU"] = "Saudi Arabia",

                // Senegal variants
                ["SN"] = "Senegal",
                ["SEN"] = "Senegal",

                // Serbia variants
                ["RS"] = "Serbia",
                ["SRB"] = "Serbia",

                // Seychelles variants
                ["SC"] = "Seychelles",
                ["SYC"] = "Seychelles",

                // Sierra Leone variants
                ["SL"] = "Sierra Leone",
                ["SLE"] = "Sierra Leone",

                // Singapore variants
                ["SG"] = "Singapore",
                ["SGP"] = "Singapore",
                ["Republic of Singapore"] = "Singapore",

                // Slovakia variants
                ["SK"] = "Slovakia",
                ["SVK"] = "Slovakia",
                ["Slovak Republic"] = "Slovakia",

                // Slovenia variants
                ["SI"] = "Slovenia",
                ["SVN"] = "Slovenia",
                ["Republic of Slovenia"] = "Slovenia",

                // Solomon Islands variants
                ["SB"] = "Solomon Islands",
                ["SLB"] = "Solomon Islands",

                // Somalia variants
                ["SO"] = "Somalia",
                ["SOM"] = "Somalia",

                // South Africa variants
                ["ZA"] = "South Africa",
                ["ZAF"] = "South Africa",
                ["Republic of South Africa"] = "South Africa",

                // South Korea variants
                ["KR"] = "South Korea",
                ["KOR"] = "South Korea",
                ["Korea"] = "South Korea",
                ["Republic of Korea"] = "South Korea",
                ["ROK"] = "South Korea",

                // South Sudan variants
                ["SS"] = "South Sudan",
                ["SSD"] = "South Sudan",

                // Spain variants
                ["ES"] = "Spain",
                ["ESP"] = "Spain",
                ["Kingdom of Spain"] = "Spain",

                // Sri Lanka variants
                ["LK"] = "Sri Lanka",
                ["LKA"] = "Sri Lanka",

                // Sudan variants
                ["SD"] = "Sudan",
                ["SDN"] = "Sudan",

                // Suriname variants
                ["SR"] = "Suriname",
                ["SUR"] = "Suriname",

                // Sweden variants
                ["SE"] = "Sweden",
                ["SWE"] = "Sweden",
                ["Kingdom of Sweden"] = "Sweden",

                // Switzerland variants
                ["CH"] = "Switzerland",
                ["CHE"] = "Switzerland",
                ["Swiss Confederation"] = "Switzerland",

                // Syria variants
                ["SY"] = "Syria",
                ["SYR"] = "Syria",

                // Taiwan variants
                ["TW"] = "Taiwan",
                ["TWN"] = "Taiwan",
                ["ROC"] = "Taiwan",
                ["Republic of China"] = "Taiwan",

                // Tajikistan variants
                ["TJ"] = "Tajikistan",
                ["TJK"] = "Tajikistan",

                // Tanzania variants
                ["TZ"] = "Tanzania",
                ["TZA"] = "Tanzania",

                // Thailand variants
                ["TH"] = "Thailand",
                ["THA"] = "Thailand",
                ["Kingdom of Thailand"] = "Thailand",

                // The Democratic Republic of the Congo variants
                ["CD"] = "The Democratic Republic of the Congo",
                ["COD"] = "The Democratic Republic of the Congo",
                ["DRC"] = "The Democratic Republic of the Congo",
                ["Congo-Kinshasa"] = "The Democratic Republic of the Congo",

                // The Republic of the Congo variants
                ["CG"] = "The Republic of the Congo",
                ["COG"] = "The Republic of the Congo",
                ["Congo-Brazzaville"] = "The Republic of the Congo",

                // Timor-Leste variants
                ["TL"] = "Timor-Leste",
                ["TLS"] = "Timor-Leste",
                ["East Timor"] = "Timor-Leste",

                // Togo variants
                ["TG"] = "Togo",
                ["TGO"] = "Togo",

                // Tonga variants
                ["TO"] = "Tonga",
                ["TON"] = "Tonga",

                // Trinidad and Tobago variants
                ["TT"] = "Trinidad and Tobago",
                ["TTO"] = "Trinidad and Tobago",

                // Tunisia variants
                ["TN"] = "Tunisia",
                ["TUN"] = "Tunisia",

                // Turkey variants
                ["TR"] = "Turkey",
                ["TUR"] = "Turkey",
                ["Republic of Turkey"] = "Turkey",

                // Turkmenistan variants
                ["TM"] = "Turkmenistan",
                ["TKM"] = "Turkmenistan",

                // Tuvalu variants
                ["TV"] = "Tuvalu",
                ["TUV"] = "Tuvalu",

                // Uganda variants
                ["UG"] = "Uganda",
                ["UGA"] = "Uganda",

                // Ukraine variants
                ["UA"] = "Ukraine",
                ["UKR"] = "Ukraine",

                // United Arab Emirates variants
                ["AE"] = "United Arab Emirates",
                ["ARE"] = "United Arab Emirates",
                ["UAE"] = "United Arab Emirates",

                // Uruguay variants
                ["UY"] = "Uruguay",
                ["URY"] = "Uruguay",

                // Uzbekistan variants
                ["UZ"] = "Uzbekistan",
                ["UZB"] = "Uzbekistan",

                // Vanuatu variants
                ["VU"] = "Vanuatu",
                ["VUT"] = "Vanuatu",

                // Venezuela variants
                ["VE"] = "Venezuela",
                ["VEN"] = "Venezuela",

                // Vietnam variants
                ["VN"] = "Vietnam",
                ["VNM"] = "Vietnam",
                ["Socialist Republic of Vietnam"] = "Vietnam",

                // Western Sahara variants
                ["EH"] = "Western Sahara",
                ["ESH"] = "Western Sahara",

                // Yemen variants
                ["YE"] = "Yemen",
                ["YEM"] = "Yemen",

                // Zambia variants
                ["ZM"] = "Zambia",
                ["ZMB"] = "Zambia",

                // Zimbabwe variants
                ["ZW"] = "Zimbabwe",
                ["ZWE"] = "Zimbabwe"
            };

            // Check if the country name is in our variants dictionary
            if (countryVariants.TryGetValue(countryOfOrigin, out string normalizedName))
            {
                return normalizedName;
            }

            // If no variant found, return the original name
            return countryOfOrigin;
        }
        private static bool ValidateCountry(string countryName)
        {
            bool countryExists = Country.CountrySearchResults.Any(
                c => c.Name.Equals(countryName, StringComparison.OrdinalIgnoreCase));

            if (!countryExists)
            {
                CustomMessageBoxResult result = CustomMessageBox.Show(
                    "Country does not exist",
                    $"Country '{countryName}' does not exist in the system. Please check the tutorial for more information. Do you want to skip this product and continue?",
                    CustomMessageBoxIcon.Exclamation, CustomMessageBoxButtons.YesNo);

                return result != CustomMessageBoxResult.Yes;
            }

            return true;
        }
        private static void EnsureCompanyExists(string companyName)
        {
            if (!MainMenu_Form.Instance.CompanyList.Contains(companyName))
            {
                MainMenu_Form.Instance.CompanyList.Add(companyName);
            }
        }

        private static Category FindOrCreateCategory(
            List<Category> list,
            string categoryName,
            Dictionary<string, HashSet<string>> existingProducts,
            Dictionary<string, HashSet<string>> addedDuringImport)
        {
            Category category = list.FirstOrDefault(c => c.Name == categoryName);

            if (category == null)
            {
                category = new Category { Name = categoryName };
                list.Add(category);
                existingProducts[categoryName] = [];
                addedDuringImport[categoryName] = [];
            }
            else if (!addedDuringImport.ContainsKey(categoryName))
            {
                addedDuringImport[categoryName] = [];
            }

            return category;
        }

        private static bool ProductExists(
            Dictionary<string, HashSet<string>> existingProducts,
            string categoryName,
            string productNameLower,
            string productName,
            bool purchase)
        {
            if (existingProducts.TryGetValue(categoryName, out HashSet<string> existingCategoryProducts) &&
                existingCategoryProducts.Contains(productNameLower))
            {
                string type = purchase ? "purchase" : "sale";

                CustomMessageBox.Show(
                    "Product already exists",
                    $"The product for {type} '{productName}' already exists and will not be imported",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.Ok);

                return true;
            }

            return false;
        }

        private static void AddProductToCategory(
            Category category,
            string productId,
            string productName,
            string productNameLower,
            string countryOfOrigin,
            string companyOfOrigin,
            Dictionary<string, HashSet<string>> addedDuringImport,
            string categoryName)
        {
            Product product = new()
            {
                ProductID = productId,
                Name = productName,
                CountryOfOrigin = countryOfOrigin,
                CompanyOfOrigin = companyOfOrigin
            };

            category.ProductList.Add(product);

            // Track that we've added this product
            if (!addedDuringImport.TryGetValue(categoryName, out HashSet<string> productsSet))
            {
                productsSet = [];
                addedDuringImport[categoryName] = productsSet;
            }
            productsSet.Add(productNameLower);
        }

        public static (bool, bool) ImportPurchaseData(IXLWorksheet worksheet, bool includeHeader)
        {
            return ImportTransactionData(worksheet, includeHeader, true);
        }
        public static (bool, bool) ImportSalesData(IXLWorksheet worksheet, bool includeHeader)
        {
            return ImportTransactionData(worksheet, includeHeader, false);
        }
        public static bool ImportReceiptsData(IXLWorksheet worksheet, bool includeHeader, string receiptsFolderPath, bool isPurchase)
        {
            IEnumerable<IXLRow> rowsToProcess = includeHeader
                ? worksheet.RowsUsed()
                : worksheet.RowsUsed().Skip(1);

            bool wasSomethingImported = false;

            foreach (IXLRow row in rowsToProcess)
            {
                string transactionId = row.Cell(1).GetValue<string>();
                string receiptFileName = row.Cell(17).GetValue<string>();

                // Skip if any essential field is empty
                if (string.IsNullOrWhiteSpace(transactionId) ||
                    string.IsNullOrWhiteSpace(receiptFileName) ||
                    string.IsNullOrWhiteSpace(receiptsFolderPath) ||
                    receiptFileName == ReadOnlyVariables.EmptyCell)
                {
                    continue;
                }

                string receiptFilePath = Path.Combine(receiptsFolderPath, receiptFileName);

                // Check if the receipt file exists
                if (!File.Exists(receiptFilePath))
                {
                    Log.Error_FileDoesNotExist(receiptFilePath);
                    continue;
                }

                // Find the transaction in the correct DataGridView
                DataGridViewRow? targetRow = FindTransactionRow(transactionId, isPurchase);
                if (targetRow == null)
                {
                    string transactionType = isPurchase ? "purchase" : "sale";
                    Log.Write(1, $"{transactionType} {transactionId} not found for receipt {receiptFileName}");
                    continue;
                }

                // Check if transaction already has a receipt
                if (TransactionHasReceipt(targetRow))
                {
                    string transactionType = isPurchase ? "purchase" : "sale";
                    CustomMessageBoxResult result = CustomMessageBox.Show(
                        "Transaction already has receipt",
                        $"{transactionType} {transactionId} already has a receipt. Do you want to replace it?",
                        CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                    if (result != CustomMessageBoxResult.Yes)
                    {
                        continue;
                    }
                }

                // Copy the receipt file to the receipts directory
                (string newReceiptPath, bool saved) = ReceiptManager.SaveReceiptInFile(receiptFilePath);
                if (!saved)
                {
                    continue;
                }

                ReceiptManager.AddReceiptToTag(targetRow, newReceiptPath);
                MainMenu_Form.SetHasReceiptColumn(targetRow, newReceiptPath);

                wasSomethingImported = true;
            }

            // Save the updated transaction data
            if (wasSomethingImported)
            {
                MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Purchase_DataGridView, MainMenu_Form.SelectedOption.Purchases);
                MainMenu_Form.SaveDataGridViewToFileAsJson(MainMenu_Form.Instance.Sale_DataGridView, MainMenu_Form.SelectedOption.Sales);
            }

            return wasSomethingImported;
        }
        private static DataGridViewRow? FindTransactionRow(string transactionId, bool isPurchase)
        {
            Guna2DataGridView targetDataGridView = isPurchase
                ? MainMenu_Form.Instance.Purchase_DataGridView
                : MainMenu_Form.Instance.Sale_DataGridView;

            foreach (DataGridViewRow row in targetDataGridView.Rows)
            {
                if (row.Cells[MainMenu_Form.Column.ID.ToString()].Value?.ToString() == transactionId)
                {
                    return row;
                }
            }

            return null;
        }
        private static bool TransactionHasReceipt(DataGridViewRow row)
        {
            if (row.Tag is (List<string> tagList, TagData))
            {
                return tagList[^1].StartsWith(ReadOnlyVariables.Receipt_text);
            }
            else if (row.Tag is (string tagString, TagData))
            {
                return !string.IsNullOrEmpty(tagString);
            }
            return false;
        }

        /// <summary>
        /// Helper method for importing purchase and sales data.
        /// </summary>
        private static (bool, bool) ImportTransactionData(IXLWorksheet worksheet, bool includeHeader, bool isPurchase)
        {
            IEnumerable<IXLRow> rowsToProcess = includeHeader
                ? worksheet.RowsUsed()
                : worksheet.RowsUsed().Skip(1);

            bool wasSomethingImported = false;
            int newRowIndex = -1;

            Guna2DataGridView targetGridView = isPurchase
                ? MainMenu_Form.Instance.Purchase_DataGridView
                : MainMenu_Form.Instance.Sale_DataGridView;

            // Get existing transaction numbers
            HashSet<string> existingTransactionNumbers = new(targetGridView.Rows.Count);
            string idColumn = MainMenu_Form.Column.ID.ToString();

            foreach (DataGridViewRow row in targetGridView.Rows)
            {
                if (row.Cells[idColumn].Value != null)
                {
                    existingTransactionNumbers.Add(row.Cells[idColumn].Value.ToString());
                }
            }

            HashSet<string> addedDuringImport = [];
            string itemType = isPurchase ? "Purchase" : "Sale";

            foreach (IXLRow row in rowsToProcess)
            {
                string transactionNumber = row.Cell(1).GetValue<string>();

                if (string.IsNullOrEmpty(transactionNumber)) { continue; }

                // Check if this row's transaction number already exists
                if (transactionNumber != ReadOnlyVariables.EmptyCell)
                {
                    bool shouldContinue = CheckIfItemExists(
                        transactionNumber,
                        existingTransactionNumbers,
                        addedDuringImport,
                        itemType);

                    if (!shouldContinue) { continue; }
                }

                // Create a new row
                DataGridViewRow newRow = (DataGridViewRow)targetGridView.RowTemplate.Clone();
                newRow.CreateCells(targetGridView);

                if (!ImportTransaction(row, newRow)) { return (false, wasSomethingImported); }

                ImportItemsInTransaction(row, newRow);

                // Add the row to the DataGridView
                targetGridView.InvokeIfRequired(() =>
                {
                    newRowIndex = targetGridView.Rows.Add(newRow);
                });

                FormatNoteCell(newRow);

                // Track that we've added this transaction number
                if (!string.IsNullOrEmpty(transactionNumber) && transactionNumber != ReadOnlyVariables.EmptyCell)
                {
                    addedDuringImport.Add(transactionNumber);
                }

                wasSomethingImported = true;
                DataGridViewManager.DataGridViewRowsAdded(targetGridView, new DataGridViewRowsAddedEventArgs(newRowIndex, 1));
            }

            // Update "Has Receipt" column for all imported rows
            if (wasSomethingImported)
            {
                MainMenu_Form.Instance.SetHasReceiptColumnVisibilty();
            }

            return (true, wasSomethingImported);
        }

        /// <summary>
        /// Checks if an item already exists and asks the user if they want to add it anyway
        /// </summary>
        /// <returns>True if the item should be added, false if it should be skipped</returns>
        private static bool CheckIfItemExists(
            string itemNumber,
            HashSet<string> existingItems,
            HashSet<string> addedDuringImport,
            string itemTypeName)
        {
            bool alreadyExistsInSystem = existingItems.Contains(itemNumber);
            bool alreadyAddedDuringImport = addedDuringImport.Contains(itemNumber);

            if (alreadyExistsInSystem)
            {
                CustomMessageBoxResult result = CustomMessageBox.Show(
                    $"{itemTypeName} # already exists",
                    $"The {itemTypeName.ToLowerInvariant()} #{itemNumber} already exists. Would you like to add this {itemTypeName.ToLowerInvariant()} anyways?",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                return result == CustomMessageBoxResult.Yes;
            }
            else if (alreadyAddedDuringImport)
            {
                CustomMessageBoxResult result = CustomMessageBox.Show(
                    $"Duplicate {itemTypeName} # in Spreadsheet",
                    $"The {itemTypeName.ToLowerInvariant()} #{itemNumber} appears multiple times in this spreadsheet. Would you like to add this duplicate anyways?",
                    CustomMessageBoxIcon.Question, CustomMessageBoxButtons.YesNo);

                return result == CustomMessageBoxResult.Yes;
            }

            return true;
        }

        /// <summary>
        /// This needs to be done after the row has been added to a DataGridView.
        /// </summary>
        private static void FormatNoteCell(DataGridViewRow row)
        {
            int noteCellIndex = Properties.Settings.Default.ShowHasReceiptColumn ? row.Cells.Count - 2 : row.Cells.Count - 1;
            DataGridViewCell lastCell = row.Cells[noteCellIndex];

            // Only add underline if the cell has a note
            if (lastCell.Value?.ToString() == ReadOnlyVariables.Show_text && lastCell.Tag != null)
            {
                DataGridViewManager.AddUnderlineToCell(lastCell);
            }
        }

        /// <summary>
        /// Imports data into a DataGridViewRow.
        /// </summary>
        /// <returns>True if the cells are imported successfully. False if the exchange rate was not retrieved.</returns>
        private static bool ImportTransaction(IXLRow row, DataGridViewRow newRow)
        {
            TagData tagData = new();

            // Get exchange rate
            string date = row.Cell(7).GetValue<string>();
            string currency = DataFileManager.GetValue(AppDataSettings.DefaultCurrencyType);
            decimal exchangeRateToDefault = Currency.GetExchangeRate("USD", currency, date, false);
            if (exchangeRateToDefault == -1) { return false; }

            int noteCellIndex = Properties.Settings.Default.ShowHasReceiptColumn ? newRow.Cells.Count - 2 : newRow.Cells.Count - 1;
            for (int i = 0; i < noteCellIndex; i++)
            {
                string value = row.Cell(i + 1).GetValue<string>();

                if (i >= 8 && i <= 14)
                {
                    decimal decimalValue = ConvertStringToDecimal(value);
                    bool useEmpty = false;

                    switch (i)
                    {
                        case 8:
                            if (value == ReadOnlyVariables.EmptyCell)
                            {
                                useEmpty = true;
                            }
                            else
                            {
                                tagData.PricePerUnitUSD = decimalValue;
                            }
                            break;
                        case 9:
                            tagData.ShippingUSD = decimalValue;
                            break;
                        case 10:
                            tagData.TaxUSD = decimalValue;
                            break;
                        case 11:
                            tagData.FeeUSD = decimalValue;
                            break;
                        case 12:
                            tagData.DiscountUSD = decimalValue;
                            break;
                        case 13:
                            tagData.ChargedDifferenceUSD = decimalValue;
                            break;
                        case 14:
                            tagData.ChargedOrCreditedUSD = decimalValue;
                            break;
                    }

                    newRow.Cells[i].Value = useEmpty
                        ? ReadOnlyVariables.EmptyCell
                        : (decimalValue * exchangeRateToDefault);
                }
                else
                {
                    newRow.Cells[i].Value = value;
                }
            }

            // Set the note
            DataGridViewCell noteCell = newRow.Cells[noteCellIndex];
            IXLCell excelNoteCell = row.Cell(16);
            string excelNoteCellValue = excelNoteCell.GetValue<string>();

            if (string.IsNullOrWhiteSpace(excelNoteCellValue) || excelNoteCellValue == ReadOnlyVariables.EmptyCell)
            {
                noteCell.Value = ReadOnlyVariables.EmptyCell;
            }
            else
            {
                noteCell.Value = ReadOnlyVariables.Show_text;
                noteCell.Tag = excelNoteCellValue;
            }

            newRow.Tag = tagData;
            return true;
        }
        private static void ImportItemsInTransaction(IXLRow row, DataGridViewRow transaction)
        {
            TagData tagData = (TagData)transaction.Tag;
            List<string> items = [];

            while (true)
            {
                IXLRow nextRow = row.RowBelow();

                // Check if the row has any data
                if (nextRow.IsEmpty())
                {
                    break;
                }

                // Get data from every item in the transaction
                string number = nextRow.Cell(1).GetValue<string>();

                // Check if the next row has no number, indicating multiple items
                if (string.IsNullOrEmpty(number))
                {
                    string productName = nextRow.Cell(3).Value.ToString();
                    string categoryName = nextRow.Cell(4).Value.ToString();
                    string currentCountry = nextRow.Cell(5).Value.ToString();
                    string currentCompany = nextRow.Cell(6).Value.ToString();
                    decimal quantity = ConvertStringToDecimal(nextRow.Cell(8).Value.ToString());
                    decimal pricePerUnit = ConvertStringToDecimal(nextRow.Cell(9).Value.ToString());

                    string item = string.Join(",",
                        productName,
                        categoryName,
                        currentCountry,
                        currentCompany,
                        quantity.ToString(),
                        pricePerUnit.ToString("N2"),
                        (quantity * pricePerUnit).ToString("N2")
                    );

                    items.Add(item);
                    row = nextRow;  // Move to the next row
                }
                else { break; }
            }

            // Save
            if (items.Count > 0)
            {
                transaction.Tag = (items, tagData);
            }
        }

        // Export spreadsheet methods
        public static void ExportSpreadsheet(string filePath)
        {
            filePath = Directories.GetNewFileNameIfItAlreadyExists(filePath);

            using XLWorkbook workbook = new();

            IXLWorksheet purchaseWorksheet = workbook.Worksheets.Add("Purchases");
            AddTransactionToWorksheet(purchaseWorksheet, MainMenu_Form.Instance.Purchase_DataGridView);

            IXLWorksheet salesWorksheet = workbook.Worksheets.Add("Sales");
            AddTransactionToWorksheet(salesWorksheet, MainMenu_Form.Instance.Sale_DataGridView);

            IXLWorksheet purchaseProductsWorksheet = workbook.Worksheets.Add("Purchase products");
            AddProductsToWorksheet(purchaseProductsWorksheet, MainMenu_Form.Instance.CategoryPurchaseList);

            IXLWorksheet saleProductsWorksheet = workbook.Worksheets.Add("Sale products");
            AddProductsToWorksheet(saleProductsWorksheet, MainMenu_Form.Instance.CategorySaleList);

            IXLWorksheet companiesWorksheet = workbook.Worksheets.Add("Companies");
            AddCompaniesToWorksheet(companiesWorksheet);

            IXLWorksheet accountantsWorksheet = workbook.Worksheets.Add("Accountants");
            AddAccountantsToWorksheet(accountantsWorksheet);

            // Save the file
            workbook.SaveAs(filePath);
        }
        private static void AddTransactionToWorksheet(IXLWorksheet worksheet, DataGridView dataGridView)
        {
            // Add headers and format them
            int excelColumnIndex = 1;
            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                // Skip the "Has Receipt" column
                if (dataGridView.Columns[i].Name == MainMenu_Form.Column.HasReceipt.ToString())
                {
                    continue;
                }

                IXLCell cell = worksheet.Cell(1, excelColumnIndex);
                cell.Value = dataGridView.Columns[i].HeaderText;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                excelColumnIndex++;
            }

            int receiptCellIndex = excelColumnIndex;

            // Add header for the receipt column
            worksheet.Cell(1, receiptCellIndex).Value = "Receipt";
            worksheet.Cell(1, receiptCellIndex).Style.Font.Bold = true;
            worksheet.Cell(1, receiptCellIndex).Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Add message
            int messageCellIndex = receiptCellIndex + 2;
            worksheet.Cell(1, messageCellIndex).Value = "All prices in USD";
            worksheet.Cell(1, messageCellIndex).Style.Font.Bold = true;

            // Extract TagData and receipt information
            string receiptFileName = ReadOnlyVariables.EmptyCell;

            int currentRow = 2;
            int rowForReceipt = 2;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                // Add transactions
                switch (row.Tag)
                {
                    case (List<string> itemList, TagData tagData) when itemList.Count > 0:
                        // Is there a receipt
                        byte receiptOffset = 0;
                        string receipt = itemList[^1];
                        if (receipt.StartsWith(ReadOnlyVariables.Receipt_text))
                        {
                            receiptOffset = 1;
                            receiptFileName = Path.GetFileName(receipt);
                        }
                        else
                        {
                            worksheet.Cell(currentRow, receiptCellIndex).Value = ReadOnlyVariables.EmptyCell;
                        }

                        AddRowToWorksheet(worksheet, row, currentRow, tagData);

                        // Add items in transaction if they exist in itemList
                        for (int i = 0; i < itemList.Count - receiptOffset; i++)
                        {
                            currentRow++;
                            string[] values = itemList[i].Split(',');

                            AddItemRowToWorksheet(worksheet, values, currentRow);
                        }
                        break;

                    case (string tagString, TagData tagData):
                        AddRowToWorksheet(worksheet, row, currentRow, tagData);
                        receiptFileName = Path.GetFileName(tagString);
                        break;
                }

                // Add receipt to the last cell
                worksheet.Cell(rowForReceipt, receiptCellIndex).Value = receiptFileName;

                currentRow++;
                rowForReceipt = currentRow;  // This ensures that the receipt is not added to the bottom of a transaction with multiple items
            }

            worksheet.Columns().AdjustToContents();
        }
        private static void AddRowToWorksheet(IXLWorksheet worksheet, DataGridViewRow row, int currentRow, TagData tagData)
        {
            int excelColumnIndex = 1;

            for (int i = 0; i < row.Cells.Count; i++)
            {
                // Skip the Notes column - it will be handled separately
                int notesColumnIndex = Properties.Settings.Default.ShowHasReceiptColumn
                    ? row.Cells.Count - 2
                    : row.Cells.Count - 1;

                if (i == notesColumnIndex)
                {
                    break;
                }

                IXLCell excelCell = worksheet.Cell(currentRow, excelColumnIndex);

                // Handle ID column (first column) as number
                if (i == 0)
                {
                    object? cellValue = row.Cells[i].Value;
                    if (cellValue != null && int.TryParse(cellValue.ToString(), out int idValue))
                    {
                        excelCell.Value = idValue;
                        excelCell.Style.NumberFormat.Format = _numberFormatPattern;
                    }
                    else
                    {
                        excelCell.Value = cellValue?.ToString();
                    }
                }
                else if (tagData != null && i >= 8 && i <= 14)
                {
                    (decimal usdValue, bool useEmpty) = i switch
                    {
                        8 => (tagData.PricePerUnitUSD, tagData.PricePerUnitUSD == 0),
                        9 => (tagData.ShippingUSD, false),
                        10 => (tagData.TaxUSD, false),
                        11 => (tagData.FeeUSD, false),
                        12 => (tagData.DiscountUSD, false),
                        13 => (tagData.ChargedDifferenceUSD, false),
                        14 => (tagData.ChargedOrCreditedUSD, false),
                        _ => (0, false)
                    };

                    if (useEmpty)
                    {
                        excelCell.Value = ReadOnlyVariables.EmptyCell;
                    }
                    else
                    {
                        // Set as numeric value with currency formatting
                        excelCell.Value = usdValue;
                        excelCell.Style.NumberFormat.Format = _currencyFormatPattern;
                    }
                }
                else
                {
                    // Handle other cell types
                    object? cellValue = row.Cells[i].Value;

                    // Try to parse as decimal for numeric columns
                    if (cellValue != null && IsNumericColumn(i) &&
                        decimal.TryParse(cellValue.ToString(), out decimal numericValue))
                    {
                        excelCell.Value = numericValue;
                        // Use currency format for money columns, decimal for others
                        if (IsCurrencyColumn(i))
                        {
                            excelCell.Style.NumberFormat.Format = _currencyFormatPattern;
                        }
                        else
                        {
                            excelCell.Style.NumberFormat.Format = _decimalFormatPattern;
                        }
                    }
                    else
                    {
                        excelCell.Value = cellValue?.ToString();
                    }
                }

                excelColumnIndex++;
            }

            // Handle the Notes column
            int notesCellIndex = Properties.Settings.Default.ShowHasReceiptColumn ? row.Cells.Count - 2 : row.Cells.Count - 1;
            DataGridViewCell notesCell = row.Cells[notesCellIndex];
            string? notesCellValue = notesCell.Value?.ToString();
            IXLCell notesExcelCell = worksheet.Cell(currentRow, excelColumnIndex);

            notesExcelCell.Value = notesCellValue == ReadOnlyVariables.EmptyCell
                ? ReadOnlyVariables.EmptyCell
                : (notesCellValue == ReadOnlyVariables.Show_text && notesCell.Tag != null)
                    ? notesCell.Tag.ToString()
                    : notesCellValue;
        }
        private static void AddItemRowToWorksheet(IXLWorksheet worksheet, string[] row, int currentRow)
        {
            for (int i = 0; i < row.Length - 1; i++)  // Skip the total value with - 1
            {
                // Shift the data one column to the right after the date column
                int columnIndex = i < 4 ? i : i + 1;
                IXLCell excelCell = worksheet.Cell(currentRow, columnIndex + 3);

                string cellValue = row[i];

                // Check if this should be a numeric value (quantity, price, etc.)
                if (i == 4 || i == 5) // quantity and price columns
                {
                    if (decimal.TryParse(cellValue, out decimal numericValue))
                    {
                        excelCell.Value = numericValue;
                        // Format price as currency, quantity as decimal
                        if (i == 5) // price column
                        {
                            excelCell.Style.NumberFormat.Format = _currencyFormatPattern;
                        }
                        else // quantity column
                        {
                            excelCell.Style.NumberFormat.Format = _decimalFormatPattern;
                        }
                    }
                    else
                    {
                        excelCell.Value = cellValue;
                    }
                }
                else
                {
                    excelCell.Value = cellValue;
                }
            }
        }
        private static bool IsCurrencyColumn(int columnIndex)
        {
            // Define which columns contain currency data
            int[] currencyColumns = [8, 9, 10, 11, 12, 13, 14]; // USD value columns
            return currencyColumns.Contains(columnIndex);
        }
        private static bool IsNumericColumn(int columnIndex)
        {
            // Define which columns contain numeric data (both currency and non-currency)
            int[] numericColumns = [7, 8, 9, 10, 11, 12, 13, 14];
            return numericColumns.Contains(columnIndex);
        }
        private static void AddCompaniesToWorksheet(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "Company name";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            int currentRow = 2;
            foreach (string company in MainMenu_Form.Instance.CompanyList)
            {
                worksheet.Cell(currentRow, 1).Value = company;
                currentRow++;
            }

            worksheet.Columns().AdjustToContents();
        }
        private static void AddAccountantsToWorksheet(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "Accountant name";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            int currentRow = 2;
            foreach (string accountant in MainMenu_Form.Instance.AccountantList)
            {
                worksheet.Cell(currentRow, 1).Value = accountant;
                currentRow++;
            }

            worksheet.Columns().AdjustToContents();
        }
        private static void AddProductsToWorksheet(IXLWorksheet worksheet, List<Category> list)
        {
            worksheet.Cell(1, 1).Value = "Product ID";
            worksheet.Cell(1, 2).Value = "Product name";
            worksheet.Cell(1, 3).Value = "Category";
            worksheet.Cell(1, 4).Value = "Country of origin";
            worksheet.Cell(1, 5).Value = "Company of origin";

            // Format title cells
            for (int i = 1; i <= 5; i++)
            {
                worksheet.Cell(1, i).Style.Font.Bold = true;
                worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.LightBlue;
            }

            int currentRow = 2;
            foreach (Category category in list)
            {
                foreach (Product product in category.ProductList)
                {
                    worksheet.Cell(currentRow, 1).Value = product.ProductID;
                    worksheet.Cell(currentRow, 2).Value = product.Name;
                    worksheet.Cell(currentRow, 3).Value = category.Name;
                    worksheet.Cell(currentRow, 4).Value = product.CountryOfOrigin;
                    worksheet.Cell(currentRow, 5).Value = product.CompanyOfOrigin;
                    currentRow++;
                }
            }

            worksheet.Columns().AdjustToContents();
        }

        // Export charts to Microsoft Excel
        public static void ExportChartToExcel(Dictionary<string, double> data, string filePath, eChartType chartType, string chartTitle, string column1Text, string column2Text, bool isSpline = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ExcelPackage.License.SetNonCommercialPersonal("Argo");
            using ExcelPackage package = new();
            string worksheetName = LanguageManager.TranslateString("Chart Data");
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(worksheetName);

            // Add headers
            worksheet.Cells["A1"].Value = LanguageManager.TranslateString(column1Text);
            worksheet.Cells["B1"].Value = LanguageManager.TranslateString(column2Text);

            // Format headers
            ExcelRange headerRange = worksheet.Cells["A1:B1"];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);

            // Add data
            int row = 2;
            foreach (KeyValuePair<string, double> item in data.OrderBy(x => x.Key))
            {
                worksheet.Cells[row, 1].Value = item.Key;
                worksheet.Cells[row, 2].Value = item.Value;
                worksheet.Cells[row, 2].Style.Numberformat.Format = _currencyFormatPattern;
                row++;
            }

            ExcelChart chart = CreateChart(worksheet, chartTitle, chartType, false);

            // Configure chart
            ExcelChartSerie series = chart.Series.Add(worksheet.Cells[$"B2:B{row - 1}"], worksheet.Cells[$"A2:A{row - 1}"]);
            if (isSpline && chartType == eChartType.Line)
            {
                ((ExcelLineChartSerie)series).Smooth = true;
            }
            chart.Legend.Remove();

            worksheet.Columns[1, 2].AutoFit();
            package.SaveAs(new FileInfo(filePath));

            TrackChartExport(stopwatch, filePath, isSpline ? ExportType.GoogleSheetsChart : ExportType.ExcelSheetsChart);
        }
        public static void ExportMultiDataSetChartToExcel(Dictionary<string, Dictionary<string, double>> data, string filePath, eChartType chartType, string chartTitle, bool isSpline = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ExcelPackage.License.SetNonCommercialPersonal("Argo");
            using ExcelPackage package = new();
            string worksheetName = LanguageManager.TranslateString("Chart Data");
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(worksheetName);

            // Get all series names
            List<string> seriesNames = data.First().Value.Keys.ToList();

            // Add headers
            worksheet.Cells[1, 1].Value = LanguageManager.TranslateString("Date");
            for (int i = 0; i < seriesNames.Count; i++)
            {
                worksheet.Cells[1, i + 2].Value = seriesNames[i];
            }

            // Format headers
            ExcelRange headerRange = worksheet.Cells[1, 1, 1, seriesNames.Count + 1];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightSkyBlue);

            // Add data
            int row = 2;
            foreach (KeyValuePair<string, Dictionary<string, double>> dateEntry in data.OrderBy(x => x.Key))
            {
                worksheet.Cells[row, 1].Value = dateEntry.Key;

                for (int i = 0; i < seriesNames.Count; i++)
                {
                    worksheet.Cells[row, i + 2].Value = dateEntry.Value[seriesNames[i]];
                    // Use currency formatting for money values
                    worksheet.Cells[row, i + 2].Style.Numberformat.Format = _currencyFormatPattern;
                }
                row++;
            }

            // Create chart
            ExcelChart chart = CreateChart(worksheet, chartTitle, chartType, true);
            chart.Legend.Position = eLegendPosition.Top;

            // Add series to chart
            for (int i = 0; i < seriesNames.Count; i++)
            {
                ExcelChartSerie series = chart.Series.Add(
                    worksheet.Cells[2, i + 2, row - 1, i + 2], // Y values
                    worksheet.Cells[2, 1, row - 1, 1]          // X values
                );
                if (isSpline && chartType == eChartType.Line)
                {
                    ((ExcelLineChartSerie)series).Smooth = true;
                }
                series.Header = seriesNames[i];
            }

            worksheet.Columns.AutoFit();
            package.SaveAs(new FileInfo(filePath));

            TrackChartExport(stopwatch, filePath, isSpline ? ExportType.GoogleSheetsChart : ExportType.ExcelSheetsChart);
        }
        private static void TrackChartExport(Stopwatch stopwatch, string filePath, ExportType exportType)
        {
            stopwatch.Stop();
            string readableSize = "0 Bytes";

            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new(filePath);
                long fileSizeBytes = fileInfo.Length;
                readableSize = Tools.ConvertBytesToReadableSize(fileSizeBytes);
            }

            Dictionary<ExportDataField, object> exportData = new()
            {
                { ExportDataField.ExportType, exportType },
                { ExportDataField.DurationMS, stopwatch.ElapsedMilliseconds },
                { ExportDataField.FileSize, readableSize }
            };

            AnonymousDataManager.AddExportData(exportData);
        }

        /// <summary>
        /// Creates and configures an Excel chart with default position and size.
        /// </summary>
        /// <returns>The created Excel chart.</returns>
        public static ExcelChart CreateChart(ExcelWorksheet worksheet, string chartTitle, eChartType chartType, bool isMultiDataset)
        {
            ExcelChart chart = worksheet.Drawings.AddChart(chartTitle, chartType);
            chart.SetPosition(0, 0, isMultiDataset ? 4 : 3, 0);
            chart.SetSize(800, 400);
            chart.Title.Text = chartTitle;

            return chart;
        }

        // Other methods
        public static decimal ConvertStringToDecimal(string value)
        {
            if (value == ReadOnlyVariables.EmptyCell) { return 0; }

            try
            {
                return Convert.ToDecimal(value);
            }
            catch
            {
                CustomMessageBox.Show(
                    "Cannot import",
                    $"Cannot import because a money value is not in the correct format: {value}",
                    CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);
                return -1;
            }
        }
    }
}