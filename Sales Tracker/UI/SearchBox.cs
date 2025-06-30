using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Theme;
using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Provides functionality for creating and managing an interactive search box with auto-complete capabilities.
    /// The search box supports keyboard navigation and debounced search.
    /// </summary>
    public class SearchBox
    {
        // Properties
        private static Label _noResults_Label;
        private static Control _searchBoxParent;
        private static Guna2TextBox _searchTextBox;
        private static List<SearchResult> _resultList;
        private static int _maxHeight;
        private static bool _increaseWidth, _translateText, _allowTextBoxEmpty, _sortAlphabetically;
        private static readonly short extraWidth = 350;
        public const string AddLine = "ADD LINE CONTROL";

        // Getters
        public static Guna2Panel SearchResultBoxContainer { get; private set; }
        public static Guna2Panel SearchResultBox { get; private set; }
        public static Timer DebounceTimer { get; private set; }

        /// <summary>
        /// Attaches events to a Guna2TextBox to add a SearchBox.
        /// </summary>
        public static void Attach(Guna2TextBox textBox, Control searchBoxParent, Func<List<SearchResult>> results,
            int maxHeight, bool increaseWidth, bool translateText, bool allowTextBoxEmpty, bool sortAlphabetically)
        {
            if (!translateText)
            {
                textBox.AccessibleDescription = AccessibleDescriptionManager.DoNotTranslate;
            }
            else
            {
                _noResults_Label.Text = _translateText ? LanguageManager.TranslateString("No results") : "No results";
            }

            textBox.MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ShowSearchBox(searchBoxParent, textBox, results, maxHeight, false, increaseWidth, translateText, allowTextBoxEmpty, sortAlphabetically);
                }
            };
            textBox.TextChanged += SearchTextBoxChanged;
            textBox.PreviewKeyDown += AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            textBox.KeyDown += (sender, e) => SearchTextBox_KeyDown(e);
        }

        // List to store and reuse controls
        public static List<Control> SearchResultControls { get; } = [];

        // Init.
        public static void ConstructSearchBox()
        {
            SearchResultBoxContainer = new Guna2Panel
            {
                BorderThickness = 1,
                BorderStyle = DashStyle.Solid,
                BorderColor = Color.Gray,
                FillColor = CustomColors.ControlBack
            };
            SearchResultBox = new Guna2Panel
            {
                Location = new Point(1, 1),
                FillColor = CustomColors.ControlBack
            };
            SearchResultBox.HorizontalScroll.Enabled = false;
            SearchResultBox.HorizontalScroll.Maximum = 0;
            SearchResultBoxContainer.Controls.Add(SearchResultBox);
            ThemeManager.CustomizeScrollBar(SearchResultBox);

            DebounceTimer = new Timer
            {
                Interval = 300
            };
            DebounceTimer.Tick += DebounceTimer_Tick;

            _noResults_Label = new()
            {
                Text = "No results",
                Height = 30,
                ForeColor = CustomColors.Text,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = CustomColors.ControlBack
            };
        }

        // Event handlers
        private static void DebounceTimer_Tick(object sender, EventArgs e)
        {
            DebounceTimer.Stop();
            ShowSearchBox(_searchBoxParent, _searchTextBox, () => _resultList, _maxHeight, true, _increaseWidth, _translateText, _allowTextBoxEmpty, _sortAlphabetically);
        }

        // Main methods
        private static void ShowSearchBox(Control searchBoxParent, Guna2TextBox textBox, Func<List<SearchResult>> resultsFunc,
            int maxHeight, bool alwaysShow, bool increaseWidth, bool translateText, bool allowTextBoxEmpty, bool sortAlphabetically)
        {
            // Check if the search box is already shown for the same text box
            if (_searchTextBox == textBox && !alwaysShow
                && _searchBoxParent.Controls.Contains(SearchResultBoxContainer))
            {
                return;
            }

            long startTime = DateTime.Now.Ticks;

            List<SearchResult> results = resultsFunc();
            if (translateText)
            {
                foreach (SearchResult result in results.Where(r => r.Name != AddLine))
                {
                    result.DisplayName = LanguageManager.TranslateString(result.Name);
                }
            }

            if (results.Count == 0)
            {
                return;
            }

            if (!alwaysShow)
            {
                CustomControls.CloseAllPanels();
            }

            _searchBoxParent = searchBoxParent;
            _searchTextBox = textBox;
            _resultList = results;
            _maxHeight = maxHeight;
            _increaseWidth = increaseWidth;
            _translateText = translateText;
            _allowTextBoxEmpty = allowTextBoxEmpty;
            _sortAlphabetically = sortAlphabetically;

            SearchResultBox.SuspendLayout();
            SearchResultBox.VerticalScroll.Value = 0;
            SearchResultControls.ForEach(c => c.Visible = false);

            string searchText = textBox.Text;
            List<SearchResult> metaList = [];

            // Set scores
            if (string.IsNullOrEmpty(searchText))
            {
                if (sortAlphabetically)
                {
                    metaList.AddRange(results
                       .Select(r => new SearchResult(r.DisplayName, r.Flag, 0))
                       .OrderBy(r => r.DisplayName));  // Sort alphabetically
                }
                else
                {
                    metaList.AddRange(results.Select(r => new SearchResult(r.DisplayName, r.Flag, 0)));
                }
            }
            else
            {
                string[] searchTerms = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(term => term.Trim().ToLower())
                    .Where(term => !string.IsNullOrEmpty(term))
                    .ToArray();

                if (searchTerms.Length != 0)
                {
                    foreach (SearchResult? result in results.Where(r => r.Name != AddLine))
                    {
                        string displayNameLower = result.DisplayName.ToLower();
                        int score = 0;

                        foreach (string term in searchTerms)
                        {
                            if (!displayNameLower.Contains(term)) { break; }
                            score += CalculateTermScore(displayNameLower, term);
                        }

                        if (score > 0)
                        {
                            metaList.Add(new SearchResult(result.DisplayName, result.Flag, score));
                        }
                    }
                }
            }

            metaList = metaList.OrderByDescending(x => x.Score).ToList();

            // Add results to _searchResultBox
            int yOffset = 1;
            int buttonHeight = 35;
            int controlIndex = 0;

            // Construct buttons
            foreach (SearchResult meta in metaList)
            {
                if (meta.Name == AddLine)
                {
                    Guna2Separator separator;
                    if (controlIndex < SearchResultControls.Count && SearchResultControls[controlIndex] is Guna2Separator existingSeparator)
                    {
                        separator = existingSeparator;
                    }
                    else
                    {
                        separator = CustomControls.ConstructSeperator(CalculateControlWidth(metaList.Count, textBox, increaseWidth), SearchResultBox);
                        SearchResultBox.Controls.Add(separator);
                        SearchResultControls.Add(separator);
                    }
                    separator.Visible = true;
                    separator.Location = new Point(10, yOffset + 12);
                }
                else
                {
                    Guna2Button btn;
                    if (controlIndex < SearchResultControls.Count && SearchResultControls[controlIndex] is Guna2Button existingButton)
                    {
                        btn = existingButton;
                    }
                    else
                    {
                        btn = new Guna2Button
                        {
                            Font = new Font("Segoe UI", 10),
                            FillColor = CustomColors.ControlBack,
                            ForeColor = CustomColors.Text,
                            Height = buttonHeight,
                            Left = 1,
                            BorderColor = CustomColors.AccentBlue,
                            ImageAlign = HorizontalAlignment.Left,
                            ImageSize = new Size(25, 13),  // Country flags have different ratios. This is just a good size
                            TextAlign = HorizontalAlignment.Left
                        };
                        btn.Click += (sender, e) =>
                        {
                            Guna2Button button = (Guna2Button)sender;
                            _searchTextBox.Text = button.Text;
                            DebounceTimer.Stop();
                            CustomControls.CloseAllPanels();
                        };

                        int padding = btn.Image != null ? 25 : 0;
                        int availableWidth = btn.Width - padding;
                        btn.Text = Tools.AddEllipsisToString(btn.Text, btn.Font, availableWidth);

                        SearchResultBox.Controls.Add(btn);
                        SearchResultControls.Add(btn);
                    }
                    btn.Text = meta.DisplayName;
                    btn.Width = CalculateControlWidth(metaList.Count, textBox, increaseWidth);
                    btn.Top = yOffset;
                    btn.Image = meta.Flag;
                    btn.Visible = true;
                    btn.BorderThickness = 0;
                }
                yOffset += buttonHeight;
                controlIndex++;
            }

            // Set width
            int containerWidth = textBox.Width + (increaseWidth ? extraWidth : 0);
            SearchResultBoxContainer.Width = containerWidth;
            SearchResultBox.Width = containerWidth - 3;

            int totalHeight = yOffset + 1;
            if (totalHeight > maxHeight)
            {
                SearchResultBox.Controls.Remove(_noResults_Label);

                SearchResultBoxContainer.Height = maxHeight + 3;
                SearchResultBox.Height = maxHeight;
                SearchResultBox.AutoScroll = true;
            }
            else if (controlIndex == 0)
            {
                _noResults_Label.Width = SearchResultBox.Width;
                SearchResultBox.Controls.Add(_noResults_Label);

                SearchResultBox.Height = 50;
                SearchResultBoxContainer.Height = SearchResultBox.Height + 10;
                SearchResultBox.AutoScroll = false;
            }
            else
            {
                SearchResultBox.Controls.Remove(_noResults_Label);

                SearchResultBox.Height = totalHeight;
                SearchResultBoxContainer.Height = totalHeight + 10;
                SearchResultBox.AutoScroll = false;
            }

            // Show search box
            SetSearchBoxLocation(textBox);
            if (!_searchBoxParent.Controls.Contains(SearchResultBoxContainer))
            {
                _searchBoxParent.Controls.Add(SearchResultBoxContainer);
            }
            SearchResultBox.ResumeLayout();
            SearchResultBoxContainer.BringToFront();

            // End timer
            long endTime = DateTime.Now.Ticks;

            // Calculate elapsed time in milliseconds
            double elapsedTime = (endTime - startTime) / TimeSpan.TicksPerMillisecond;
            Log.Write(1, "Elapsed time for updating the SearchBox: " + elapsedTime + " ms");
        }
        private static int CalculateTermScore(string source, string term)
        {
            int score = 1;
            string[] words = source.Split(' ');

            if (words.Contains(term)) { score += 2; }
            if (source.StartsWith(term)) { score += 3; }
            if (words.Any(word => word.StartsWith(term))) { score += 1; }

            return score;
        }
        private static int CalculateControlWidth(int count, Guna2TextBox textBox, bool increaseWidth)
        {
            int baseWidth = textBox.Width + (increaseWidth ? extraWidth : 0);
            if (count > 12)
            {
                return baseWidth - SystemInformation.VerticalScrollBarWidth - 4;
            }
            else
            {
                return baseWidth - 2;
            }
        }
        public static List<SearchResult> ConvertToSearchResults(List<string> names)
        {
            return names.Select(name => new SearchResult(name, null, 0)).ToList();
        }
        public static void SearchTextBoxChanged(object sender, EventArgs e)
        {
            if (_resultList == null) { return; }

            HashSet<string> names = [.. _resultList.Select(result => result.DisplayName)];
            CheckValidity(_searchTextBox, names);

            TextBoxManager.RightClickTextBox_Panel.Parent?.Controls.Remove(TextBoxManager.RightClickTextBox_Panel);

            DebounceTimer.Stop();
            DebounceTimer.Start();
        }

        // Methods
        private static void CheckValidity(Guna2TextBox textBox, HashSet<string> resultNames_set)
        {
            if (resultNames_set.Contains(textBox.Text) || string.IsNullOrEmpty(textBox.Text) && _allowTextBoxEmpty)
            {
                SetTextBoxToValid(textBox);
            }
            else
            {
                SetTextBoxToInvalid(textBox);
            }
        }
        private static void SearchTextBox_KeyDown(KeyEventArgs e)
        {
            List<Guna2Button> results = SearchResultBox.Controls.OfType<Guna2Button>().Where(btn => btn.Visible).ToList();
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
                        _searchTextBox.Text = btn.Text;
                        DebounceTimer.Stop();
                        isResultSelected = true;
                        CustomControls.CloseAllPanels();
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
            SearchResultBox.ScrollControlIntoView(control);
        }
        private static void SetTextBoxToInvalid(Guna2TextBox textBox)
        {
            textBox.BorderColor = CustomColors.AccentRed;
            textBox.HoverState.BorderColor = CustomColors.AccentRed;
            textBox.FocusedState.BorderColor = CustomColors.AccentRed;
            textBox.Tag = "0";
        }
        private static void SetTextBoxToValid(Guna2TextBox textBox)
        {
            textBox.BorderColor = CustomColors.ControlBorder;
            textBox.HoverState.BorderColor = CustomColors.AccentBlue;
            textBox.FocusedState.BorderColor = CustomColors.AccentBlue;
            textBox.Tag = "1";
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

            // Calculate absolute position relative to searchBoxParent
            while (parent != null && parent != _searchBoxParent)
            {
                location.Offset(parent.Location);
                parent = parent.Parent;
            }

            if (parent != _searchBoxParent)
            {
                return;
            }

            if (_increaseWidth)
            {
                // Center the panel relative to the textbox
                int widthDifference = SearchResultBoxContainer.Width - textBox.Width;
                int centeredX = location.X - (widthDifference / 2);

                // Ensure the panel doesn't extend beyond the parent's bounds
                if (centeredX < 0)
                {
                    centeredX = 0;
                }
                else if (centeredX + SearchResultBoxContainer.Width > _searchBoxParent.Width)
                {
                    centeredX = _searchBoxParent.Width - SearchResultBoxContainer.Width;
                }

                SearchResultBoxContainer.Location = new Point(centeredX, location.Y + textBox.Height);
            }
            else
            {
                // Align panel to the left of the TextBox
                SearchResultBoxContainer.Location = new Point(location.X, location.Y + textBox.Height);
            }
        }
        public static void CloseSearchBox()
        {
            if (IsSearchBoxOpen())
            {
                // Set focus to another control to avoid reopening the search box
                Label firstLabel = _searchBoxParent.Controls.OfType<Label>().FirstOrDefault();
                firstLabel?.Select();

                _searchBoxParent.Controls.Remove(SearchResultBoxContainer);
            }
        }
        private static bool IsSearchBoxOpen()
        {
            return _searchBoxParent != null && _searchBoxParent.Controls.Contains(SearchResultBoxContainer);
        }
    }
}