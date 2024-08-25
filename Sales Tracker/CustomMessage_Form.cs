using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Properties;

namespace Sales_Tracker
{
    public partial class CustomMessage_Form : Form
    {
        // Properties
        private static CustomMessage_Form _instance;
        public static CustomMessage_Form Instance
        {
            get { return _instance; }
        }
        public CustomMessageBoxResult result;

        // Init.
        public CustomMessage_Form(string title, string message, CustomMessageBoxIcon icon, CustomMessageBoxButtons buttons)
        {
            InitializeComponent();
            _instance = this;
            DoubleBuffered = true;

            LoadingPanel.ShowBlankLoadingPanel(this);

            Theme.SetThemeForForm(this);

            SetMessageBox(title, message, icon, buttons);
        }

        // Form event handlers
        private void CustomMessage_Form_Shown(object sender, EventArgs e)
        {
            LoadingPanel.HideBlankLoadingPanel(this);
        }

        // Methods
        private void SetMessageBox(string title, string message, CustomMessageBoxIcon icon, CustomMessageBoxButtons buttons)
        {
            // Set text
            Text = title;
            Message_Label.Text = message;
            Message_Label.LinkArea = new LinkArea(CustomMessageBoxVariables.LinkStart, CustomMessageBoxVariables.LinkLength);
            Message_Label.LinkColor = CustomColors.linkColor;
            Message_Label.ActiveLinkColor = CustomColors.linkColor;

            CustomMessageBoxVariables.Reset();
            Controls.Add(Icon_PictureBox);

            Height = 130 + Message_Label.Height;

            // Set icon
            switch (icon)
            {
                case CustomMessageBoxIcon.Question:
                    Icon_PictureBox.BackgroundImage = Resources.HelpGray;
                    break;
                case CustomMessageBoxIcon.Exclamation:
                    Icon_PictureBox.BackgroundImage = Resources.ExclamationMark;
                    break;
                case CustomMessageBoxIcon.Error:
                    Icon_PictureBox.BackgroundImage = Resources.Error;
                    break;
                case CustomMessageBoxIcon.Info:
                    Icon_PictureBox.BackgroundImage = Resources.Info;
                    break;
                case CustomMessageBoxIcon.None:
                    Icon_PictureBox.BackgroundImage = null;
                    Controls.Remove(Icon_PictureBox);
                    break;
            }

            // Set buttons
            switch (buttons)
            {
                case CustomMessageBoxButtons.YesNo:
                    No_Button.Left = Width - No_Button.Width - 30;
                    Yes_Button.Left = No_Button.Left - Yes_Button.Width - 15;
                    Controls.Add(Yes_Button);
                    Controls.Add(No_Button);
                    Controls.Remove(Cancel_Button);
                    Controls.Remove(Ok_Button);
                    Controls.Remove(Save_Button);
                    Controls.Remove(DontSave_Button);
                    Yes_Button.Focus();
                    break;
                case CustomMessageBoxButtons.Ok:
                    Ok_Button.Left = Width - Ok_Button.Width - 30;
                    Controls.Add(Ok_Button);
                    Controls.Remove(Yes_Button);
                    Controls.Remove(No_Button);
                    Controls.Remove(Cancel_Button);
                    Controls.Remove(Save_Button);
                    Controls.Remove(DontSave_Button);
                    Ok_Button.Focus();
                    break;
                case CustomMessageBoxButtons.OkCancel:
                    Cancel_Button.Left = Width - Cancel_Button.Width - 30;
                    Ok_Button.Left = Cancel_Button.Left - Ok_Button.Width - 15;
                    Controls.Add(Ok_Button);
                    Controls.Add(Cancel_Button);
                    Controls.Remove(Yes_Button);
                    Controls.Remove(No_Button);
                    Controls.Remove(Save_Button);
                    Controls.Remove(DontSave_Button);
                    Ok_Button.Focus();
                    break;
                case CustomMessageBoxButtons.SaveDontSaveCancel:
                    Cancel_Button.Left = Width - Cancel_Button.Width - 30;
                    DontSave_Button.Left = Cancel_Button.Left - DontSave_Button.Width - 15;
                    Save_Button.Left = DontSave_Button.Left - Save_Button.Width - 15;
                    Controls.Add(Save_Button);
                    Controls.Add(DontSave_Button);
                    Controls.Add(Cancel_Button);
                    Controls.Remove(Yes_Button);
                    Controls.Remove(No_Button);
                    Controls.Remove(Ok_Button);
                    Save_Button.Focus();

                    ShowThingsThatHaveChanged();
                    break;
            }
        }
        private Panel Changed_Panel;
        private Guna2Panel ChangedBackground_Panel;
        private void AdjustFormHeight(int contentHeight)
        {
            int requiredHeight = Math.Min(contentHeight, MaximumSize.Height);

            if (contentHeight > MaximumSize.Height - 150)
            {
                Height = MaximumSize.Height;
                ChangedBackground_Panel.Height = Height - 150;
                Changed_Panel.Height = Height - 152;
                FormBorderStyle = FormBorderStyle.SizableToolWindow;
                Changed_Panel.AutoScroll = true;
            }
            else
            {
                Height = requiredHeight + 150;
                ChangedBackground_Panel.Height = Height - 150;
                Changed_Panel.Height = Height - 152;
                FormBorderStyle = FormBorderStyle.FixedToolWindow;
                Changed_Panel.AutoScroll = false;
            }
        }

