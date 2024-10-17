using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker.UI
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

        /// <summary>
        /// Attaches events to a Guna2TextBox to add a SearchBox.
        /// </summary>
        public static void Attach(Guna2TextBox textBox, Control searchBoxParent, Func<List<SearchResult>> results, int maxHeight, bool allowTextBoxEmpty = true)
        {
            textBox.Click += (sender, e) => { ShowSearchBox(searchBoxParent, textBox, results, maxHeight, allowTextBoxEmpty); };
            textBox.GotFocus += (sender, e) => { ShowSearchBox(searchBoxParent, textBox, results, maxHeight, allowTextBoxEmpty); };
            textBox.TextChanged += SearchTextBoxChanged;
            textBox.PreviewKeyDown += AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            textBox.KeyDown += (sender, e) => { SearchBoxTextBox_KeyDown(e); };
        }

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
        private static Control _searchBoxParent;
        private static Guna2TextBox searchTextBox;
        private static List<SearchResult> resultList;
        private static int maxHeight;
        private static bool allowEmpty;

        // Event handlers
        private static void DebounceTimer_Tick(object sender, EventArgs e)
        {
            debounceTimer.Stop();
            ShowSearchBox(_searchBoxParent, searchTextBox, () => resultList, maxHeight, allowEmpty, true);
        }

        // Main methods
        private static void ShowSearchBox(Control searchBoxParent, Guna2TextBox textBox, Func<List<SearchResult>> resultsFunc, int maxHeight, bool allowTextBoxEmpty, bool alwaysShow = false)
        {
            // Check if the search box is already shown for the same text box
            if (searchTextBox == textBox & !alwaysShow)
            {
                return;
            }

            List<SearchResult> results = resultsFunc();

            _searchBoxParent = searchBoxParent;
            searchTextBox = textBox;
            resultList = results;
            SearchBox.maxHeight = maxHeight;
            allowEmpty = allowTextBoxEmpty;

            // Start timer
            long startTime = DateTime.Now.Ticks;

            CustomControls.CloseAllPanels(null, null);

            if (results.Count == 0)
            {
                CloseSearchBox();
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
                foreach (SearchResult result in results)
                {
                    metaList.Add(new SearchResult(result.Name, result.Flag, 0));
                }
            }
            else
            {
                foreach (SearchResult result in results)
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
                        separator = CustomControls.ConstructSeperator(CalculateControlWidth(metaList.Count, textBox), _searchResultBox);
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
                            Height = buttonHeight,
                            Left = 1,
                            BorderColor = CustomColors.accent_blue,
                            ImageAlign = HorizontalAlignment.Left,
                            ImageSize = new Size(25, 13),  // Country flags have different ratios. This is just a good size
                            TextAlign = HorizontalAlignment.Left
                        };
                        btn.Click += (sender, e) =>
                        {
                            Guna2Button button = (Guna2Button)sender;
                            searchTextBox.Text = button.Text;
                            debounceTimer.Stop();
                            CloseSearchBox();
                        };
                        _searchResultBox.Controls.Add(btn);
                        searchResultControls.Add(btn);
                    }
                    btn.Text = meta.Name;
                    btn.Width = CalculateControlWidth(metaList.Count, textBox);
                    btn.Top = yOffset;
                    btn.Image = meta.Flag;
                    btn.Visible = true;
                    btn.BorderThickness = 0;
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
                CloseSearchBox();
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
            if (!_searchBoxParent.Controls.Contains(_searchResultBoxContainer))
            {
                _searchBoxParent.Controls.Add(_searchResultBoxContainer);
            }
            _searchResultBox.ResumeLayout();
            _searchResultBoxContainer.BringToFront();

            // End timer
            long endTime = DateTime.Now.Ticks;

            // Calculate elapsed time in milliseconds
            double elapsedTime = (endTime - startTime) / TimeSpan.TicksPerMillisecond;
            Log.Write(1, "Elapsed time for updating the SearchBox: " + elapsedTime + " ms");
        }
        private static int CalculateControlWidth(int count, Guna2TextBox textBox)
        {
            if (count > 12)
            {
                return textBox.Width - SystemInformation.VerticalScrollBarWidth - 4;
            }
            else
            {
                return textBox.Width - 2;
            }
        }
        public static List<SearchResult> ConvertToSearchResults(List<string> names)
        {
            return names.Select(name => new SearchResult(name, null, 0)).ToList();
        }
        private static void SearchTextBoxChanged(object sender, EventArgs e)
        {
            if (searchTextBox == null) { return; }

            HashSet<string> names = new(resultList.Select(result => result.Name));
            CheckValidity(searchTextBox, names);

            debounceTimer.Stop();
            debounceTimer.Start();
        }

        // Methods
        private static void CheckValidity(Guna2TextBox textBox, HashSet<string> resultNames_set)
        {
            if (resultNames_set.Contains(textBox.Text) || string.IsNullOrEmpty(textBox.Text) && allowEmpty)
            {
                SetTextBoxToValid(textBox);
            }
            else
            {
                SetTextBoxToInvalid(textBox);
            }
        }
        private static void SearchBoxTextBox_KeyDown(KeyEventArgs e)
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
                            SelectResult(results[i + 1]);
                            isResultSelected = true;
                        }
                        else
                        {
                            SelectResult(results[0]);
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
                            SelectResult(results[i - 1]);
                            isResultSelected = true;
                        }
                        else
                        {
                            SelectResult(results[^1]);
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
                        searchTextBox.Text = btn.Text;
                        debounceTimer.Stop();
                        isResultSelected = true;
                        CloseSearchBox();
                        break;
                    }
                }
            }

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                e.SuppressKeyPress = true;
            }

            // If nothing was selected, select the first result
            if (!isResultSelected)
            {
                SelectResult(results[0]);
            }
        }
        private static void SelectResult(Guna2Button control)
        {
            control.BorderThickness = 1;
            _searchResultBox.ScrollControlIntoView(control);
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
        private static void AllowTabAndEnterKeysInTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
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

            while (parent != null && parent != _searchBoxParent)
            {
                location.Offset(parent.Location);
                parent = parent.Parent;
            }

            if (parent == _searchBoxParent)
            {
                _searchResultBoxContainer.Location = new Point(location.X, location.Y + textBox.Height);
            }
        }
        public static void CloseSearchBox()
        {
            if (_searchBoxParent == null) { return; }

            if (_searchBoxParent.Controls.Contains(_searchResultBoxContainer))
            {
                // Set focus to another control to avoid reopening the search box
                Label firstLabel = _searchBoxParent.Controls.OfType<Label>().FirstOrDefault();
                firstLabel?.Select();

                _searchBoxParent.Controls.Remove(_searchResultBoxContainer);
            }

            // Reset
            searchTextBox = null;
        }
    }
}