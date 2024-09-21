using Guna.UI2.WinForms;
using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker.Classes
{
    public class SearchBox
    {
        // Properties
        private static Guna2Panel _searchResultBox;
        private static Guna2Panel _searchResultBoxContainer;
        private static Timer debounceTimer;

        // Getters
        public static Guna2Panel SearchResultBoxContainer => _searchResultBoxContainer;
        public static Guna2Panel SearchResultBox => _searchResultBox;

        // List to store and reuse controls
        private readonly static List<Control> searchResultControls = [];

        // Init.
        public static void ConstructSearchBox()
        {
            _searchResultBoxContainer = new Guna2Panel
            {
                BorderStyle = DashStyle.Solid,
                BorderColor = Color.Gray,
                BorderThickness = 1,
                FillColor = CustomColors.controlBack,
                BorderRadius = 1,
                UseTransparentBackground = true
            };
            _searchResultBox = new Guna2Panel
            {
                Location = new Point(1, 1),
                FillColor = CustomColors.controlBack
            };
            _searchResultBox.HorizontalScroll.Enabled = false;
            _searchResultBox.HorizontalScroll.Maximum = 0;
            _searchResultBoxContainer.Controls.Add(_searchResultBox);

            debounceTimer = new Timer
            {
                Interval = 300
            };
            debounceTimer.Tick += DebounceTimer_Tick;
        }

        public const string addLine = "ADD LINE CONTROL";
        private static Control controlToAddSearchBox;
        private static Guna2TextBox searchTextBox;
        private static List<SearchResult> resultList;
        private static Control deselectControl;
        private static int maxHeight;

        // Event handlers
        private static void DebounceTimer_Tick(object sender, EventArgs e)
        {
            debounceTimer.Stop();
            ShowSearchBox(controlToAddSearchBox, searchTextBox, resultList, deselectControl, maxHeight);
        }

        // Main methods
        public static void ShowSearchBox(Control controlToAddBox, Guna2TextBox textBox, List<SearchResult> result_list, Control deselectControl, int maxHeight)
        {
            // Start timing
            long startTime = DateTime.Now.Ticks;

            controlToAddSearchBox = controlToAddBox;
            UI.CloseAllPanels(null, null);

            if (result_list.Count == 0)
            {
                CloseSearchBox(controlToAddSearchBox);
                return;
            }

            _searchResultBox.SuspendLayout();
            foreach (Control control in searchResultControls)
            {
                control.Visible = false;
            }

            List<SearchResult> metaList = [];
            string searchText = textBox.Text;

            if (string.IsNullOrEmpty(searchText))
            {
                foreach (SearchResult result in result_list)
                {
                    metaList.Add(new SearchResult(result.Name, result.Flag, 0));
                }
            }
            else
            {
                foreach (SearchResult result in result_list)
                {
                    if (result.Name == addLine) { continue; }

                    if (result.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Increase the score if the first letter is the same
                        int score = result.Name[0].ToString().Equals(searchText[0].ToString(), StringComparison.CurrentCultureIgnoreCase) ? 2 : 1;
                        metaList.Add(new SearchResult(result.Name, result.Flag, score));
                    }
                }
            }

            metaList = metaList.OrderByDescending(x => x.Score).ToList();

            // Add results to _searchResultBox
            int yOffset = 1;
            int buttonHeight = 35;
            int controlIndex = 0;

            foreach (SearchResult meta in metaList)
            {
                if (meta.Name == addLine)
                {
                    Guna2Separator separator;
                    if (controlIndex < searchResultControls.Count && searchResultControls[controlIndex] is Guna2Separator existingSeparator)
                    {
                        separator = existingSeparator;
                    }
                    else
                    {
                        separator = UI.ConstructSeperator(CalculateControlWidth(metaList.Count), _searchResultBox);
                        _searchResultBox.Controls.Add(separator);
                        searchResultControls.Add(separator);
                    }
                    separator.Visible = true;
                    separator.Location = new Point(10, yOffset + 12);
                }
                else
                {
                    Guna2Button btn;
                    if (controlIndex < searchResultControls.Count && searchResultControls[controlIndex] is Guna2Button existingButton)
                    {
                        btn = existingButton;
                    }
                    else
                    {
                        btn = new Guna2Button
                        {
                            Font = new Font("Segoe UI", 10),
                            FillColor = CustomColors.controlBack,
                            ForeColor = CustomColors.text,
                            BorderColor = CustomColors.accent_blue,
                            ImageAlign = HorizontalAlignment.Left,
                            ImageSize = new Size(25, 13),  // Flags have different ratios. This is just a good size
                            TextAlign = HorizontalAlignment.Left
                        };
                        btn.Click += (sender, e) =>
                        {
                            Guna2Button? button = sender as Guna2Button;
                            Guna2TextBox selectedTextBox = (Guna2TextBox)button.Tag;
                            selectedTextBox.Text = button.Text;
                            CloseSearchBox(controlToAddSearchBox);
                            deselectControl.Focus();
                            debounceTimer.Stop();
                        };
                        _searchResultBox.Controls.Add(btn);
                        searchResultControls.Add(btn);
                    }
                    btn.Size = new Size(CalculateControlWidth(metaList.Count), buttonHeight);
                    btn.Text = meta.Name;
                    btn.Location = new Point(1, yOffset);
                    btn.Image = meta.Flag;
                    btn.Visible = true;
                    btn.Tag = textBox;
                }
                yOffset += buttonHeight;
                controlIndex++;
            }

            // Hide any remaining controls
            for (int i = controlIndex; i < searchResultControls.Count; i++)
            {
                searchResultControls[i].Visible = false;
            }

            int totalHeight = yOffset + 1;
            if (totalHeight > maxHeight)
            {
                _searchResultBoxContainer.Height = maxHeight + 3;
                _searchResultBox.Height = maxHeight;
                _searchResultBox.AutoScroll = true;
                Theme.CustomizeScrollBar(_searchResultBox);
            }
            else if (controlIndex == 0)
            {
                CloseSearchBox(controlToAddSearchBox);
                debounceTimer.Stop();
                return;
            }
            else
            {
                _searchResultBox.Height = totalHeight;
                _searchResultBoxContainer.Height = totalHeight + 10;
                _searchResultBox.AutoScroll = false;
            }

            // Set width to match textBox
            _searchResultBoxContainer.Width = textBox.Width;
            _searchResultBox.Width = textBox.Width - 3;

            // Show search box
            SetSearchBoxLocation(textBox);
            if (!controlToAddSearchBox.Controls.Contains(_searchResultBoxContainer))
            {
                controlToAddSearchBox.Controls.Add(_searchResultBoxContainer);
            }
            _searchResultBox.ResumeLayout();
            _searchResultBoxContainer.BringToFront();

            // End timing
            long endTime = DateTime.Now.Ticks;

            // Calculate elapsed time in milliseconds
            double elapsedTime = (endTime - startTime) / TimeSpan.TicksPerMillisecond;
            Log.Write(1, "Elapsed time for updating the SearchBox: " + elapsedTime + " ms");
        }
        private static int CalculateControlWidth(int count)
        {
            if (count > 12)
            {
                return _searchResultBox.Width - SystemInformation.VerticalScrollBarWidth - 4;
            }
            else
            {
                return _searchResultBox.Width - 2;
            }
        }
        public static List<SearchResult> ConvertToSearchResults(List<string> names)
        {
            return names.Select(name => new SearchResult(name, null, 0)).ToList();
        }
        public static void SearchTextBoxChanged(Control controlToAddSearchBox, Guna2TextBox textBox, List<SearchResult> result_list, Control deselectControl, int maxHeight)
        {
            // Save parameters for debounce mechanism
            SearchBox.controlToAddSearchBox = controlToAddSearchBox;
            searchTextBox = textBox;
            resultList = result_list;
            SearchBox.deselectControl = deselectControl;
            SearchBox.maxHeight = maxHeight;

            HashSet<string> names = new(resultList.Select(result => result.Name));
            CheckValidity(searchTextBox, names);

            debounceTimer.Stop();
            debounceTimer.Start();
        }

        // Methods
        private static void CheckValidity(Guna2TextBox textBox, HashSet<string> resultNames_set)
        {
            if (resultNames_set.Contains(textBox.Text) || string.IsNullOrEmpty(textBox.Text))
            {
                SetTextBoxToValid(textBox);
            }
            else
            {
                SetTextBoxToInvalid(textBox);
            }
        }
        public static void SearchBoxTextBox_KeyDown(Guna2TextBox textBox, Control controlToRemoveSearchBox, Control deselectControl, KeyEventArgs e)
        {
            List<Guna2Button> results = _searchResultBox.Controls.OfType<Guna2Button>().Where(btn => btn.Visible).ToList();
            if (results.Count == 0) { return; }

            bool isResultSelected = false;

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Tab)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].BorderThickness == 1)
                    {
                        results[i].BorderThickness = 0;
                        if (i < results.Count - 1)
                        {
                            results[i + 1].BorderThickness = 1;
                            _searchResultBox.ScrollControlIntoView(results[i + 1]);
                            isResultSelected = true;
                        }
                        else
                        {
                            results[0].BorderThickness = 1;
                            _searchResultBox.ScrollControlIntoView(results[0]);
                        }
                        break;
                    }
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].BorderThickness == 1)
                    {
                        results[i].BorderThickness = 0;
                        if (i > 0)
                        {
                            results[i - 1].BorderThickness = 1;
                            _searchResultBox.ScrollControlIntoView(results[i - 1]);
                            isResultSelected = true;
                        }
                        else
                        {
                            results[^1].BorderThickness = 1;
                            _searchResultBox.ScrollControlIntoView(results[^1]);
                        }
                        break;
                    }
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                foreach (Guna2Button btn in results)
                {
                    if (btn.BorderThickness == 1)
                    {
                        textBox.Text = btn.Text;
                        CloseSearchBox(controlToRemoveSearchBox);
                        deselectControl.Focus();
                        debounceTimer.Stop();
                        isResultSelected = true;
                        break;
                    }
                }
            }

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                e.SuppressKeyPress = true;
            }

            if (!isResultSelected)
            {
                results[0].BorderThickness = 1;
                _searchResultBox.ScrollControlIntoView(results[0]);
            }
        }
        private static void SetTextBoxToInvalid(Guna2TextBox gTextBox)
        {
            gTextBox.BorderColor = CustomColors.accent_red;
            gTextBox.HoverState.BorderColor = CustomColors.accent_red;
            gTextBox.FocusedState.BorderColor = CustomColors.accent_red;
            gTextBox.Tag = "0";
        }
        private static void SetTextBoxToValid(Guna2TextBox gTextBox)
        {
            gTextBox.BorderColor = CustomColors.controlBorder;
            gTextBox.HoverState.BorderColor = CustomColors.accent_blue;
            gTextBox.FocusedState.BorderColor = CustomColors.accent_blue;
            gTextBox.Tag = "1";
        }
        public static void CloseSearchBox(Control controlToRemoveSearchBox)
        {
            controlToRemoveSearchBox.Controls.Remove(_searchResultBoxContainer);
        }
        public static void AllowTabAndEnterKeysInTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Tab || e.KeyData == Keys.Enter)
            {
                e.IsInputKey = true;
            }
        }
        private static void SetSearchBoxLocation(Guna2TextBox textBox)
        {
            Point location = textBox.Location;
            Control parent = textBox.Parent;

            while (parent != null && parent != controlToAddSearchBox)
            {
                location.Offset(parent.Location);
                parent = parent.Parent;
            }

            if (parent == controlToAddSearchBox)
            {
                _searchResultBoxContainer.Location = new Point(location.X, location.Y + textBox.Height);
            }
        }
    }
}