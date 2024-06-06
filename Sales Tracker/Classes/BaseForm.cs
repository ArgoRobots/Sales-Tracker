namespace Sales_Tracker.Classes
{
    public partial class BaseForm : Form
    {
        /// <summary>
        /// The Ctrl+A shortcut does not work when ShortcutsEnabled = false,
        /// so override ProcessCmdKey to ensure Ctrl+A selects all text in the active TextBox.
        /// </summary>
        /// <returns>True if the key event was handled; otherwise, false.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (msg.Msg == 0x100 && keyData == (Keys.Control | Keys.A))
            {
                Tools.FindSelectedGTextBox(this)?.SelectAll();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
