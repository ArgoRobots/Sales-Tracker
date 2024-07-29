using Guna.UI2.WinForms;
using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace Sales_Tracker.Classes
{
    public class SearchBox
    {
        public static Guna2Panel SearchResultBox { get; set; }
        public static Guna2Panel SearchResultBoxContainer { get; set; }

        private static Timer debounceTimer;

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

            debounceTimer = new Timer
            {
                Interval = 300
            };
            debounceTimer.Tick += DebounceTimer_Tick;
        }

        public class SearchResult(string name, Image flag, int score)
        {
            public string Name { get; set; } = name;
            public Image Flag { get; set; } = flag;
            public int Score { get; set; } = score;
        }

        public const string addLine = "ADD LINE CONTROL";

        private static Control controlToAddSearchBox;
        private static Guna2TextBox searchTextBox;
        private static List<SearchResult> resultList;
        private static Control deselectControl;
        private static int maxHeight;

        private static void DebounceTimer_Tick(object sender, EventArgs e)
        {
            debounceTimer.Stop();
            ShowSearchBox(controlToAddSearchBox, searchTextBox, resultList, deselectControl, maxHeight);
        }

        public static void ShowSearchBox(Control controlToAddBox, Guna2TextBox textBox, List<SearchResult> result_list, Control deselectControl, int maxHeight)
        {
            controlToAddSearchBox = controlToAddBox;
            UI.CloseAllPanels(null, null);

            if (result_list.Count == 0)
            {
                CloseSearchBox(controlToAddSearchBox);
                return;
            }

            SearchResultBox.SuspendLayout();
            SearchResultBox.Controls.Clear();

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

            // Add results to SearchResultBox
            int yOffset = 1;

            foreach (SearchResult meta in metaList)
            {
                if (meta.Name == addLine)
                {
                    Guna2Separator separator = UI.ConstructSeperator(CalculateControlWidth(metaList.Count), SearchResultBox);
                    separator.Location = new Point(10, yOffset + 12);
                }
                else
                {
                    Guna2Button gBtn = new()
                    {
                        Text = meta.Name,
                        Size = new Size(CalculateControlWidth(metaList.Count), 24),
                        Location = new Point(1, yOffset),
                        Font = new Font("Segoe UI", 10),
                        FillColor = CustomColors.controlBack,
                        ForeColor = CustomColors.text,
                        BorderColor = CustomColors.accent_blue,
                        Image = meta.Flag,
                        ImageAlign = HorizontalAlignment.Left,
                        ImageSize = new Size(25, 13),
                        TextAlign = HorizontalAlignment.Left
                    };
                    gBtn.Click += (sender, e) =>
                    {
                        textBox.Text = gBtn.Text;
                        CloseSearchBox(controlToAddSearchBox);
                        deselectControl.Focus();
                        debounceTimer.Stop();
                    };
                    SearchResultBox.Controls.Add(gBtn);
                }
                yOffset += 24;
            }

            int totalHeight = yOffset + 1;
            if (totalHeight > maxHeight)
            {
                SearchResultBoxContainer.Height = maxHeight + 3;
                SearchResultBox.Height = maxHeight;
                SearchResultBox.AutoScroll = true;
                Theme.CustomizeScrollBar(SearchResultBox);
            }
            else if (SearchResultBox.Controls.Count == 0)
            {
                CloseSearchBox(controlToAddSearchBox);
                debounceTimer.Stop();
                return;
            }
            else
            {
                SearchResultBox.Height = totalHeight;
                SearchResultBoxContainer.Height = totalHeight + 10;
                SearchResultBox.AutoScroll = false;
            }

            // Show search box
            SetSearchBoxLocation(textBox);
            controlToAddSearchBox.Controls.Add(SearchResultBoxContainer);
            SearchResultBox.ResumeLayout();
            SearchResultBoxContainer.BringToFront();
        }
        private static int CalculateControlWidth(int count)
        {
            if (count > 12)
            {
                return SearchResultBox.Width - SystemInformation.VerticalScrollBarWidth - 4;
            }
            else
            {
                return SearchResultBox.Width - 2;
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

            List<string> names = resultList.Select(result => result.Name).ToList();
            CheckValidity(searchTextBox, names);

            debounceTimer.Stop();
            debounceTimer.Start();
        }

        public static void CheckValidity(Guna2TextBox textBox, List<string> resultNames_list)
        {
            if (resultNames_list.Contains(textBox.Text) || string.IsNullOrEmpty(textBox.Text))
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
            List<Guna2Button> results = SearchResultBox.Controls.OfType<Guna2Button>().ToList();
            if (results.Count == 0) { return; }

            bool isResultSelected = false;

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Tab)
            {
                SelectNextResult(results, ref isResultSelected);
            }
            else if (e.KeyCode == Keys.Up)
            {
                SelectPreviousResult(results, ref isResultSelected);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                SelectResultByEnterKey(textBox, results, controlToRemoveSearchBox, deselectControl, ref isResultSelected);
            }

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                e.SuppressKeyPress = true;
            }

            if (!isResultSelected)
            {
                SelectFirstResult(results);
            }
        }
        private static void SelectNextResult(List<Guna2Button> results, ref bool isResultSelected)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].BorderThickness == 1)
                {
                    results[i].BorderThickness = 0;
                    if (i < results.Count - 1)
                    {
                        results[i + 1].BorderThickness = 1;
                        SearchResultBox.ScrollControlIntoView(results[i + 1]);
                        isResultSelected = true;
                    }
                    else
                    {
                        results[0].BorderThickness = 1;
                        SearchResultBox.ScrollControlIntoView(results[0]);
                    }
                    break;
                }
            }
        }
        private static void SelectPreviousResult(List<Guna2Button> results, ref bool isResultSelected)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].BorderThickness == 1)
                {
                    results[i].BorderThickness = 0;
                    if (i > 0)
                    {
                        results[i - 1].BorderThickness = 1;
                        SearchResultBox.ScrollControlIntoView(results[i - 1]);
                        isResultSelected = true;
                    }
                    else
                    {
                        results[^1].BorderThickness = 1;
                        SearchResultBox.ScrollControlIntoView(results[^1]);
                    }
                    break;
                }
            }
        }
        private static void SelectResultByEnterKey(Guna2TextBox textBox, List<Guna2Button> results, Control controlToRemoveSearchBox, Control deselectControl, ref bool isResultSelected)
        {
            foreach (Guna2Button btn in results)
            {
                if (btn.BorderThickness == 1)
                {
                    textBox.Text = btn.Text;
                    CloseSearchBox(controlToRemoveSearchBox);
                    deselectControl.Focus();
                    isResultSelected = true;
                    debounceTimer.Stop();
                    break;
                }
            }
        }
        private static void SelectFirstResult(List<Guna2Button> results)
        {
            results[0].BorderThickness = 1;
            SearchResultBox.ScrollControlIntoView(results[0]);
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
            controlToRemoveSearchBox.Controls.Remove(SearchResultBoxContainer);
            SearchResultBox.Controls.Clear();
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
                SearchResultBoxContainer.Location = new Point(location.X, location.Y + textBox.Height);
            }
        }
    }
}