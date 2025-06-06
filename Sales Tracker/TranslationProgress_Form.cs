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
            ProgressStats_Label.Text = $"{completedTranslations:N0} / {totalPossibleTranslations:N0} translations completed";
        }
        public void CompleteProgress()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(CompleteProgress));
                return;
            }

            ProgressBar.Value = 100;
            Operation_Label.Text = "Translation generation complete!";
            Cancel_Button.Text = "Close";

            // Auto-close after 2 seconds
            Timer closeTimer = new()
            {
                Interval = 2000
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