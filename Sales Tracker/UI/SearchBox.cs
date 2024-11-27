using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.DataClasses;
using Sales_Tracker.Settings;
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
        private static Guna2Panel _searchResultBox;
        private static Guna2Panel _searchResultBoxContainer;
        private static Timer debounceTimer;
        private static Label noResults_Label;
        public const string addLine = "ADD LINE CONTROL";
        private static Control _searchBoxParent;
        private static Guna2TextBox searchTextBox;
        private static List<SearchResult> resultList;
        private static int _maxHeight;
        private static bool _increaseWidth, _translateText, _allowTextBoxEmpty, _sortAlphabetically;
        private static byte extraWidth = 200;

        // Getters
        public static Guna2Panel SearchResultBoxContainer => _searchResultBoxContainer;
        public static Guna2Panel SearchResultBox => _searchResultBox;

        /// <summary>
        /// Attaches events to a Guna2TextBox to add a SearchBox.
        /// </summary>
        public static void Attach(Guna2TextBox textBox, Control searchBoxParent, Func<List<SearchResult>> results,
            int maxHeight, bool increaseWidth, bool translateText, bool allowTextBoxEmpty, bool sortAlphabetically)
        {
            if (!translateText)
            {
                textBox.AccessibleDescription = AccessibleDescriptionStrings.DoNotTranslate;
            }
            else
            {
                noResults_Label.Text = _translateText ? LanguageManager.TranslateSingleString("No results") : "No results";
            }

            textBox.Click += (_, _) => ShowSearchBox(searchBoxParent, textBox, results, maxHeight, false, increaseWidth, translateText, allowTextBoxEmpty, sortAlphabetically);
            textBox.GotFocus += (_, _) =>
            {
                if (Settings_Form.Instance != null && !Settings_Form.Instance.IsFormClosing)  // This fixes a bug
                {
                    ShowSearchBox(searchBoxParent, textBox, results, maxHeight, false, increaseWidth, translateText, allowTextBoxEmpty, sortAlphabetically);
                    Settings_Form.Instance.IsFormClosing = false;
                }
            };
            textBox.TextChanged += SearchTextBoxChanged;
            textBox.PreviewKeyDown += AllowTabAndEnterKeysInTextBox_PreviewKeyDown;
            textBox.KeyDown += (sender, e) => SearchBoxTextBox_KeyDown(e);
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
                FillColor = CustomColors.ControlBack,
                BorderRadius = 1,
                UseTransparentBackground = true
            };
            _searchResultBox = new Guna2Panel
            {
                Location = new Point(1, 1),
                FillColor = CustomColors.ControlBack
            };
            _searchResultBox.HorizontalScroll.Enabled = false;
            _searchResultBox.HorizontalScroll.Maximum = 0;
            _searchResultBoxContainer.Controls.Add(_searchResultBox);
            Theme.CustomizeScrollBar(_searchResultBox);

            debounceTimer = new Timer
            {
                Interval = 300
            };
            debounceTimer.Tick += DebounceTimer_Tick;

            InitNoResultsLabel();
        }
        private static void InitNoResultsLabel()
        {
            noResults_Label = new()
            {
                Text = "No results",
                Height = 30,
                ForeColor = CustomColors.Text,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        // Event handlers
        private static void DebounceTimer_Tick(object sender, EventArgs e)
        {
            debounceTimer.Stop();
            ShowSearchBox(_searchBoxParent, searchTextBox, () => resultList, _maxHeight, true, _increaseWidth, _translateText, _allowTextBoxEmpty, _sortAlphabetically);
        }

        // Main methods
        private static void ShowSearchBox(Control searchBoxParent, Guna2TextBox textBox, Func<List<SearchResult>> resultsFunc,
            int maxHeight, bool alwaysShow, bool increaseWidth, bool translateText, bool allowTextBoxEmpty, bool sortAlphabetically)
        {
            // Check if the search box is already shown for the same text box
            if (searchTextBox == textBox && !alwaysShow
                && _searchBoxParent.Controls.Contains(_searchResultBoxContainer))
            {
                return;
            }

            CustomControls.CloseAllPanels(null, null);
            long startTime = DateTime.Now.Ticks;

            List<SearchResult> results = resultsFunc();
            if (translateText)
            {
                foreach (SearchResult result in results.Where(r => r.Name != addLine))
                {
                    result.DisplayName = LanguageManager.TranslateSingleString(result.Name);
                }
            }

            _searchBoxParent = searchBoxParent;
            searchTextBox = textBox;
            resultList = results;
            _maxHeight = maxHeight;
            _increaseWidth = increaseWidth;
            _translateText = translateText;
            _allowTextBoxEmpty = allowTextBoxEmpty;
            _sortAlphabetically = sortAlphabetically;

            if (results.Count == 0)
            {
                CloseSearchBox();
                return;
            }

            _searchResultBox.SuspendLayout();
            _searchResultBox.VerticalScroll.Value = 0;
            searchResultControls.ForEach(c => c.Visible = false);

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
                    foreach (SearchResult? result in results.Where(r => r.Name != addLine))
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
                if (meta.Name == addLine)
                {
                    Guna2Separator separator;
                    if (controlIndex < searchResultControls.Count && searchResultControls[controlIndex] is Guna2Separator existingSeparator)
                    {
                        separator = existingSeparator;
                    }
                    else
                    {
                        separator = CustomControls.ConstructSeperator(CalculateControlWidth(metaList.Count, textBox, increaseWidth), _searchResultBox);
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
                            searchTextBox.Text = button.Text;
                            debounceTimer.Stop();
                            CloseSearchBox();
                        };

                        int padding = btn.Image != null ? 25 : 0;
                        int availableWidth = btn.Width - padding;
                        btn.Text = Tools.AddEllipsisToString(btn.Text, btn.Font, availableWidth);

                        _searchResultBox.Controls.Add(btn);
                        searchResultControls.Add(btn);
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

            // Set width to match textBox
            int containerWidth = textBox.Width + (increaseWidth ? extraWidth : 0);
            _searchResultBoxContainer.Width = containerWidth;
            _searchResultBox.Width = containerWidth - 3;

            int totalHeight = yOffset + 1;
            if (totalHeight > maxHeight)
            {
                _searchResultBox.Controls.Remove(noResults_Label);

                _searchResultBoxContainer.Height = maxHeight + 3;
                _searchResultBox.Height = maxHeight;
            }
            else if (controlIndex == 0)
            {
                noResults_Label.Width = _searchResultBox.Width;
                _searchResultBox.Controls.Add(noResults_Label);

                _searchResultBox.Height = 50;
                _searchResultBoxContainer.Height = _searchResultBox.Height + 10;
            }
            else
            {
                _searchResultBox.Controls.Remove(noResults_Label);

                _searchResultBox.Height = totalHeight;
                _searchResultBoxContainer.Height = totalHeight + 10;
            }

            // Show search box
            SetSearchBoxLocation(textBox);
            if (!_searchBoxParent.Controls.Contains(_searchResultBoxContainer))
            {
                _searchBoxParent.Controls.Add(_searchResultBoxContainer);
            }
            _searchResultBox.ResumeLayout();
            _searchResultBoxContainer.BringToFront();

            // This fixes the scrollbar thumb size
            _searchResultBox.AutoScroll = false;
            _searchResultBox.AutoScroll = true;

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
        private static void SearchTextBoxChanged(object sender, EventArgs e)
        {
            if (resultList == null) { return; }

            HashSet<string> names = new(resultList.Select(result => result.DisplayName));
            CheckValidity(searchTextBox, names);

            debounceTimer.Stop();
            debounceTimer.Start();
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
        }
    }
}