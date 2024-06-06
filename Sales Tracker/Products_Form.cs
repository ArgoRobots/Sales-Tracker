namespace Sales_Tracker
{
    public partial class Products_Form : Form
    {
        public readonly static List<string> thingsThatHaveChangedInFile = [];
        // Init
        public static Products_Form Instance { get; set; }
        public Products_Form()
        {
            InitializeComponent();
            Instance = this;
        }
    }
}