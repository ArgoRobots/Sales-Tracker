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
        private static readonly Dictionary<Guna2TextBox, Stack<TextState>> undoStacks = [];
        private static readonly Dictionary<Guna2TextBox, Stack<TextState>> redoStacks = [];
        private static readonly Dictionary<Guna2TextBox, bool> isTextChangedByUserFlags = [];
        private const byte maxStackSize = 250;

        /// <summary>
        /// Represents the state of a TextBox, including its text content and cursor position
        /// </summary>
        private class TextState
        {
            // Properties
            private readonly string _text;
            private readonly int _cursorPosition;

            // Getters and setters
            public string Text
            {
                get => _text;
                init => _text = value ?? "";
            }
            public int CursorPosition
            {
                get => _cursorPosition;
                init => _cursorPosition = value >= 0 ? value : 0;
            }

            // Methods
            public TextState(string text, int cursorPosition)
            {
                Text = text ?? "";
                CursorPosition = cursorPosition >= 0 ? cursorPosition : 0;
            }
        }

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
            undoStacks[textBox] = new Stack<TextState>();
            redoStacks[textBox] = new Stack<TextState>();
            isTextChangedByUserFlags[textBox] = true;
            undoStacks[textBox].Push(new TextState(textBox.Text, textBox.SelectionStart));
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

        /// Handles KeyDown event for keyboard shortcuts and text manipulation.
        /// </summary>
        private static void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
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
                HandleKeyboardShortcut(textBox, e);
            }
        }

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
                case Keys.OemPeriod:
                case Keys.OemMinus:
                case Keys.Subtract:
                case Keys.Decimal:
                    return true;
            }

            char keyChar = (char)keyData;
            return char.IsLetterOrDigit(keyChar) ||
                   char.IsPunctuation(keyChar) ||
                   char.IsWhiteSpace(keyChar);
        }

        /// <summary>
        /// Processes keyboard shortcuts for text manipulation operations.
        /// </summary>
        private static void HandleKeyboardShortcut(Guna2TextBox textBox, KeyEventArgs e)
        {
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
    }
}