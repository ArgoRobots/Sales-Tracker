﻿using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Language;
using Sales_Tracker.ReturnProduct;
using Sales_Tracker.Theme;
using Sales_Tracker.UI;

namespace Sales_Tracker
{
    public partial class UndoReturn_Form : BaseForm
    {
        // Properties
        private readonly DataGridViewRow _transactionRow;
        private readonly bool _hasPartialReturn;
        private readonly List<string> _items;
        private readonly List<int> _returnedItems;
        private List<Guna2CustomCheckBox> _itemCheckboxes;
        private Guna2Panel _itemsPanel;
        private Label _selectItemsLabel;

        // Init.
        public UndoReturn_Form() : this(null) { }  // This is needed for TranslationGenerator.GenerateAllLanguageTranslationFiles()
        public UndoReturn_Form(DataGridViewRow transactionRow)
        {
            InitializeComponent();
            if (transactionRow == null) { return; }

            _transactionRow = transactionRow;

            // Get return information
            ReturnInfo returnInfo = ReturnManager.GetReturnInfo(_transactionRow);
            _returnedItems = returnInfo.ReturnedItems;

            _hasPartialReturn = _transactionRow.Tag is (List<string>, TagData) && _returnedItems.Count > 0;

            if (_hasPartialReturn && _transactionRow.Tag is ValueTuple<List<string>, TagData> tuple)
            {
                _items = tuple.Item1;
            }
            else
            {
                _items = [];
            }

            LoadTransactionData();
            if (_hasPartialReturn)
            {
                CreateItemSelectionControls();
            }
            UpdateTheme();
            SetAccessibleDescriptions();
            UpdateCharacterCount();
            LanguageManager.UpdateLanguageForControl(this);
            LoadingPanel.ShowBlankLoadingPanel(this);
        }
        private void LoadTransactionData()
        {
            string transactionId = _transactionRow.Cells[ReadOnlyVariables.ID_column].Value.ToString();
            string productName = _transactionRow.Cells[ReadOnlyVariables.Product_column].Value.ToString();
            string categoryName = _transactionRow.Cells[ReadOnlyVariables.Category_column].Value.ToString();
            string date = _transactionRow.Cells[ReadOnlyVariables.Date_column].Value.ToString();
            bool isPurchase = MainMenu_Form.Instance.Selected == MainMenu_Form.SelectedOption.Purchases;
            string transactionType = isPurchase ? LanguageManager.TranslateString("Purchase") : LanguageManager.TranslateString("Sale");

            TransactionDetails_Label.Text = transactionType + LanguageManager.TranslateString(" Transaction Details:");

            if (_hasPartialReturn)
            {
                // For partial returns, show transaction overview and return status
                int totalItems = _items.Count;

                // Exclude receipt from count if present
                if (_items.Count > 0 && _items[^1].StartsWith(ReadOnlyVariables.Receipt_text))
                {
                    totalItems--;
                }

                TransactionInfo_Label.Text = $"{LanguageManager.TranslateString("Transaction ID")}: {transactionId}\n" +
                                             $"{LanguageManager.TranslateString("Multiple Items Transaction")}\n" +
                                             $"{LanguageManager.TranslateString("Total Items")}: {totalItems}\n" +
                                             $"{LanguageManager.TranslateString("Date")}: {date}";
            }
            else
            {
                // For full returns, show standard details
                TransactionInfo_Label.Text = $"{LanguageManager.TranslateString("Transaction ID")}: {transactionId}\n" +
                                             $"{LanguageManager.TranslateString("Product")}: {productName}\n" +
                                             $"{LanguageManager.TranslateString("Category")}: {categoryName}\n" +
                                             $"{LanguageManager.TranslateString("Date")}: {date}";
            }

            // Load return information
            ReturnInfo returnInfo = ReturnManager.GetReturnInfo(_transactionRow);

            ReturnInfo_Label.Text = $"{LanguageManager.TranslateString("Returned on")}: {returnInfo.FormattedDate}\n" +
                                    $"{LanguageManager.TranslateString("Reason")}: {returnInfo.DisplayReason}\n" +
                                    $"{LanguageManager.TranslateString("Returned by")}: {returnInfo.DisplayActionBy}";
        }
        private void CreateItemSelectionControls()
        {
            // Create label for item selection
            _selectItemsLabel = new Label
            {
                Text = LanguageManager.TranslateString("Select returned items to undo:"),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = CustomColors.Text,
                AutoSize = true,
                Location = new Point(ReturnInformation_Label.Left, ReturnInfo_Label.Bottom + 10)
            };
            Controls.Add(_selectItemsLabel);

            // Don't include receipt in the list
            int itemsToShow = _items.Count;
            if (_items.Count > 0 && _items[^1].StartsWith(ReadOnlyVariables.Receipt_text))
            {
                itemsToShow--;
            }

            // Count how many items are actually returned
            int returnedItemsToShow = 0;
            for (int i = 0; i < itemsToShow; i++)
            {
                if (_returnedItems.Contains(i))
                {
                    returnedItemsToShow++;
                }
            }

            const int checkBoxHeight = 20;
            const int minItemHeight = 35;
            const int itemPadding = 5;
            const int panelPadding = 20;
            const int maxPanelHeight = 300;

            // Create panel to hold item checkboxes
            _itemsPanel = new Guna2Panel
            {
                Location = new Point(_selectItemsLabel.Left, _selectItemsLabel.Bottom + 10),
                FillColor = CustomColors.ControlBack,
                Width = Width - 60,
                AutoScroll = true
            };
            Controls.Add(_itemsPanel);

            // Create checkboxes for returned items only
            _itemCheckboxes = [];
            int yPosition = 10;
            int totalCalculatedHeight = panelPadding;

            for (int i = 0; i < itemsToShow; i++)
            {
                string[] itemDetails = _items[i].Split(',');
                if (itemDetails.Length < 6) { continue; }

                string productName = itemDetails[0];
                string categoryName = itemDetails[1];
                string companyName = itemDetails[3];
                int quantity = int.Parse(itemDetails[4]);
                decimal pricePerUnit = decimal.Parse(itemDetails[5]);

                bool isItemReturned = _returnedItems.Contains(i);

                // Only show returned items as they are the ones that can be undone
                if (!isItemReturned) { continue; }

                // Create custom checkbox
                Guna2CustomCheckBox itemCheckBox = new()
                {
                    Size = new Size(20, 20),
                    Location = new Point(10, yPosition),
                    Tag = i,  // Store the item index
                    Animated = true
                };

                // Create label for the checkbox
                Label itemLabel = new()
                {
                    Text = $"{productName} ({companyName}) - {LanguageManager.TranslateString("Quantity")}: {quantity} @ {MainMenu_Form.CurrencySymbol}{pricePerUnit:N2}",
                    MaximumSize = new Size(_itemsPanel.Width - 50, 0),
                    Font = new Font("Segoe UI", 10),
                    ForeColor = CustomColors.AccentRed,  // Show in red to indicate it's returned
                    AutoSize = true,
                    AutoEllipsis = true,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Calculate the actual height needed for this item
                int labelHeight = itemLabel.PreferredHeight;
                int actualItemHeight = Math.Max(minItemHeight, Math.Max(checkBoxHeight, labelHeight) + itemPadding);

                // Position label vertically centered with checkbox
                int labelYOffset = (actualItemHeight - labelHeight) / 2;
                itemLabel.Location = new Point(35, yPosition + labelYOffset);

                // Position checkbox vertically centered
                int checkBoxYOffset = (actualItemHeight - checkBoxHeight) / 2;
                itemCheckBox.Location = new Point(10, yPosition + checkBoxYOffset);

                // Handle label click to toggle checkbox
                itemLabel.Click += (s, e) =>
                {
                    itemCheckBox.Checked = !itemCheckBox.Checked;
                };

                itemCheckBox.CheckedChanged += ItemCheckBox_CheckedChanged;
                _itemCheckboxes.Add(itemCheckBox);
                _itemsPanel.Controls.Add(itemCheckBox);
                _itemsPanel.Controls.Add(itemLabel);

                yPosition += actualItemHeight;
                totalCalculatedHeight += actualItemHeight;
            }

            // Set the final panel height
            _itemsPanel.Height = Math.Min(totalCalculatedHeight, maxPanelHeight);

            // Adjust form height to accommodate new controls
            int additionalHeight = _itemsPanel.Bottom - ReturnInfo_Label.Bottom;
            Height += additionalHeight;
            MinimumSize = Size;

            // Update validation since we now need at least one item selected for partial undos
            ValidateInputs();
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);

            ThemeManager.MakeGButtonBlueSecondary(Cancel_Button);
            ThemeManager.MakeGButtonBluePrimary(UndoReturn_Button);
        }
        private void SetAccessibleDescriptions()
        {
            TransactionInfo_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            ReturnInfo_Label.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
        }

