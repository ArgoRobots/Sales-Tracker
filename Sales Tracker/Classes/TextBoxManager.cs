using Guna.UI2.WinForms;

namespace Sales_Tracker.Classes
{
    internal class TextBoxManager
    {
        // Add keyboard shortcuts to Guna2TextBox
        // Dictionaries to hold undo/redo stacks and flags for each TextBox
        private static readonly Dictionary<Guna2TextBox, Stack<string>> undoStacks = new();
        private static readonly Dictionary<Guna2TextBox, Stack<string>> redoStacks = new();
        private static readonly Dictionary<Guna2TextBox, bool> isTextChangedByUserFlags = new();

        public static void Attach(Guna2TextBox textBox)
        {
            // Initialize stacks and flags for this TextBox
            undoStacks[textBox] = new Stack<string>();
            redoStacks[textBox] = new Stack<string>();
            isTextChangedByUserFlags[textBox] = true;

            // Push the initial text onto the undo stack
            undoStacks[textBox].Push(textBox.Text);

            // Attach event handlers
            textBox.TextChanged += TextBox_TextChanged;
            textBox.KeyDown += TextBox_KeyDown;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
        }
        private static void TextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.A || e.KeyCode == Keys.C || e.KeyCode == Keys.X ||
                              e.KeyCode == Keys.V || e.KeyCode == Keys.Z || e.KeyCode == Keys.Y))
            {
                e.IsInputKey = true;
            }
        }
        private static void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;

            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        // Select all
                        textBox.SelectAll();
                        e.SuppressKeyPress = true;
                        break;

                    case Keys.C:
                        // Copy
                        if (!string.IsNullOrEmpty(textBox.SelectedText))
                        {
                            Clipboard.SetText(textBox.SelectedText);
                        }
                        e.SuppressKeyPress = true;
                        break;

                    case Keys.X:
                        // Cut
                        if (!string.IsNullOrEmpty(textBox.SelectedText))
                        {
                            Clipboard.SetText(textBox.SelectedText);
                            textBox.SelectedText = "";
                        }
                        e.SuppressKeyPress = true;
                        break;

                    case Keys.V:
                        // Paste
                        if (Clipboard.ContainsText())
                        {
                            int selectionStart = textBox.SelectionStart;
                            string clipboardText = Clipboard.GetText();
                            textBox.SelectedText = clipboardText;
                            textBox.SelectionStart = selectionStart + clipboardText.Length;
                        }
                        e.SuppressKeyPress = true;
                        break;

                    case Keys.Z:
                        if (e.Shift)
                        {
                            // Redo
                            Redo(textBox);
                        }
                        else
                        {
                            // Undo
                            Undo(textBox);
                        }
                        e.SuppressKeyPress = true;
                        break;

                    case Keys.Y:
                        // Redo
                        Redo(textBox);
                        e.SuppressKeyPress = true;
                        break;
                }
            }
        }
        private static void TextBox_TextChanged(object sender, EventArgs e)
        {
            Guna2TextBox textBox = (Guna2TextBox)sender;
            bool isTextChangedByUser = isTextChangedByUserFlags[textBox];

            if (isTextChangedByUser)
            {
                Stack<string> undoStack = undoStacks[textBox];
                Stack<string> redoStack = redoStacks[textBox];

                // Prevent pushing duplicate text onto the stack
                if (undoStack.Count == 0 || undoStack.Peek() != textBox.Text)
                {
                    undoStack.Push(textBox.Text);
                }

                // Clear redo stack since new action invalidates redo history
                redoStack.Clear();
            }
        }
        private static void Undo(Guna2TextBox textBox)
        {
            Stack<string> undoStack = undoStacks[textBox];
            Stack<string> redoStack = redoStacks[textBox];
            Dictionary<Guna2TextBox, bool> isTextChangedByUserFlag = isTextChangedByUserFlags;

            if (undoStack.Count > 1)
            {
                // Move the current state to the redo stack
                redoStack.Push(undoStack.Pop());

                // Update the TextBox without triggering another undo push
                isTextChangedByUserFlag[textBox] = false;
                textBox.Text = undoStack.Peek();
                textBox.SelectionStart = textBox.Text.Length;
                isTextChangedByUserFlag[textBox] = true;
            }
        }
        private static void Redo(Guna2TextBox textBox)
        {
            Stack<string> undoStack = undoStacks[textBox];
            Stack<string> redoStack = redoStacks[textBox];
            Dictionary<Guna2TextBox, bool> isTextChangedByUserFlag = isTextChangedByUserFlags;

            if (redoStack.Count > 0)
            {
                // Move the state back to the undo stack
                undoStack.Push(redoStack.Pop());

                // Update the TextBox without triggering another undo push
                isTextChangedByUserFlag[textBox] = false;
                textBox.Text = undoStack.Peek();
                textBox.SelectionStart = textBox.Text.Length;
                isTextChangedByUserFlag[textBox] = true;
            }
        }
    }
}