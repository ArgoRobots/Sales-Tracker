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

        /// <summary>
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
                HandleKeyboardShortcut(textBox, e);
            }
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