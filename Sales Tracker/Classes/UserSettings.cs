using Sales_Tracker.Settings.Menus;

namespace Sales_Tracker.Classes
{
    class UserSettings
    {
        public static void SaveUserSettings()
        {
            Properties.Settings.Default.Language = General_Form.Instance.Language_ComboBox.Text;
            Properties.Settings.Default.Currency = General_Form.Instance.Currency_ComboBox.Text;
            Properties.Settings.Default.ShowDebugInfo = General_Form.Instance.ShowDebugInfo_CheckBox.Checked;
            Properties.Settings.Default.SendAnonymousInformation = General_Form.Instance.SendAnonymousInformation_CheckBox.Checked;
            Properties.Settings.Default.PurchaseReceipts = General_Form.Instance.PurchaseReceipts_CheckBox.Checked;

            Properties.Settings.Default.EncryptFiles = Security_Form.Instance.EncryptFiles_CheckBox.Checked;
            Properties.Settings.Default.Save();
        }
        public static void ResetAllToDefault()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();

            General_Form.Instance.UpdateControls();
            Security_Form.Instance.UpdateControls();
        }
    }
}