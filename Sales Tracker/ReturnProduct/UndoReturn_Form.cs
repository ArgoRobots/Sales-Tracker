using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
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
        private Label _returnStatusLabel;

        // Init.
        public UndoReturn_Form(DataGridViewRow transactionRow)
        {
            InitializeComponent();

            _transactionRow = transactionRow;

            // Get return information
            (_, _, _, List<int> returnedItems) = ReturnManager.GetReturnInfo(_transactionRow);
            _returnedItems = returnedItems ?? [];

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
            string transactionType = isPurchase ? "Purchase" : "Sale";

            TransactionDetails_Label.Text = $"{transactionType} Transaction Details:";

            if (_hasPartialReturn)
            {
                // For partial returns, show transaction overview and return status
                int totalItems = _items.Count;

                // Exclude receipt from count if present
                if (_items.Count > 0 && _items[^1].StartsWith(ReadOnlyVariables.Receipt_text))
                {
                    totalItems--;
                }

                List<string> returnedItemNames = ReturnManager.GetReturnedItemNames(_transactionRow);

                TransactionInfo_Label.Text = $"Transaction ID: {transactionId}\n" +
                                             $"Multiple Items Transaction\n" +
                                             $"Total Items: {totalItems}\n" +
                                             $"Returned items: {string.Join(", ", returnedItemNames)}\n" +
                                             $"Date: {date}";
            }
            else
            {
                // For full returns, show standard details
                TransactionInfo_Label.Text = $"Transaction ID: {transactionId}\n" +
                                             $"Product: {productName}\n" +
                                             $"Category: {categoryName}\n" +
                                             $"Date: {date}";
            }

            // Load return information
            (DateTime? returnDate, string returnReason, string returnedBy, _) = ReturnManager.GetReturnInfo(_transactionRow);

            ReturnInfo_Label.Text = $"Returned on: {returnDate?.ToString("MM/dd/yyyy HH:mm") ?? "Unknown"}\n" +
                                    $"Reason: {returnReason ?? "No reason provided"}\n" +
                                    $"Returned by: {returnedBy ?? "Unknown"}";
        }
        private void CreateItemSelectionControls()
        {
            // Create status label
            _returnStatusLabel = new Label
            {
                Text = "Return Status:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = CustomColors.Text,
                AutoSize = true,
                Location = new Point(ReturnInfo_Label.Left, ReturnInfo_Label.Bottom + 20)
            };
            Controls.Add(_returnStatusLabel);

            // Create label for item selection
            _selectItemsLabel = new Label
            {
                Text = "Select returned items to undo:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = CustomColors.Text,
                AutoSize = true,
                Location = new Point(_returnStatusLabel.Left, _returnStatusLabel.Bottom + 15)
            };
            Controls.Add(_selectItemsLabel);

            // Don't include receipt in the list
            int itemsToShow = _items.Count;
            if (_items.Count > 0 && _items[^1].StartsWith(ReadOnlyVariables.Receipt_text))
            {
                itemsToShow--;
            }

            // Count how many items are actually returned (will be shown)
            int returnedItemsToShow = 0;
            for (int i = 0; i < itemsToShow; i++)
            {
                if (_returnedItems.Contains(i))
                {
                    returnedItemsToShow++;
                }
            }

            // Calculate dynamic panel height based on number of returned items
            const int itemHeight = 35;       // Height per item
            const int panelPadding = 20;     // Top and bottom padding
            const int maxPanelHeight = 300;  // Maximum height before scrolling
            const int minPanelHeight = 60;   // Minimum height for "no items" message

            int calculatedHeight = returnedItemsToShow > 0
                ? (returnedItemsToShow * itemHeight) + panelPadding
                : minPanelHeight;
            int panelHeight = Math.Min(calculatedHeight, maxPanelHeight);

            // Create panel to hold item checkboxes
            _itemsPanel = new Guna2Panel
            {
                Location = new Point(_selectItemsLabel.Left, _selectItemsLabel.Bottom + 10),
                Size = new Size(Width - 60, panelHeight),
                FillColor = CustomColors.ControlBack,
                AutoScroll = true
            };
            Controls.Add(_itemsPanel);

            // Create checkboxes for returned items only
            _itemCheckboxes = [];
            int yPosition = 10;

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
                    Text = $"{productName} ({companyName}) - Qty: {quantity} @ {MainMenu_Form.CurrencySymbol}{pricePerUnit:N2}",
                    MaximumSize = new Size(_itemsPanel.Width - 50, 0),
                    Font = new Font("Segoe UI", 10),
                    ForeColor = CustomColors.AccentRed,  // Show in red to indicate it's returned
                    AutoSize = true,
                    AutoEllipsis = true,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Position label vertically centered with checkbox after AutoSize
                const int checkBoxHeight = 20;
                int labelYOffset = (checkBoxHeight - itemLabel.PreferredHeight) / 2;
                itemLabel.Location = new Point(35, yPosition + labelYOffset);

                // Handle label click to toggle checkbox
                itemLabel.Click += (s, e) =>
                {
                    itemCheckBox.Checked = !itemCheckBox.Checked;
                };

                itemCheckBox.CheckedChanged += ItemCheckBox_CheckedChanged;
                _itemCheckboxes.Add(itemCheckBox);
                _itemsPanel.Controls.Add(itemCheckBox);
                _itemsPanel.Controls.Add(itemLabel);

                yPosition += itemHeight;
            }

            // Adjust form height to accommodate new controls
            int additionalHeight = _itemsPanel.Bottom - ReturnInfo_Label.Bottom;
            Height += additionalHeight;
            MinimumSize = new Size(Width, Height);

            // Move existing controls down
            UndoReason_Label.Top += additionalHeight;
            UndoReason_TextBox.Top += additionalHeight;
            CharacterCount_Label.Top += additionalHeight;

            // Update validation since we now need at least one item selected for partial undos
            ValidateInputs();
        }
        private void UpdateTheme()
        {
            ThemeManager.SetThemeForForm(this);

            ThemeManager.MakeGButtonBlueSecondary(Cancel_Button);
            ThemeManager.MakeGButtonBluePrimary(UndoReturn_Button);
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
            if (currentLength >= maxLength)
            {
                CharacterCount_Label.ForeColor = CustomColors.AccentRed;
            }
            else
            {
                CharacterCount_Label.ForeColor = CustomColors.Text;
            }
        }
    }
}