        // Things that have changed
        private void ShowThingsThatHaveChanged()
        {
            // Construct panels
            ChangedBackground_Panel = new()
            {
                Top = 40,
                Size = new Size(450, 70),
                BackColor = CustomColors.mainBackground,
                BorderColor = CustomColors.controlBorder,
                BorderThickness = 1,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom
            };

            Changed_Panel = new()
            {
                Location = new Point(1, 1),
                Size = new Size(448, 68),
                BackColor = CustomColors.mainBackground,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom
            };
            ChangedBackground_Panel.Left = (Width - Changed_Panel.Width) / 2;  // Center
            ChangedBackground_Panel.Controls.Add(Changed_Panel);
            Controls.Add(ChangedBackground_Panel);
            Controls.Remove(Icon_PictureBox);
            Controls.Remove(Message_Label);

            // Construct new message label
            Label label = new()
            {
                Top = 10,
                Text = Message_Label.Text,
                Font = new Font("Segoe UI", 11),
                ForeColor = CustomColors.text,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true
            };
            Controls.Add(label);
            label.Left = (Width - label.Width) / 2;

            // Add list of things that has changed to panel
            int top = 5;

            top = AddListForThingsChanged("General", MainMenu_Form.ThingsThatHaveChangedInFile, top);
            top = AddListForThingsChanged("Accountants", Accountants_Form.thingsThatHaveChangedInFile, top);
            top = AddListForThingsChanged("Categories", Categories_Form.ThingsThatHaveChangedInFile, top);
            top = AddListForThingsChanged("Companies", Companies_Form.thingsThatHaveChangedInFile, top);
            top = AddListForThingsChanged("Purchases", AddPurchase_Form.ThingsThatHaveChangedInFile, top);
            top = AddListForThingsChanged("Sales", AddSale_Form.ThingsThatHaveChangedInFile, top);
            top = AddListForThingsChanged("Products", Products_Form.ThingsThatHaveChangedInFile, top);

            // This is a dummy control to add extra space at the end, even when the panel is scrollable
            Changed_Panel.Controls.Add(new Label()
            {
                Size = new Size(1, 15),
                Left = 1,
                Top = top,
            });
            top += 20;

            AdjustFormHeight(top);
        }
        private int AddListForThingsChanged(string title, List<string> list, int top)
        {
            if (list.Count == 0) { return top; }

            Message_Label.Location = new Point((Width - Message_Label.Width) / 2, 0);

            Label label = new()
            {
                Text = title,
                Location = new Point(10, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = CustomColors.text,
                Cursor = Cursors.Arrow,
            };
            Changed_Panel.Controls.Add(label);
            top += label.Height + 5;

            foreach (string item in list)
            {
                label = new()
                {
                    Text = item,
                    Location = new Point(25, top),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11),
                    ForeColor = CustomColors.text,
                    Cursor = Cursors.Arrow,
                };
                Changed_Panel.Controls.Add(label);
                top += label.Height + 5;
            }

            return top;
        }
        public static void AddThingThatHasChanged(List<string> list, string thing)
        {
            bool isChanged = false;

            if (!list.Contains(thing))
            {
                list.Add(thing);
                isChanged = true;
            }

            AddChangesMadeToInfoFile(isChanged);
        }
        public static void AddChangesMadeToInfoFile(bool changesMade)
        {
            DataFileManager.SetValue(Directories.Info_file, DataFileManager.AppDataSettings.ChangesMade, changesMade.ToString());
            DataFileManager.Save(Directories.AppDataConfig_file);
        }

        // Event handlers
        private void No_Button_Click(object sender, EventArgs e)
        {
            result = CustomMessageBoxResult.No;
            Close();
        }
        private void Yes_Button_Click(object sender, EventArgs e)
        {
            result = CustomMessageBoxResult.Yes;
            Close();
        }
        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            result = CustomMessageBoxResult.Cancel;
            Close();
        }
        private void Ok_Button_Click(object sender, EventArgs e)
        {
            result = CustomMessageBoxResult.Ok;
            Close();
        }
        private void Save_Button_Click(object sender, EventArgs e)
        {
            result = CustomMessageBoxResult.Save;
            Close();
        }
        private void DontSave_Button_Click(object sender, EventArgs e)
        {
            result = CustomMessageBoxResult.DontSave;
            Close();
        }
        private void Message_Label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Tools.OpenLink(CustomMessageBoxVariables.Link);
        }
    }

    public enum CustomMessageBoxIcon
    {
        Question,
        Exclamation,
        Error,
        Info,
        None
    }
    public enum CustomMessageBoxButtons
    {
        YesNo,
        Ok,
        OkCancel,
        SaveDontSaveCancel
    }
    public enum CustomMessageBoxResult
    {
        Ok,
        Cancel,
        Yes,
        No,
        Save,
        DontSave
    }

    public static class CustomMessageBox
    {
        public static CustomMessageBoxResult Show(string title, string message, CustomMessageBoxIcon icon, CustomMessageBoxButtons buttons)
        {
            // Construct a new form to free resources when it closes
            new CustomMessage_Form(title, message, icon, buttons).ShowDialog();
            return CustomMessage_Form.Instance.result;
        }
    }

    public static class CustomMessageBoxVariables
    {
        public static string Link { get; set; }
        public static int LinkStart { get; set; }
        public static int LinkLength { get; set; }

        public static void Reset()
        {
            LinkStart = 0;
            LinkLength = 0;
        }
    }
}