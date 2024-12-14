using Guna.UI2.WinForms;
using Sales_Tracker.DataClasses;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Manages keyboard shortcuts and text manipulation functionality for Guna2TextBox controls.
    /// Provides undo/redo capability and clipboard operations.
    /// </summary>
    internal class TextBoxManager
    {
        // Dictionaries to hold undo/redo stacks and flags for each TextBox
        private static readonly Dictionary<Guna2TextBox, Stack<TextState>> undoStacks = [];
        private static readonly Dictionary<Guna2TextBox, Stack<TextState>> redoStacks = [];
        private static readonly Dictionary<Guna2TextBox, bool> isTextChangedByUserFlags = [];
        private const byte maxStackSize = 250;

        /// <summary>
        /// Represents the state of a TextBox, including its text content and cursor position
        /// </summary>
        private class TextState(string text, int cursorPosition)
        {
            private readonly string _text = text ?? "";
            private readonly int _cursorPosition = cursorPosition >= 0 ? cursorPosition : 0;

            public string Text => _text;
            public int CursorPosition => _cursorPosition;
        }

        // Main methods
        /// <summary>
        /// Attaches keyboard shortcut functionality (copy, paste, undo, redo) and other custom behavior to a Guna2TextBox.
        /// </summary>
        /// <param name="textBoxes">One or more Guna2TextBox controls to attach functionality to.</param>
        public static void Attach(params Guna2TextBox[] textBoxes)
        {
            foreach (Guna2TextBox textBox in textBoxes)
            {
                if (IsAttached(textBox)) { continue; }

                InitializeTextBox(textBox);
                AttachEventHandlers(textBox);
                TextBoxTooltip.SetOverflowTooltip(textBox);
            }
        }

        /// <summary>
        /// Initializes the undo/redo stacks and flags for a TextBox.
        /// </summary>
        private static void InitializeTextBox(Guna2TextBox textBox)
        {
            undoStacks[textBox] = new Stack<TextState>();
            redoStacks[textBox] = new Stack<TextState>();
            isTextChangedByUserFlags[textBox] = true;
            undoStacks[textBox].Push(new TextState(textBox.Text, textBox.SelectionStart));
        }
        private static void AttachEventHandlers(Guna2TextBox textBox)
        {
            textBox.TextChanged += TextBox_TextChanged;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            textBox.KeyDown += TextBox_KeyDown;
            textBox.MouseClick += TextBox_MouseClick;
        }
        private static bool IsAttached(Guna2TextBox textBox) => undoStacks.ContainsKey(textBox);

        // TextBox event handlers
        /// <summary>
        /// Handles text changes and manages undo/redo stacks.
        /// </summary>
        private static void TextBox_TextChanged(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;

            if (!isTextChangedByUserFlags[textBox]) { return; }

            Stack<TextState> undoStack = undoStacks[textBox];
            Stack<TextState> redoStack = redoStacks[textBox];

            if (undoStack.Count == 0 || undoStack.Peek().Text != textBox.Text)
            {
                UpdateUndoStack(textBox, undoStack);
                redoStack.Clear();
            }
        }

        /// <summary>
        /// Handles PreviewKeyDown event to ensure keyboard shortcuts work properly.
        /// </summary>
        private static void TextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.A || e.KeyCode == Keys.C || e.KeyCode == Keys.X ||
                            e.KeyCode == Keys.V || e.KeyCode == Keys.Z || e.KeyCode == Keys.Y))
            {
                e.IsInputKey = true;
            }
        }

        /// Handles KeyDown event for keyboard shortcuts and text manipulation.
        /// </summary>
        private static void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Block the '>' symbol
            if ((e.KeyCode == Keys.OemPeriod || e.KeyCode == Keys.Decimal) && e.Shift)
            {
                e.SuppressKeyPress = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.Y)
            {
                e.SuppressKeyPress = true;
                Guna2TextBox textBox = (Guna2TextBox)sender;
                Redo(textBox);
                return;
            }

            if (!IsInputKey(e.KeyCode) && !e.Control)
            {
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
            }
            else
            {
                Guna2TextBox textBox = (Guna2TextBox)sender;
                if (e.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Z when e.Shift:
                            Redo(textBox);
                            break;
                        case Keys.Z:
                            Undo(textBox);
                            break;
                    }
                }
            }
        }
        private static void TextBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Guna2TextBox textBox = (Guna2TextBox)sender;
                ShowRightClickMenu(textBox, e.Location);
            }
        }

        // Methods for event handlers
        private static bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Tab:
                case Keys.Enter:
                case Keys.Escape:
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Delete:
                case Keys.Back:
                case Keys.Home:
                case Keys.End:
                case Keys.Subtract:
                case Keys.OemMinus:
                case Keys.Decimal:
                case Keys.OemPeriod:
                    return true;
            }

            char keyChar = (char)keyData;
            return char.IsLetterOrDigit(keyChar) ||
                   char.IsPunctuation(keyChar) ||
                   char.IsWhiteSpace(keyChar);
        }

        // Stack methods (undo, redo)
        /// <summary>
        /// Updates the undo stack and manages its size.
        /// </summary>
        private static void UpdateUndoStack(Guna2TextBox textBox, Stack<TextState> undoStack)
        {
            undoStack.Push(new TextState(textBox.Text, textBox.SelectionStart));

            if (undoStack.Count > maxStackSize)
            {
                LimitStackSize(textBox);
            }
        }

        /// <summary>
        /// Limits the undo stack size to prevent memory bloat.
        /// </summary>
        private static void LimitStackSize(Guna2TextBox textBox)
        {
            Stack<TextState> tempStack = new();
            Stack<TextState> currentStack = undoStacks[textBox];

            for (int i = 0; i < maxStackSize && currentStack.Count > 0; i++)
            {
                tempStack.Push(currentStack.Pop());
            }

            undoStacks[textBox] = new Stack<TextState>(tempStack.Reverse());
        }

        /// <summary>
        /// Performs undo operation if available.
        /// </summary>
        private static void Undo(Guna2TextBox textBox)
        {
            Stack<TextState> undoStack = undoStacks[textBox];
            Stack<TextState> redoStack = redoStacks[textBox];

            if (undoStack.Count > 1)
            {
                redoStack.Push(undoStack.Pop());
                TextState state = undoStack.Peek();
                UpdateTextBoxContent(textBox, state.Text, state.CursorPosition);
            }
        }

        /// <summary>
        /// Performs redo operation if available.
        /// </summary>
        private static void Redo(Guna2TextBox textBox)
        {
            Stack<TextState> undoStack = undoStacks[textBox];
            Stack<TextState> redoStack = redoStacks[textBox];

            if (redoStack.Count > 0)
            {
                TextState state = redoStack.Pop();
                undoStack.Push(state);
                UpdateTextBoxContent(textBox, state.Text, state.CursorPosition);
            }
        }

        /// <summary>
        /// Updates TextBox content without triggering TextChanged event.
        /// </summary>
        private static void UpdateTextBoxContent(Guna2TextBox textBox, string newText, int cursorPosition)
        {
            isTextChangedByUserFlags[textBox] = false;
            textBox.Text = newText;
            textBox.SelectionStart = cursorPosition;
            isTextChangedByUserFlags[textBox] = true;
        }

        // Right click menu
        private static Guna2Panel _rightClickTextBox_Panel;
        public static Guna2Panel RightClickTextBox_Panel => _rightClickTextBox_Panel;
        public static void ConstructRightClickTextBoxMenu()
        {
            _rightClickTextBox_Panel = CustomControls.ConstructPanelForMenu(
                new Size(CustomControls.PanelWidth, 6 * CustomControls.PanelButtonHeight + CustomControls.SpaceForPanel),
                "rightClickTextBox_Panel"
            );

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)_rightClickTextBox_Panel.Controls[0];

            Guna2Button selectAllBtn = CustomControls.ConstructBtnForMenu("Select all", CustomControls.PanelBtnWidth, false, flowPanel);
            selectAllBtn.Click += HandleSelectAll;
            CustomControls.ConstructKeyShortcut("Ctrl+A", selectAllBtn);

            Guna2Button copyBtn = CustomControls.ConstructBtnForMenu("Copy", CustomControls.PanelBtnWidth, false, flowPanel);
            copyBtn.Click += HandleCopy;
            CustomControls.ConstructKeyShortcut("Ctrl+C", copyBtn);

            Guna2Button pasteBtn = CustomControls.ConstructBtnForMenu("Paste", CustomControls.PanelBtnWidth, false, flowPanel);
            pasteBtn.Click += HandlePaste;
            CustomControls.ConstructKeyShortcut("Ctrl+V", pasteBtn);

            Guna2Button cutBtn = CustomControls.ConstructBtnForMenu("Cut", CustomControls.PanelBtnWidth, false, flowPanel);
            cutBtn.Click += HandleCut;
            CustomControls.ConstructKeyShortcut("Ctrl+X", cutBtn);

            Guna2Button undoBtn = CustomControls.ConstructBtnForMenu("Undo", CustomControls.PanelBtnWidth, false, flowPanel);
            undoBtn.Click += HandleUndo;
            CustomControls.ConstructKeyShortcut("Ctrl+Z", undoBtn);

            Guna2Button redoBtn = CustomControls.ConstructBtnForMenu("Redo", CustomControls.PanelBtnWidth, false, flowPanel);
            redoBtn.Click += HandleRedo;
            CustomControls.ConstructKeyShortcut("Ctrl+Y", redoBtn);
        }
        private static void HandleCopy(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)_rightClickTextBox_Panel.Tag;
            if (textBox.SelectedText != "")
            {
                Clipboard.SetText(textBox.SelectedText);
            }
            CustomControls.CloseAllPanels(null, null);
        }
        private static void HandlePaste(object sender, EventArgs e)
        {
            string clipboardText = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboardText)) { return; }

            Guna2TextBox textBox = (Guna2TextBox)_rightClickTextBox_Panel.Tag;
            int cursorPosition = textBox.SelectionStart;
            string text = textBox.Text;

            if (textBox.SelectionLength > 0)
            {
                // Replace selected text
                text = text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            }

            text = text.Insert(cursorPosition, clipboardText);
            textBox.Text = text;
            textBox.SelectionStart = cursorPosition + clipboardText.Length;

            CustomControls.CloseAllPanels(null, null);
        }
        private static void HandleCut(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)_rightClickTextBox_Panel.Tag;
            if (textBox.SelectedText == "") { return; }

            Clipboard.SetText(textBox.SelectedText);
            int cursorPosition = textBox.SelectionStart;
            textBox.Text = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            textBox.SelectionStart = cursorPosition;

            CustomControls.CloseAllPanels(null, null);
        }
        private static void HandleSelectAll(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)_rightClickTextBox_Panel.Tag;
            textBox.SelectAll();
            CustomControls.CloseAllPanels(null, null);
        }
        private static void HandleUndo(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)_rightClickTextBox_Panel.Tag;
            Undo(textBox);
            CustomControls.CloseAllPanels(null, null);
        }
        private static void HandleRedo(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)_rightClickTextBox_Panel.Tag;
            Redo(textBox);
            CustomControls.CloseAllPanels(null, null);
        }

        // Methods for right click menu
        private static void ShowRightClickMenu(Guna2TextBox textBox, Point mouseLocation)
        {
            // Convert mouse coordinates to screen coordinates
            Point screenPoint = textBox.PointToScreen(mouseLocation);
            screenPoint.X -= ReadOnlyVariables.OffsetRightClickPanel;

            // Ensure the panel doesn't go off screen
            Rectangle screenBounds = Screen.FromControl(textBox).Bounds;
            if (screenPoint.X + _rightClickTextBox_Panel.Width > screenBounds.Right)
            {
                screenPoint.X = screenBounds.Right - _rightClickTextBox_Panel.Width - ReadOnlyVariables.OffsetRightClickPanel;
            }
            if (screenPoint.Y + _rightClickTextBox_Panel.Height > screenBounds.Bottom)
            {
                screenPoint.Y = screenBounds.Bottom - _rightClickTextBox_Panel.Height;
            }

            // Show the panel at the calculated position
            _rightClickTextBox_Panel.Tag = textBox;
            textBox.FindForm().Controls.Add(_rightClickTextBox_Panel);
            _rightClickTextBox_Panel.Location = textBox.Parent.PointToClient(screenPoint);
            _rightClickTextBox_Panel.BringToFront();
        }
    }
}