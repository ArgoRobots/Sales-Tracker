using Sales_Tracker.Theme;
using Sales_Tracker.UI;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker
{
    public partial class TranslationProgress_Form : Form
    {
        // Properties
        private readonly CancellationTokenSource cancellationTokenSource;
        private int totalLanguages;
        private int totalTranslationsPerLanguage;
        private int currentLanguageIndex;
        private int currentTranslationIndex;
        private bool operationCancelled = false;

        // Time estimation properties
        private DateTime operationStartTime;
        private readonly Queue<DateTime> progressTimestamps = new();
        private readonly Queue<int> progressValues = new();
        private const int MaxSampleSize = 10; // Number of samples for moving average

        // Getters
        public bool IsCancelled => operationCancelled;
        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        // Init.
        public TranslationProgress_Form()
        {
            InitializeComponent();

            ThemeManager.SetThemeForForm(this);
            ThemeManager.MakeGButtonBlueSecondary(Cancel_Button);

            cancellationTokenSource = new CancellationTokenSource();
            operationStartTime = DateTime.Now;

            LoadingPanel.ShowBlankLoadingPanel(this);
        }

        // Form event handlers
        private void TranslationProgress_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void TranslationProgress_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!operationCancelled && Cancel_Button.Text != "Close")
            {
                operationCancelled = true;
                cancellationTokenSource.Cancel();
            }
        }

        // Methods
        public void Initialize(int totalLanguageCount, int translationsPerLanguage)
        {
            totalLanguages = totalLanguageCount;
            totalTranslationsPerLanguage = translationsPerLanguage;
            currentLanguageIndex = 0;
            currentTranslationIndex = 0;
            operationStartTime = DateTime.Now;

            // Clear previous samples
            progressTimestamps.Clear();
            progressValues.Clear();

            UpdateProgressDisplay();
        }
        public void UpdateLanguage(string languageName, int languageIndex)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateLanguage(languageName, languageIndex)));
                return;
            }

            currentLanguageIndex = languageIndex;
            currentTranslationIndex = 0;

            CurrentLanguage_Label.Text = $"Processing: {languageName} ({languageIndex + 1} of {totalLanguages})";
            Operation_Label.Text = "Collecting source texts...";

            UpdateProgressDisplay();
        }
        public void UpdateTranslationProgress(int translationIndex, string operation = null)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateTranslationProgress(translationIndex, operation)));
                return;
            }

            currentTranslationIndex = translationIndex;

            if (!string.IsNullOrEmpty(operation))
                Operation_Label.Text = operation;

            UpdateProgressDisplay();
        }
        public void UpdateBatchProgress(int batchIndex, int totalBatches, int textsInBatch)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateBatchProgress(batchIndex, totalBatches, textsInBatch)));
                return;
            }

            Operation_Label.Text = $"Translating batch {batchIndex + 1} of {totalBatches} ({textsInBatch} texts)";

            // Calculate progress within current language
            int progressWithinLanguage = (int)((double)batchIndex / totalBatches * totalTranslationsPerLanguage);
            currentTranslationIndex = progressWithinLanguage;

            UpdateProgressDisplay();
        }
        private void UpdateProgressDisplay()
        {
            // Calculate overall progress
            int totalPossibleTranslations = totalLanguages * totalTranslationsPerLanguage;
            int completedTranslations = currentLanguageIndex * totalTranslationsPerLanguage + currentTranslationIndex;

            int progressPercentage = totalPossibleTranslations > 0
                ? (int)((double)completedTranslations / totalPossibleTranslations * 100)
                : 0;

            ProgressBar.Value = Math.Min(progressPercentage, 100);

            // Update time estimation
            string timeEstimate = CalculateTimeRemaining(completedTranslations, totalPossibleTranslations);

            ProgressStats_Label.Text = $"{completedTranslations:N0} / {totalPossibleTranslations:N0} translations completed{timeEstimate}";
        }
        private string CalculateTimeRemaining(int completedTranslations, int totalTranslations)
        {
            if (completedTranslations == 0 || totalTranslations == 0)
                return "";

            DateTime now = DateTime.Now;

            // Add current progress sample
            progressTimestamps.Enqueue(now);
            progressValues.Enqueue(completedTranslations);

            // Remove old samples to maintain moving window
            while (progressTimestamps.Count > MaxSampleSize)
            {
                progressTimestamps.Dequeue();
                progressValues.Dequeue();
            }

            // Need at least 2 samples to calculate rate
            if (progressTimestamps.Count < 2)
                return "";

            try
            {
                // Calculate average rate using linear regression for better accuracy
                double rate = CalculateProgressRate();

                if (rate <= 0)
                    return " - Calculating time remaining...";

                int remainingTranslations = totalTranslations - completedTranslations;
                double estimatedSecondsRemaining = remainingTranslations / rate;

                // Format time remaining
                TimeSpan timeRemaining = TimeSpan.FromSeconds(estimatedSecondsRemaining);

                string formattedTime;
                if (timeRemaining.TotalHours >= 1)
                {
                    formattedTime = $"{(int)timeRemaining.TotalHours}h {timeRemaining.Minutes}m";
                }
                else if (timeRemaining.TotalMinutes >= 1)
                {
                    formattedTime = $"{(int)timeRemaining.TotalMinutes}m {timeRemaining.Seconds}s";
                }
                else
                {
                    formattedTime = $"{(int)timeRemaining.TotalSeconds}s";
                }

                return $" - {formattedTime} remaining";
            }
            catch
            {
                return " - Calculating time remaining...";
            }
        }
        private double CalculateProgressRate()
        {
            if (progressTimestamps.Count < 2)
                return 0;

            // Use simple rate calculation for more stability
            DateTime firstTime = progressTimestamps.First();
            DateTime lastTime = progressTimestamps.Last();
            int firstProgress = progressValues.First();
            int lastProgress = progressValues.Last();

            double timeSpanSeconds = (lastTime - firstTime).TotalSeconds;
            if (timeSpanSeconds <= 0)
                return 0;

            int progressDifference = lastProgress - firstProgress;
            return progressDifference / timeSpanSeconds; // translations per second
        }
        public void CompleteProgress()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(CompleteProgress));
                return;
            }

            ProgressBar.Value = 100;

            // Calculate total time taken
            TimeSpan totalTime = DateTime.Now - operationStartTime;
            string totalTimeFormatted;

            if (totalTime.TotalHours >= 1)
            {
                totalTimeFormatted = $"{(int)totalTime.TotalHours}h {totalTime.Minutes}m {totalTime.Seconds}s";
            }
            else if (totalTime.TotalMinutes >= 1)
            {
                totalTimeFormatted = $"{(int)totalTime.TotalMinutes}m {totalTime.Seconds}s";
            }
            else
            {
                totalTimeFormatted = $"{(int)totalTime.TotalSeconds}s";
            }

            Operation_Label.Text = $"Translation generation complete! (Total time: {totalTimeFormatted})";
            Cancel_Button.Text = "Close";

            // Auto-close after 3 seconds
            Timer closeTimer = new()
            {
                Interval = 3000
            };
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                DialogResult = DialogResult.OK;
                Close();
            };
            closeTimer.Start();
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (Cancel_Button.Text == "Close")
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            operationCancelled = true;
            cancellationTokenSource.Cancel();

            Cancel_Button.Enabled = false;
            Cancel_Button.Text = "Cancelling...";
            Operation_Label.Text = "Cancelling operation...";
        }
    }
}