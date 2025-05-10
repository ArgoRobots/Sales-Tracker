using Sales_Tracker.Classes;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class Loading_Form : Form
    {
        // Static members to track form instance and operations
        private static Loading_Form _instance;
        private static int _activeOperations = 0;
        private static readonly List<string> _activeMessages = [];

        // Get the current instance or create a new one
        public static Loading_Form Instance => _instance ??= new Loading_Form("Loading...");

        // Init for first creation
        private Loading_Form(string message)
        {
            InitializeComponent();
            Theme.SetThemeForForm(this);
            LoadingPanel.ShowLoadingScreen(this, message);
        }

        // Form event handlers
        private void Loading_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Reset when form is closed
            LoadingPanel.HideLoadingScreen(this);
            _activeOperations = 0;
            _activeMessages.Clear();
            _instance = null;
        }

        // Static methods to manage operations

        /// <summary>
        /// Shows the loading form or updates the existing one with a new operation
        /// </summary>
        public static void ShowLoading(string message)
        {
            // Create form if needed or get existing
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new Loading_Form(message);
                _instance.Show();
            }

            // Add to operations count
            _activeOperations++;
            _activeMessages.Add(message);

            // Update the message to show how many operations are running
            UpdateLoadingMessage();
        }

        /// <summary>
        /// Completes an operation and updates or closes the form
        /// </summary>
        public static void CompleteOperation(string message = null)
        {
            if (_instance == null || _instance.IsDisposed)
                return;

            // Decrement operation count
            _activeOperations = Math.Max(0, _activeOperations - 1);

            // Remove the message if specified
            if (message != null && _activeMessages.Contains(message))
            {
                _activeMessages.Remove(message);
            }
            else if (_activeMessages.Count > 0)
            {
                // Remove the oldest message if none specified
                _activeMessages.RemoveAt(0);
            }

            // Update or close the form
            if (_activeOperations <= 0)
            {
                _instance.Close();
                _instance = null;
            }
            else
            {
                UpdateLoadingMessage();
            }
        }

        /// <summary>
        /// Updates the loading message on the form
        /// </summary>
        private static void UpdateLoadingMessage()
        {
            if (_instance == null || _instance.IsDisposed)
                return;

            // Use UI thread to update
            _instance.Invoke(new Action(() =>
            {
                string baseMessage = _activeMessages.Count > 0 ? _activeMessages[0] : "Loading...";
                string displayMessage = baseMessage;

                if (_activeOperations > 1)
                {
                    displayMessage = $"{baseMessage} ({_activeOperations} operations in progress)";
                }

                // Update the loading panel message
                LoadingPanel.HideLoadingScreen(_instance);
                LoadingPanel.ShowLoadingScreen(_instance, displayMessage);
            }));
        }
    }
}