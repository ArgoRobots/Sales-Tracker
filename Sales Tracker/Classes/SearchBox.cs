using Guna.UI2.WinForms;
using System.Drawing.Drawing2D;

namespace Sales_Tracker.Classes
{
    public class SearchBox
    {
        private static Guna2Panel SearchResultBox { get; set; }
        private static Guna2Panel SearchResultBoxContainer { get; set; }
        public static void ConstructSearchBox()
        {
            SearchResultBoxContainer = new Guna2Panel
            {
                Width = 200,
                BorderStyle = DashStyle.Solid,
                BorderColor = Color.Gray,
                BorderThickness = 1,
                FillColor = CustomColors.controlBack,
                BorderRadius = 1,
                UseTransparentBackground = true
            };
            SearchResultBox = new Guna2Panel
            {
                Width = 197,
                Location = new Point(1, 1),
                FillColor = CustomColors.controlBack
            };
            SearchResultBox.HorizontalScroll.Enabled = false;
            SearchResultBox.HorizontalScroll.Maximum = 0;
            SearchResultBox.VerticalScroll.Enabled = true;
            SearchResultBoxContainer.Controls.Add(SearchResultBox);
        }



        public class SearchResult(string name, Image? flag, int score)
        {
            public string Name { get; set; } = name;
            public Image Flag { get; set; } = flag;
            public int Score { get; set; } = score;
        }
        public const string addLine = "ADD LINE CONTROL";
        public static void ShowSearchBox(Control controlToAddSearchBox, Guna2TextBox textBox, List<SearchResult> result_list, Control deselectControl, int maxHeight, bool addExtraParent = false)
        {
            // Check if SearchResultBoxContainer is already added for this textBox
            if (controlToAddSearchBox.Controls.Contains(SearchResultBoxContainer) && SearchResultBoxContainer.Tag == textBox)
            {
                return;
            }
            SearchResultBoxContainer.Tag = textBox;

            SearchResultBox.Controls.Clear();
            SearchResultBox.SuspendLayout();  // Prevent the horizontal scrollbar from appearing

            // Simple search function
            List<SearchResult> metaList = [];
            foreach (SearchResult result in result_list)
            {
                if (textBox.Text == "")
                {
                    metaList.Add(new SearchResult(result.Name, result.Flag, 0));
                    continue;
                }
                if (result.Name == addLine)
                {
                    continue;
                }
                if (IsResultNameValid(result.Name))
                {
                    // If the result contains text that is in the textBox
                    if (result.Name.Contains(textBox.Text, StringComparison.CurrentCultureIgnoreCase) && result.Name != "")
                    {
                        int score = 1;

                        // Increase the score if the first character is the same
                        if (result.Name[0].ToString().Equals(textBox.Text[0].ToString(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            score = 2;
                        }

                        metaList.Add(new SearchResult(result.Name, result.Flag, score));
                    }
                }
            }
            metaList = metaList.OrderByDescending(x => x.Score).ToList();

            // Add results to SearchResultBox
            for (int i = 0; i < metaList.Count; i++)
            {
                if (metaList[i].Name == addLine)
                {
                    Guna2Separator seperator = UI.CosntructSeperator(177, SearchResultBox);
                    seperator.Location = new Point(10, i * 24 + 13);
                }
                else
                {
                    Guna2Button gBtn = UI.ConstructGBtn(null, metaList[i].Name, 0, new Size(197, 24), new Point(1, i * 24 + 1), SearchResultBox);
                    gBtn.Font = new Font("Segoe UI", 10);
                    gBtn.FillColor = CustomColors.controlBack;
                    gBtn.ForeColor = CustomColors.text;
                    gBtn.BorderColor = CustomColors.accent_blue;
                    gBtn.Click -= UI.CloseAllPanels;
                    gBtn.Image = metaList[i].Flag;
                    gBtn.ImageAlign = HorizontalAlignment.Left;
                    gBtn.ImageSize = new Size(25, 13);
                    gBtn.TextAlign = HorizontalAlignment.Left;
                    gBtn.Click += (sender2, e2) =>
                    {
                        // Put the name into the selected TextBox
                        textBox.Text = gBtn.Text;
                        CloseSearchBox(controlToAddSearchBox);
                        DeselectControl(deselectControl);
                    };
                }
            }

            // Set the height of SearchResultBox
            if (metaList.Count * 24 > maxHeight)
            {
                SearchResultBoxContainer.Height = maxHeight + 3;
                SearchResultBox.Height = maxHeight;
                SearchResultBox.AutoScroll = true;
            }
            else if (metaList.Count == 1)
            {
                SearchResultBoxContainer.Height = 42;
                SearchResultBox.Height = 40;
                SearchResultBox.AutoScroll = false;
            }
            else if (metaList.Count == 0)
            {
                CloseSearchBox(controlToAddSearchBox);
                return;
            }
            else
            {
                SearchResultBox.Height = metaList.Count * 24 + 2;
                SearchResultBoxContainer.Height = SearchResultBox.Height + 10;
                SearchResultBox.AutoScroll = false;
            }

            // Show search box
            if (textBox.Parent is Form)
            {
                SearchResultBoxContainer.Location = new Point(textBox.Left, textBox.Top + textBox.Height);
            }
            else
            {
                if (addExtraParent)
                {
                    SearchResultBoxContainer.Location = new Point(textBox.Left + textBox.Parent.Left + textBox.Parent.Parent.Left,
                                                                  textBox.Top + textBox.Parent.Top + textBox.Parent.Parent.Top + textBox.Height);
                }
                else
                {
                    SearchResultBoxContainer.Location = new Point(textBox.Left + textBox.Parent.Left,
                                                                  textBox.Top + textBox.Parent.Top + textBox.Height);
                }
            }
            controlToAddSearchBox.Controls.Add(SearchResultBoxContainer);
            SearchResultBox.ResumeLayout();
            SearchResultBoxContainer.BringToFront();
        }
        public static List<SearchResult> ConvertToSearchResults(List<string> names)
        {
            return names.Select(name => new SearchResult(name, null, 0)).ToList();
        }
        /// <summary>
        /// Updates the search box and checks if the textBox is valid.
        /// </summary>
        public static void SearchTextBoxChanged(Control controlToAddSearchBox, Guna2TextBox textBox, List<SearchResult> result_list, Control deselectControl, Guna2Button button, int maxHeight)
        {
            ShowSearchBox(controlToAddSearchBox, textBox, result_list, deselectControl, maxHeight);
            List<string> names = result_list.Select(result => result.Name).ToList();
            CheckValidity(textBox, names, button);
        }
        public static void CheckValidity(Guna2TextBox textBox, List<string> resultNames_list, Guna2Button button)
        {
            if (resultNames_list.Contains(textBox.Text))
            {
                SetTextBoxToValid(textBox, button);
            }
            else { SetTextBoxToInvalid(textBox, button); }
        }
        public static void SearchBoxTextBox_KeyDown(Guna2TextBox textBox, Control controlToRemoveSearchBox, Control deselectControl, KeyEventArgs e)
        {
            System.Collections.IList results = SearchResultBox.Controls;

            if (results.Count == 0)
            {
                return;
            }

            // Select the next result
            bool isResultSelected = false;
            if (e.KeyCode is Keys.Down or Keys.Tab)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    Guna2Button btn = (Guna2Button)results[i];

                    // Find the result that is selected
                    if (btn.BorderThickness == 1)
                    {
                        // Unselect current one
                        btn.BorderThickness = 0;

                        // If it's not the last one
                        if (i < results.Count - 1)
                        {
                            // Select the next result
                            Guna2Button nextBtn = (Guna2Button)results[i + 1];
                            nextBtn.BorderThickness = 1;
                            SearchResultBox.ScrollControlIntoView(nextBtn);
                        }
                        else
                        {
                            // Select the first one
                            Guna2Button firstBtn = (Guna2Button)results[0];
                            firstBtn.BorderThickness = 1;
                            SearchResultBox.ScrollControlIntoView(firstBtn);
                        }
                        isResultSelected = true;
                        break;
                    }
                }
            }
            else if (e.KeyCode is Keys.Up)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    Guna2Button btn = (Guna2Button)results[i];
                    // Find the result that is selected
                    if (btn.BorderThickness == 1)
                    {
                        // Unselect current one
                        btn.BorderThickness = 0;

                        // If it's not the first one
                        if (i > 0)
                        {
                            // Select the previous result
                            Guna2Button nextBtn = (Guna2Button)results[i - 1];
                            nextBtn.BorderThickness = 1;
                            SearchResultBox.ScrollControlIntoView(nextBtn);
                        }
                        else
                        {
                            // Select the last one
                            Guna2Button firstBtn = (Guna2Button)results[results.Count - 1];
                            firstBtn.BorderThickness = 1;
                            SearchResultBox.ScrollControlIntoView(firstBtn);
                        }
                        isResultSelected = true;
                        break;
                    }
                }
            }
            else if (e.KeyCode is Keys.Enter)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    Guna2Button btn = (Guna2Button)results[i];
                    // Find the result that is selected
                    if (btn.BorderThickness == 1)
                    {
                        textBox.Text = btn.Text;

                        CloseSearchBox(controlToRemoveSearchBox);
                        DeselectControl(deselectControl);

                        isResultSelected = true;
                        break;
                    }
                }
            }

            if (e.KeyCode is Keys.Enter or Keys.Tab)
            {
                // Remove Windows "ding" noise when user presses enter
                e.SuppressKeyPress = true;
            }
            if (!isResultSelected)
            {
                // Select the first one
                Guna2Button firstBtn = (Guna2Button)results[0];
                firstBtn.BorderThickness = 1;
            }
        }
        private static void DeselectControl(Control deselectControl)
        {
            deselectControl.Focus();
        }
        private static void SetTextBoxToInvalid(Guna2TextBox gTextBox, Guna2Button button)
        {
            gTextBox.BorderColor = CustomColors.accent_red;
            gTextBox.HoverState.BorderColor = CustomColors.accent_red;
            gTextBox.FocusedState.BorderColor = CustomColors.accent_red;
            button.Tag = 0;
        }
        private static void SetTextBoxToValid(Guna2TextBox gTextBox, Guna2Button button)
        {
            gTextBox.BorderColor = CustomColors.controlBorder;
            gTextBox.HoverState.BorderColor = CustomColors.accent_blue;
            gTextBox.FocusedState.BorderColor = CustomColors.accent_blue;
            button.Tag = 1;
        }

        private static bool IsResultNameValid(string resultName)
        {
            if (string.IsNullOrEmpty(resultName))
            {
                return false;
            }

            // Check if the first character is a letter
            if (!char.IsLetter(resultName[0]))
            {
                return false;
            }

            // Check if all characters are letters, digits, or allowed special characters
            return resultName.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '(' || c == ')' || c == ' ');
        }
        public static void CloseSearchBox(Control controlToRemoveSearchBox)
        {
            controlToRemoveSearchBox.Controls.Remove(SearchResultBoxContainer);
            SearchResultBox.Controls.Clear();
        }
        public static void AllowTabAndEnterKeysInTextBox_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
        {
            // Some key presses, such as the TAB, RETURN, ESC, and arrow keys, are ignored by some controls because they are not considered input key presses.
            // You can handle PreviewKeyDown and set them as input key.
            // https://stackoverflow.com/questions/35914536/detect-the-tab-key-press-in-textbox

            if (e.KeyData is Keys.Tab or Keys.Enter)
            {
                e.IsInputKey = true;
            }
        }
    }
}