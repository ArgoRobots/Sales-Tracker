using Guna.UI2.WinForms;
using Sales_Tracker.Classes;

namespace Sales_Tracker.UI
{
    /// <summary>
    /// Manages keyboard shortcuts and text manipulation functionality for Guna2TextBox controls.
    /// Provides undo/redo capability and clipboard operations.
    /// </summary>
    internal class TextBoxManager
    {
        // Dictionaries to hold undo/redo stacks and flags for each TextBox
        private static readonly Dictionary<Guna2TextBox, Stack<string>> undoStacks = [];
        private static readonly Dictionary<Guna2TextBox, Stack<string>> redoStacks = [];
        private static readonly Dictionary<Guna2TextBox, bool> isTextChangedByUserFlags = [];
        private const int maxStackSize = 250;

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
            }
        }

        /// <summary>
        /// Initializes the undo/redo stacks and flags for a TextBox.
        /// </summary>
        private static void InitializeTextBox(Guna2TextBox textBox)
        {
            undoStacks[textBox] = new Stack<string>();
            redoStacks[textBox] = new Stack<string>();
            isTextChangedByUserFlags[textBox] = true;
            undoStacks[textBox].Push(textBox.Text);
        }

        /// <summary>
        /// Attaches all required event handlers to the TextBox.
        /// </summary>
        private static void AttachEventHandlers(Guna2TextBox textBox)
        {
            textBox.TextChanged += TextBox_TextChanged;
            textBox.KeyDown += TextBox_KeyDown;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            textBox.Enter += Tools.MakeSureTextIsNotSelectedAndCursorIsAtEnd;
        }

        /// <summary>
        /// Checks if the TextBox already has keyboard shortcuts attached.
        /// </summary>
        private static bool IsAttached(Guna2TextBox textBox) => undoStacks.ContainsKey(textBox);

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

        // <summary>
        /// Handles KeyDown event for keyboard shortcuts and text manipulation.
        /// </summary>
        private static void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;

            if (e.Control || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;  // Remove Windows "ding" noise
            }

            if (e.Control)
            {
                if (e.Shift)
                {
                    HandleCtrlShiftSelection(textBox, e);
                }
                else if (IsArrowKey(e.KeyCode))
                {
                    HandleCtrlCursorMovement(textBox, e);
                }
                else
                {
                    HandleKeyboardShortcut(textBox, e);
                }
            }
            else if (e.Shift)
            {
                HandleShiftSelection(textBox, e);
            }
        }

        /// <summary>
        /// Checks if the key is an arrow key.
        /// </summary>
        private static bool IsArrowKey(Keys key)
        {
            return key == Keys.Left || key == Keys.Right;
        }

        /// <summary>
        /// Handles Ctrl + arrow key combinations for word-by-word cursor movement.
        /// </summary>
        private static void HandleCtrlCursorMovement(Guna2TextBox textBox, KeyEventArgs e)
        {
            string text = textBox.Text;
            int currentPos = textBox.SelectionStart;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    textBox.SelectionStart = FindPreviousWordStart(text, currentPos);
                    textBox.SelectionLength = 0;
                    e.Handled = true;
                    break;

                case Keys.Right:
                    if (textBox.SelectionLength > 0)
                    {
                        currentPos += textBox.SelectionLength;
                    }
                    textBox.SelectionStart = FindNextWordEnd(text, currentPos);
                    textBox.SelectionLength = 0;
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Handles Shift key combinations for character-level text selection.
        /// </summary>
        private static void HandleShiftSelection(Guna2TextBox textBox, KeyEventArgs e)
        {
            int selStart = textBox.SelectionStart;
            int selLength = textBox.SelectionLength;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (selStart > 0)
                    {
                        if (textBox.SelectionLength == 0)
                        {
                            textBox.SelectionStart = selStart - 1;
                            textBox.SelectionLength = 1;
                        }
                        else
                        {
                            if (selStart == textBox.SelectionStart)
                            {
                                textBox.SelectionStart = selStart - 1;
                                textBox.SelectionLength = selLength + 1;
                            }
                            else
                            {
                                textBox.SelectionLength = selLength - 1;
                            }
                        }
                    }
                    e.Handled = true;
                    break;

                case Keys.Right:
                    if (selStart < textBox.Text.Length)
                    {
                        if (textBox.SelectionLength == 0)
                        {
                            textBox.SelectionLength = 1;
                        }
                        else
                        {
                            if (selStart == textBox.SelectionStart)
                            {
                                textBox.SelectionLength = selLength - 1;
                            }
                            else
                            {
                                textBox.SelectionLength = selLength + 1;
                            }
                        }
                    }
                    e.Handled = true;
                    break;

                case Keys.Home:
                    int currentPos = textBox.SelectionStart + textBox.SelectionLength;
                    textBox.SelectionStart = 0;
                    textBox.SelectionLength = currentPos;
                    e.Handled = true;
                    break;

                case Keys.End:
                    textBox.SelectionLength = textBox.Text.Length - textBox.SelectionStart;
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Handles Ctrl+Shift key combinations for word-level text selection.
        /// </summary>
        private static void HandleCtrlShiftSelection(Guna2TextBox textBox, KeyEventArgs e)
        {
            string text = textBox.Text;
            int selStart = textBox.SelectionStart;
            int selLength = textBox.SelectionLength;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    int wordStart = FindPreviousWordStart(text, selStart);
                    if (selLength == 0)
                    {
                        textBox.SelectionStart = wordStart;
                        textBox.SelectionLength = selStart - wordStart;
                    }
                    else if (selStart == textBox.SelectionStart)
                    {
                        textBox.SelectionStart = wordStart;
                        textBox.SelectionLength = selLength + (selStart - wordStart);
                    }
                    else
                    {
                        textBox.SelectionLength = selLength - (selStart - wordStart);
                    }
                    e.Handled = true;
                    break;

                case Keys.Right:
                    int wordEnd = FindNextWordEnd(text, selStart + selLength);
                    if (selLength == 0)
                    {
                        textBox.SelectionLength = wordEnd - selStart;
                    }
                    else if (selStart == textBox.SelectionStart)
                    {
                        textBox.SelectionLength = selLength - (wordEnd - (selStart + selLength));
                    }
                    else
                    {
                        textBox.SelectionLength = selLength + (wordEnd - (selStart + selLength));
                    }
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Finds the starting position of the previous word.
        /// </summary>
        private static int FindPreviousWordStart(string text, int currentPos)
        {
            if (currentPos <= 0) return 0;

            // Skip spaces before the current position
            while (currentPos > 0 && char.IsWhiteSpace(text[currentPos - 1]))
            {
                currentPos--;
            }

            // Find the start of the current word
            while (currentPos > 0 && !char.IsWhiteSpace(text[currentPos - 1]))
            {
                currentPos--;
            }

            return currentPos;
        }

        /// <summary>
        /// Finds the ending position of the next word.
        /// </summary>
        private static int FindNextWordEnd(string text, int currentPos)
        {
            if (currentPos >= text.Length) return text.Length;

            // Skip spaces after the current position
            while (currentPos < text.Length && char.IsWhiteSpace(text[currentPos]))
            {
                currentPos++;
            }

            // Find the end of the current word
            while (currentPos < text.Length && !char.IsWhiteSpace(text[currentPos]))
            {
                currentPos++;
            }

            return currentPos;
        }

        /// <summary>
        /// Processes keyboard shortcuts for text manipulation operations.
        /// </summary>
        private static void HandleKeyboardShortcut(Guna2TextBox textBox, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    SelectAllText(textBox);
                    break;

                case Keys.C:
                    CopySelectedText(textBox);
                    break;

                case Keys.X:
                    CutSelectedText(textBox);
                    break;

                case Keys.V:
                    PasteText(textBox);
                    break;

                case Keys.Z:
                    if (e.Shift) { Redo(textBox); }
                    else { Undo(textBox); }
                    break;

                case Keys.Y:
                    Redo(textBox);
                    break;
            }
        }

        /// <summary>
        /// Selects all text in the TextBox.
        /// </summary>
        private static void SelectAllText(Guna2TextBox textBox)
        {
            textBox.SelectAll();
        }

        /// <summary>
        /// Copies selected text to clipboard if any text is selected.
        /// </summary>
        private static void CopySelectedText(Guna2TextBox textBox)
        {
            if (HasSelectedText(textBox))
            {
                Clipboard.SetText(textBox.SelectedText);
            }
        }

        /// <summary>
        /// Cuts selected text to clipboard if any text is selected.
        /// </summary>
        private static void CutSelectedText(Guna2TextBox textBox)
        {
            if (HasSelectedText(textBox))
            {
                Clipboard.SetText(textBox.SelectedText);
                RemoveSelectedText(textBox);
            }
        }

        /// <summary>
        /// Pastes text from clipboard at current cursor position.
        /// </summary>
        private static void PasteText(Guna2TextBox textBox)
        {
            if (!Clipboard.ContainsText()) return;

            int selectionStart = textBox.SelectionStart;
            string clipboardText = Clipboard.GetText();

            InsertTextAtCursor(textBox, clipboardText);
            SetCursorPosition(textBox, selectionStart + clipboardText.Length);
        }

        /// <summary>
        /// Checks if the TextBox has any text selected.
        /// </summary>
        private static bool HasSelectedText(Guna2TextBox textBox)
        {
            return !string.IsNullOrEmpty(textBox.SelectedText);
        }

        /// <summary>
        /// Removes the currently selected text.
        /// </summary>
        private static void RemoveSelectedText(Guna2TextBox textBox)
        {
            textBox.SelectedText = string.Empty;
        }

        /// <summary>
        /// Inserts text at the current cursor position.
        /// </summary>
        private static void InsertTextAtCursor(Guna2TextBox textBox, string text)
        {
            textBox.SelectedText = text;
        }

        /// <summary>
        /// Sets the cursor position in the TextBox.
        /// </summary>
        private static void SetCursorPosition(Guna2TextBox textBox, int position)
        {
            textBox.SelectionStart = position;
        }

        /// <summary>
        /// Handles text changes and manages undo/redo stacks.
        /// </summary>
        private static void TextBox_TextChanged(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;
            if (!isTextChangedByUserFlags[textBox]) { return; }

            Stack<string> undoStack = undoStacks[textBox];
            Stack<string> redoStack = redoStacks[textBox];

            if (undoStack.Count == 0 || undoStack.Peek() != textBox.Text)
            {
                UpdateUndoStack(textBox, undoStack);
                redoStack.Clear();
            }
        }

        /// <summary>
        /// Updates the undo stack and manages its size.
        /// </summary>
        private static void UpdateUndoStack(Guna2TextBox textBox, Stack<string> undoStack)
        {
            undoStack.Push(textBox.Text);

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
            Stack<string> tempStack = new();
            Stack<string> currentStack = undoStacks[textBox];

            for (int i = 0; i < maxStackSize && currentStack.Count > 0; i++)
            {
                tempStack.Push(currentStack.Pop());
            }

            undoStacks[textBox] = new Stack<string>(tempStack.Reverse());
        }

        /// <summary>
        /// Performs undo operation if available.
        /// </summary>
        private static void Undo(Guna2TextBox textBox)
        {
            Stack<string> undoStack = undoStacks[textBox];
            Stack<string> redoStack = redoStacks[textBox];

            if (undoStack.Count > 1)
            {
                redoStack.Push(undoStack.Pop());
                UpdateTextBoxContent(textBox, undoStack.Peek());
            }
        }

        /// <summary>
        /// Performs redo operation if available.
        /// </summary>
        private static void Redo(Guna2TextBox textBox)
        {
            Stack<string> undoStack = undoStacks[textBox];
            Stack<string> redoStack = redoStacks[textBox];

            if (redoStack.Count > 0)
            {
                string redoText = redoStack.Pop();
                undoStack.Push(redoText);
                UpdateTextBoxContent(textBox, redoText);
            }
        }

        /// <summary>
        /// Updates TextBox content without triggering TextChanged event.
        /// </summary>
        private static void UpdateTextBoxContent(Guna2TextBox textBox, string newText)
        {
            isTextChangedByUserFlags[textBox] = false;
            textBox.Text = newText;
            SetCursorPosition(textBox, textBox.Text.Length);
            isTextChangedByUserFlags[textBox] = true;
        }
    }
}