using Guna.UI2.WinForms;
using System.Drawing.Drawing2D;

namespace Sales_Tracker.Classes
{
    internal class SearchBox
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
            SearchResultBoxContainer.Controls.Add(SearchResultBox);
        }
        private class ResultsMeta
        {
            public string name;
            public int score;
        }
        public static void ShowSearchBox(Control controlToAddSearchBox, Guna2TextBox textBox, List<string> resultNames_list, Control deselectControl, int maxHeight)
        {
            SearchResultBox.Controls.Clear();

            // Simple search function
            List<ResultsMeta> metaList = [];
            foreach (string result in resultNames_list)
            {
                if (textBox.Text == "")
                {
                    metaList.Add(new ResultsMeta() { name = result, score = 0 });
                    continue;
                }
                if (IsVariableNameValid(result))
                {
                    // If the variable contains text that is in the textBox
                    if (result.ToLower().Contains(textBox.Text.ToLower()) && result != "")
                    {
                        int scoreValue = 1;

                        // Increase the score if the first character is the same
                        if (result[0].ToString().ToLower() == textBox.Text[0].ToString().ToLower())
                        {
                            scoreValue = 2;
                        }

                        metaList.Add(new ResultsMeta() { name = result, score = scoreValue });
                    }
                }
            }
            metaList = metaList.OrderByDescending(x => x.score).ToList();

            // Add variables to variableBox
            for (int i = 0; i < metaList.Count; i++)
            {
                Guna2Button gBtn = UI.ConstructGBtn(null, metaList[i].name, 0, new Size(197, 24), new Point(1, i * 24 + 1), SearchResultBox);
                gBtn.Font = new Font("Segoe UI", 10);
                gBtn.FillColor = CustomColors.controlBack;
                gBtn.ForeColor = CustomColors.text;
                gBtn.BorderColor = CustomColors.accent_blue;
                gBtn.Click -= UI.CloseAllPanels;
                gBtn.Click += (sender2, e2) =>
                {
                    // Put the variable name into the selected TextBox
                    textBox.Text = gBtn.Text;

                    CloseVariableBox(controlToAddSearchBox);
                    DeselectControl(deselectControl);
                };
            }

            // Set the height of variableBox
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
                CloseVariableBox(controlToAddSearchBox);
                return;
            }
            else
            {
                SearchResultBox.Height = metaList.Count * 24 + 2;
                SearchResultBoxContainer.Height = SearchResultBox.Height + 10;
                SearchResultBox.AutoScroll = false;
            }

            // Show variable box
            if (textBox.Parent is Form)
            {
                SearchResultBoxContainer.Location = new Point(textBox.Left, textBox.Top + textBox.Height);
            }
            else
            {
                SearchResultBoxContainer.Location = new Point(textBox.Left + textBox.Parent.Left, textBox.Top + textBox.Parent.Top + textBox.Height);
            }
            controlToAddSearchBox.Controls.Add(SearchResultBoxContainer);
            SearchResultBoxContainer.BringToFront();
        }
        /// <summary>
        /// Updates the variableBox and checks if the textBox is valid.
        /// </summary>
        public static void VariableTextBoxChanged(Control controlToAddSearchBox, Guna2TextBox textBox, List<string> resultNames_list, Control deselectControl, Guna2Button button, int maxHeight)
        {
            ShowSearchBox(controlToAddSearchBox, textBox, resultNames_list, deselectControl, maxHeight);
            CheckValidity(textBox, resultNames_list, button);
        }
        public static void CheckValidity(Guna2TextBox textBox, List<string> resultNames_list, Guna2Button button)
        {
            if (resultNames_list.Contains(textBox.Text) || textBox.Text.All(char.IsDigit))
            {
                SetTextBoxToValid(textBox, button);
            }
            else
            {
                SetTextBoxToInvalid(textBox, button);
            }
        }
        public static void VariableTextBox_KeyDown(Guna2TextBox textBox, Control controlToRemoveSearchBox, Control deselectControl, KeyEventArgs e)
        {
            System.Collections.IList results = SearchResultBox.Controls;

            if (results.Count == 0)
            {
                return;
            }

            // Select the next variable
            bool isVariableSelected = false;
            if (e.KeyCode is Keys.Down or Keys.Tab)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    Guna2Button btn = (Guna2Button)results[i];

                    // Find the variable that is selected
                    if (btn.BorderThickness == 1)
                    {
                        // Unselect current one
                        btn.BorderThickness = 0;

                        // If it's not the last one
                        if (i < results.Count - 1)
                        {
                            // Select the next variable
                            Guna2Button nextBtn = (Guna2Button)results[i + 1];
                            nextBtn.BorderThickness = 1;
                        }
                        else
                        {
                            // Select the first one
                            Guna2Button firstBtn = (Guna2Button)results[0];
                            firstBtn.BorderThickness = 1;
                        }
                        isVariableSelected = true;
                        break;
                    }
                }
            }
            else if (e.KeyCode is Keys.Up)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    Guna2Button btn = (Guna2Button)results[i];
                    // Find the variable that is selected
                    if (btn.BorderThickness == 1)
                    {
                        // Unselect current one
                        btn.BorderThickness = 0;

                        // If it's not the first one
                        if (i > 0)
                        {
                            // Select the previous variable
                            Guna2Button nextBtn = (Guna2Button)results[i - 1];
                            nextBtn.BorderThickness = 1;
                        }
                        else
                        {
                            // Select the last one
                            Guna2Button firstBtn = (Guna2Button)results[results.Count - 1];
                            firstBtn.BorderThickness = 1;
                        }
                        isVariableSelected = true;
                        break;
                    }
                }
            }
            else if (e.KeyCode is Keys.Enter)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    Guna2Button btn = (Guna2Button)results[i];
                    // Find the variable that is selected
                    if (btn.BorderThickness == 1)
                    {
                        textBox.Text = btn.Text;

                        CloseVariableBox(controlToRemoveSearchBox);
                        DeselectControl(deselectControl);

                        isVariableSelected = true;
                        break;
                    }
                }
            }

            if (e.KeyCode is Keys.Enter or Keys.Tab)
            {
                // Remove Windows "ding" noise when user presses enter
                e.SuppressKeyPress = true;
            }
            if (!isVariableSelected)
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
            gTextBox.BorderThickness = 2;
            gTextBox.HoverState.BorderColor = CustomColors.accent_red;
            gTextBox.FocusedState.BorderColor = CustomColors.accent_red;

            if (button != null)
            {
                button.Tag = 0;
            }
        }
        private static void SetTextBoxToValid(Guna2TextBox gTextBox, Guna2Button button)
        {
            gTextBox.BorderColor = CustomColors.controlBorder;
            gTextBox.BorderThickness = 1;
            gTextBox.HoverState.BorderColor = CustomColors.accent_blue;
            gTextBox.FocusedState.BorderColor = CustomColors.accent_blue;

            if (button != null)
            {
                button.Tag = 1;
            }
        }

        private static bool IsVariableNameValid(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
            {
                return false;
            }

            // Check if the first character is a letter
            if (!char.IsLetter(variableName[0]))
            {
                return false;
            }

            // Check if all characters are letters, digits, or allowed special characters
            return variableName.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '(' || c == ')' || c == ' ');
        }

        public static void CloseVariableBox(Control controlToRemoveSearchBox)
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