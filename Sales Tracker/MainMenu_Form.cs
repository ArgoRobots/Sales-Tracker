namespace Sales_Tracker
{
    public partial class MainMenu_Form : Form
    {
        // Init
        public static Log_Form Instance { get; set; }
        public MainMenu_Form()
        {
            InitializeComponent();
            Instance = this;
        }
    }
}