        // Form event handlers
        private void UndoReturn_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Event handlers
        private void UndoReturn_Button_Click(object sender, EventArgs e)
        {
            if (_hasPartialReturn && _itemCheckboxes?.Count > 0)
            {
                // Get selected items to undo
                List<int> selectedItemIndices = [];
                foreach (Guna2CustomCheckBox checkBox in _itemCheckboxes)
                {
                    if (checkBox.Checked && checkBox.Tag is int index)
                    {
                        selectedItemIndices.Add(index);
                    }
                }

                if (selectedItemIndices.Count == 0)
                {
                    CustomMessageBox.Show("No items selected", "Please select at least one returned item to undo.",
                        CustomMessageBoxIcon.Info, CustomMessageBoxButtons.Ok);
                    return;
                }

                // Check if user wants to undo all returned items
                if (selectedItemIndices.Count == _returnedItems.Count)
                {
                    // Undoing all items - use full undo
                    ReturnManager.UndoReturn(_transactionRow, UndoReason_TextBox.Text.Trim());
                }
                else
                {
                    // Partial undo
                    ReturnManager.UndoPartialReturn(_transactionRow, selectedItemIndices, UndoReason_TextBox.Text.Trim());
                }
            }
            else
            {
                // Full undo for single item or complete transaction return
                ReturnManager.UndoReturn(_transactionRow, UndoReason_TextBox.Text.Trim());
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        private void UndoReason_TextBox_TextChanged(object sender, EventArgs e)
        {
            ValidateInputs();
            UpdateCharacterCount();
        }
        private void ItemCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ValidateInputs();
        }

        // Methods
        private void ValidateInputs()
        {
            bool reasonProvided = !string.IsNullOrWhiteSpace(UndoReason_TextBox.Text);
            bool itemsSelected = true;

            if (_hasPartialReturn && _itemCheckboxes?.Count > 0)
            {
                // For partial returns with selectable items, at least one item must be selected
                itemsSelected = _itemCheckboxes.Any(cb => cb.Checked);
            }

            UndoReturn_Button.Enabled = reasonProvided && itemsSelected;
        }
        private void UpdateCharacterCount()
        {
            int currentLength = UndoReason_TextBox.Text.Length;
            int maxLength = UndoReason_TextBox.MaxLength;

            CharacterCount_Label.Text = $"{currentLength}/{maxLength}";

            // Change color to red when at max limit
            CharacterCount_Label.ForeColor = currentLength >= maxLength ? CustomColors.AccentRed : CustomColors.Text;
        }
    }
}