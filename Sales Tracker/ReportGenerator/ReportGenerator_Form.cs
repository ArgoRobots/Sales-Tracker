using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Language;
using Sales_Tracker.Properties;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker.ReportGenerator
{
    /// <summary>
    /// Main container form for the Data Report Generation System.
    /// Manages navigation between child forms and coordinates the report creation process.
    /// </summary>
    public partial class ReportGenerator_Form : BaseForm
    {
        // Properties
        private static ReportGenerator_Form _instance;
        private ReportDataSelection_Form _dataSelectionForm;
        private ReportLayoutDesigner_Form _layoutDesignerForm;
        private ReportPreviewExport_Form _previewExportForm;
        private int _currentStep = 0;
        private const byte _totalSteps = 3;

        // Getters
        public static ReportGenerator_Form Instance => _instance;
        public ReportConfiguration CurrentReportConfiguration { get; private set; }

        public enum ReportStep
        {
            DataSelection = 0,
            LayoutDesigner = 1,
            PreviewExport = 2
        }

        // Init.
        public ReportGenerator_Form()
        {
            InitializeComponent();
            _instance = this;

            CurrentReportConfiguration = new ReportConfiguration();
            InitializeChildForms();

            SetCurrentStep(ReportStep.DataSelection);
            UpdateNavigationButtons();
            UpdateProgressIndicator();

            UpdateTheme();
            LanguageManager.UpdateLanguageForControl(this);
            SetAccessibleDescriptions();
            AnimateButtons();

            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void InitializeChildForms()
        {
            _dataSelectionForm = new(this);
            _layoutDesignerForm = new(this);
            _previewExportForm = new(this);

            SetupChildForm(_dataSelectionForm);
            SetupChildForm(_layoutDesignerForm);
            SetupChildForm(_previewExportForm);
        }
        private void SetupChildForm(Form childForm)
        {
            if (childForm == null) { return; }

            childForm.TopLevel = false;
            childForm.Dock = DockStyle.Fill;
            childForm.Visible = false;

            // Add to main content panel
            Content_Panel.Controls.Add(childForm);
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);

            ReportDataSelection_Form.Instance.IncludeReturns_CheckBox.UncheckedState.FillColor = CustomColors.MainBackground;
            ReportDataSelection_Form.Instance.IncludeLosses_CheckBox.UncheckedState.FillColor = CustomColors.MainBackground;

            ReportLayoutDesigner_Form.Instance.ToolsContainer_Panel.BackColor = CustomColors.ControlBack;
            ReportLayoutDesigner_Form.Instance.PropertiesContainer_Panel.BackColor = CustomColors.ControlBack;

            ReportPreviewExport_Form.Instance.IncludeHeader_CheckBox.UncheckedState.FillColor = CustomColors.MainBackground;
            ReportPreviewExport_Form.Instance.IncludeFooter_CheckBox.UncheckedState.FillColor = CustomColors.MainBackground;
            ReportPreviewExport_Form.Instance.OpenAfterExport_CheckBox.UncheckedState.FillColor = CustomColors.MainBackground;

            ThemeManager.MakeGButtonBluePrimary(Previous_Button);
            ThemeManager.MakeGButtonBluePrimary(Next_Button);
            ThemeManager.MakeGButtonBlueSecondary(Cancel_Button);

            ReportLayoutDesigner_Form form = ReportLayoutDesigner_Form.Instance;

            if (ThemeManager.IsDarkTheme())
            {
                form.AlignLeft_Button.Image = Resources.ALignLeftWhite;
                form.AlignCenter_Button.Image = Resources.AlignCenterWhite;
                form.AlignRight_Button.Image = Resources.AlignRightWhite;

                form.AlignTop_Button.Image = Resources.AlignTopWhite;
                form.AlignMiddle_Button.Image = Resources.AlignMiddleWhite;
                form.AlignBottom_Button.Image = Resources.AlignBottomWhite;

                form.DistributeHorizontally_Button.Image = Resources.DistributeHorizontallyWhite;
                form.DistributeVertically_Button.Image = Resources.DistributeVerticallyWhite;

                form.MakeSameWidth_Button.Image = Resources.SameWidthWhite;
                form.MakeSameHeight_Button.Image = Resources.SameHeightWhite;
                form.MakeSameSize_Button.Image = Resources.SameSizeWhite;
            }
            else
            {
                form.AlignLeft_Button.Image = Resources.ALignLeftBlack;
                form.AlignCenter_Button.Image = Resources.AlignCenterBlack;
                form.AlignRight_Button.Image = Resources.AlignRightBlack;

                form.AlignTop_Button.Image = Resources.AlignTopBlack;
                form.AlignMiddle_Button.Image = Resources.AlignMiddleBlack;
                form.AlignBottom_Button.Image = Resources.AlignBottomBlack;

                form.DistributeHorizontally_Button.Image = Resources.DistributeHorizontallyBlack;
                form.DistributeVertically_Button.Image = Resources.DistributeVerticallyBlack;

                form.MakeSameWidth_Button.Image = Resources.SameWidthBlack;
                form.MakeSameHeight_Button.Image = Resources.SameHeightBlack;
                form.MakeSameSize_Button.Image = Resources.SameSizeBlack;
            }
        }
        private void SetAccessibleDescriptions()
        {
            StepTitle_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            ProgressValue_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
        }
        public void AnimateButtons()
        {
            CustomControls.AnimateButtons([Previous_Button, Next_Button, Cancel_Button]);
        }

        // Navigation methods
        public void SetCurrentStep(ReportStep step)
        {
            _currentStep = (int)step;

            HideAllChildForms();

            // Show the appropriate child form
            switch (step)
            {
                case ReportStep.DataSelection:
                    ShowChildForm(_dataSelectionForm);
                    StepTitle_Label.Text = LanguageManager.TranslateString("Data Selection");
                    break;

                case ReportStep.LayoutDesigner:
                    ShowChildForm(_layoutDesignerForm);
                    StepTitle_Label.Text = LanguageManager.TranslateString("Layout Designer");
                    break;

                case ReportStep.PreviewExport:
                    ShowChildForm(_previewExportForm);
                    StepTitle_Label.Text = LanguageManager.TranslateString("Preview && Export");
                    break;
            }

            UpdateNavigationButtons();
            UpdateProgressIndicator();
        }
        private void HideAllChildForms()
        {
            _dataSelectionForm?.Hide();
            _layoutDesignerForm?.Hide();
            _previewExportForm?.Hide();
        }
        private static void ShowChildForm(Form childForm)
        {
            childForm?.Show();
            childForm?.BringToFront();
        }
        private void UpdateNavigationButtons()
        {
            Previous_Button.Enabled = _currentStep > 0;

            bool canProceed = CanProceedToNextStep();
            Next_Button.Enabled = _currentStep < _totalSteps - 1 && canProceed;

            if (_currentStep == _totalSteps - 1)
            {
                Next_Button.Text = LanguageManager.TranslateString("Export");
                Next_Button.Enabled = canProceed;
            }
            else
            {
                Next_Button.Text = LanguageManager.TranslateString("Next");
            }
        }
        private bool CanProceedToNextStep()
        {
            return (ReportStep)_currentStep switch
            {
                ReportStep.DataSelection => _dataSelectionForm?.IsValidForNextStep() ?? false,
                ReportStep.LayoutDesigner => _layoutDesignerForm?.IsValidForNextStep() ?? false,
                ReportStep.PreviewExport => true,  // Always can finish from preview
                _ => false,
            };
        }
        private void UpdateProgressIndicator()
        {
            int progressPercentage = (int)((_currentStep + 1.0) / _totalSteps * 100);
            Progress_ProgressBar.Value = progressPercentage;
            ProgressValue_Label.Text = $"{_currentStep + 1} / {_totalSteps}";
        }

        // Form event handlers
        private void ReportGenerator_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }
        private void ReportGenerator_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up child forms
            _dataSelectionForm?.Dispose();
            _layoutDesignerForm?.Dispose();
            _previewExportForm?.Dispose();
        }

        // Event handlers
        private void Previous_Button_Click(object sender, EventArgs e)
        {
            if (_currentStep > 0)
            {
                SetCurrentStep((ReportStep)(_currentStep - 1));
            }
        }
        private void Next_Button_Click(object sender, EventArgs e)
        {
            if (_currentStep < _totalSteps - 1)
            {
                if (ValidateCurrentStep())
                {
                    SetCurrentStep((ReportStep)(_currentStep + 1));
                }
            }
            else
            {
                FinishReportGeneration();
            }
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            if (ConfirmCancellation())
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        // Validation and completion
        private bool ValidateCurrentStep()
        {
            return (ReportStep)_currentStep switch
            {
                ReportStep.DataSelection => _dataSelectionForm?.ValidateStep() ?? false,
                ReportStep.LayoutDesigner => _layoutDesignerForm?.ValidateStep() ?? false,
                ReportStep.PreviewExport => _previewExportForm?.ValidateStep() ?? false,
                _ => false,
            };
        }
        private void FinishReportGeneration()
        {
            try
            {
                // Final validation
                if (!_previewExportForm?.ValidateStep() ?? true)
                {
                    return;
                }

                // Export the report
                bool exportSuccess = _previewExportForm?.ExportReport() ?? false;

                if (exportSuccess)
                {
                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(
                    "Export Error",
                    $"An error occurred while exporting the report: {ex.Message}",
                    CustomMessageBoxIcon.Error,
                    CustomMessageBoxButtons.Ok
                );
            }
        }
        private static bool ConfirmCancellation()
        {
            CustomMessageBoxResult result = CustomMessageBox.Show(
                "Cancel Report Generation",
                "Are you sure you want to cancel? Any unsaved changes will be lost.",
                CustomMessageBoxIcon.Question,
                CustomMessageBoxButtons.YesNo
            );

            return result == CustomMessageBoxResult.Yes;
        }

        // Public methods for child Forms
        /// <summary>
        /// Called by child forms to notify that their validation state has changed.
        /// </summary>
        public void OnChildFormValidationChanged()
        {
            UpdateNavigationButtons();
        }
    }
}