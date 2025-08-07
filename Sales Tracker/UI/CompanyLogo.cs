using Guna.UI2.WinForms;
using Sales_Tracker.Classes;
using Sales_Tracker.Theme;
using System.Drawing.Drawing2D;

namespace Sales_Tracker.UI
{
    public static class CompanyLogo
    {
        // Properties
        private static PictureBox _companyLogo;
        private static bool _isLogoHovered = false;
        private static Bitmap _cameraIcon;
        private static readonly SolidBrush _overlayBrush;

        // Getters
        public static Guna2Panel CompanyLogoRightClick_Panel { get; private set; }

        // Static constructor for one-time initialization
        static CompanyLogo()
        {
            _overlayBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
        }

        // Methods
        public static void ConstructCompanyLogoRightClickMenu()
        {
            CompanyLogoRightClick_Panel = CustomControls.ConstructPanelForMenu(
                new Size(CustomControls.PanelWidth - 50, 2 * CustomControls.PanelButtonHeight + CustomControls.SpaceForPanel),
                "companyLogoRightClick_Panel"
            );

            FlowLayoutPanel flowPanel = (FlowLayoutPanel)CompanyLogoRightClick_Panel.Controls[0];
            int btnWidth = CustomControls.PanelBtnWidth - 50;

            // Change Logo button
            Guna2Button changeLogo_Button = CustomControls.ConstructBtnForMenu("Change Logo...", btnWidth, true, flowPanel);
            changeLogo_Button.Click += (sender, e) =>
            {
                CustomControls.CloseAllPanels();
                BrowseForCompanyLogo();
            };

            // Remove Logo button (will be shown/hidden based on whether there's a custom logo)
            Guna2Button removeLogo_Button = CustomControls.ConstructBtnForMenu("Remove Logo", btnWidth, true, flowPanel);
            removeLogo_Button.Click += (sender, e) =>
            {
                CustomControls.CloseAllPanels();
                RemoveCompanyLogo();
            };
            removeLogo_Button.Name = "RemoveLogo_Button";
        }
        public static void SetCompanyLogo()
        {
            if (Properties.Settings.Default.ShowCompanyLogo)
            {
                CreateLogoIfNeeded();
                LoadCompanyLogoImage();
                UpdateLogoPosition();
            }
            else
            {
                RemoveLogoControl();
            }
        }
        private static void CreateLogoIfNeeded()
        {
            if (_companyLogo != null) { return; }

            _companyLogo = new PictureBox
            {
                Size = new Size(60, 60),
                Location = new Point(18, (MainMenu_Form.Instance.MainTop_Panel.Height - 60) / 2),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };

            // Set up event handlers
            _companyLogo.MouseEnter += CompanyLogo_MouseEnter;
            _companyLogo.MouseLeave += CompanyLogo_MouseLeave;
            _companyLogo.Click += CompanyLogo_Click;
            _companyLogo.MouseDown += CompanyLogo_MouseDown;
            _companyLogo.Paint += CompanyLogo_Paint;

            CustomTooltip.SetToolTip(_companyLogo, "", "Click to change company logo • Right-click for options");
            MainMenu_Form.Instance.MainTop_Panel.Controls.Add(_companyLogo);
        }
        private static void RemoveLogoControl()
        {
            if (_companyLogo == null) { return; }

            MainMenu_Form.Instance.MainTop_Panel.Controls.Remove(_companyLogo);
            _companyLogo.Image?.Dispose();
            _companyLogo.Dispose();
            _companyLogo = null;

            // Move company name back to original position
            MainMenu_Form.Instance.CompanyName_Label.Left = 18;
            MainMenu_Form.Instance.SetEditButtonLocation();
        }
        private static void UpdateLogoPosition()
        {
            MainMenu_Form.Instance.CompanyName_Label.Left = _companyLogo.Right + 10;
            MainMenu_Form.Instance.SetEditButtonLocation();
        }
        private static void LoadCompanyLogoImage()
        {
            if (_companyLogo == null) { return; }

            // Dispose previous image
            _companyLogo.Image?.Dispose();

            string logoPath = Properties.Settings.Default.CompanyLogoPath;

            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                try
                {
                    _companyLogo.Image = Image.FromFile(logoPath);
                }
                catch (Exception ex)
                {
                    Log.Error_WriteToFile($"Failed to load company logo: {ex.Message}");
                    _companyLogo.Image = CreateDefaultLogoImage();
                }
            }
            else
            {
                _companyLogo.Image = CreateDefaultLogoImage();
            }
        }
        private static Bitmap CreateDefaultLogoImage()
        {
            Bitmap defaultLogo = new(60, 60);

            using (Graphics g = Graphics.FromImage(defaultLogo))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (SolidBrush backgroundBrush = new(CustomColors.AccentBlue))
                {
                    g.FillRectangle(backgroundBrush, 0, 0, 60, 60);
                }

                string initials = GetCompanyInitials(Directories.CompanyName);
                using Font font = new("Segoe UI", 16, FontStyle.Bold);
                using SolidBrush textBrush = new(Color.White);
                SizeF textSize = g.MeasureString(initials, font);
                float x = (60 - textSize.Width) / 2;
                float y = (60 - textSize.Height) / 2;
                g.DrawString(initials, font, textBrush, x, y);
            }

            return defaultLogo;
        }
        private static string GetCompanyInitials(string companyName)
        {
            string[] words = companyName.Split([' ', '.', '-'], StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) { return "AC"; }  // Default initials if name is empty "Argo Company"
            if (words.Length == 1) { return char.ToUpperInvariant(words[0][0]).ToString(); }

            return char.ToUpperInvariant(words[0][0]).ToString() + char.ToUpperInvariant(words[1][0]).ToString();
        }
        private static void CompanyLogo_MouseEnter(object sender, EventArgs e)
        {
            _isLogoHovered = true;
            _companyLogo.Invalidate();

            // Create camera icon on first hover
            _cameraIcon ??= CreateCameraIcon();
        }
        private static void CompanyLogo_MouseLeave(object sender, EventArgs e)
        {
            _isLogoHovered = false;
            _companyLogo.Invalidate();
        }
        private static void CompanyLogo_Paint(object sender, PaintEventArgs e)
        {
            if (!_isLogoHovered || _cameraIcon == null) { return; }

            // Draw semi-transparent overlay using cached brush
            e.Graphics.FillRectangle(_overlayBrush, _companyLogo.ClientRectangle);

            // Draw camera icon in center
            int iconSize = 32;
            int x = (_companyLogo.Width - iconSize) / 2;
            int y = (_companyLogo.Height - iconSize) / 2;
            e.Graphics.DrawImage(_cameraIcon, x, y, iconSize, iconSize);
        }
        private static Bitmap CreateCameraIcon()
        {
            Bitmap icon = new(32, 32);

            using (Graphics g = Graphics.FromImage(icon))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using Pen whitePen = new(Color.White, 2);
                using SolidBrush whiteBrush = new(Color.White);

                // Draw camera body
                g.DrawRectangle(whitePen, 4, 12, 24, 14);
                // Draw camera top
                g.DrawRectangle(whitePen, 9, 8, 8, 4);
                // Draw lens
                g.DrawEllipse(whitePen, 12, 16, 8, 8);
                // Draw lens center
                g.FillEllipse(whiteBrush, 14, 18, 4, 4);
            }

            return icon;
        }
        private static void CompanyLogo_Click(object sender, EventArgs e)
        {
            MainMenu_Form.Instance.CloseAllPanels(null, null);
            BrowseForCompanyLogo();
        }
        private static void CompanyLogo_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowLogoContextMenu(e.Location);
            }
        }
        private static void ShowLogoContextMenu(Point location)
        {
            // Toggle Remove Logo button visibility
            FlowLayoutPanel flowPanel = (FlowLayoutPanel)CompanyLogoRightClick_Panel.Controls[0];
            Guna2Button removeLogo_Button = flowPanel.Controls.OfType<Guna2Button>()
                .FirstOrDefault(b => b.Name == "RemoveLogo_Button");

            if (removeLogo_Button != null)
            {
                removeLogo_Button.Visible = !string.IsNullOrEmpty(Properties.Settings.Default.CompanyLogoPath);
            }

            // Position and show menu
            Point screenLocation = _companyLogo.PointToScreen(location);
            CompanyLogoRightClick_Panel.Location = MainMenu_Form.Instance.PointToClient(screenLocation);

            MainMenu_Form.Instance.Controls.Add(CompanyLogoRightClick_Panel);
            CompanyLogoRightClick_Panel.BringToFront();
        }
        public static void BrowseForCompanyLogo()
        {
            using OpenFileDialog dialog = new();

            dialog.Title = "Select Company Logo";
            dialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.ico|" +
                            "PNG Files|*.png|" +
                            "JPEG Files|*.jpg;*.jpeg|" +
                            "Bitmap Files|*.bmp|" +
                            "GIF Files|*.gif|" +
                            "Icon Files|*.ico|" +
                            "All Files|*.*";
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() != DialogResult.OK) { return; }

            try
            {
                // Update settings
                string oldLogoPath = Properties.Settings.Default.CompanyLogoPath;
                Properties.Settings.Default.CompanyLogoPath = dialog.FileName;
                Properties.Settings.Default.Save();

                LogLogoChange(oldLogoPath, dialog.FileName);
                LoadCompanyLogoImage();
            }
            catch
            {
                ShowImageError();
            }
        }
        private static void LogLogoChange(string oldLogoPath, string newLogoPath)
        {
            string message = string.IsNullOrEmpty(oldLogoPath)
                ? $"Company logo set to: {Path.GetFileName(newLogoPath)}"
                : $"Company logo changed to: {Path.GetFileName(newLogoPath)}";

            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(
                MainMenu_Form.SettingsThatHaveChangedInFile, 3, message);
            Log.Write(2, $"Company logo updated: {newLogoPath}");
        }
        private static void ShowImageError()
        {
            CustomMessageBox.Show("Invalid Image",
                "The selected file is not a valid image or cannot be loaded.",
                CustomMessageBoxIcon.Error, CustomMessageBoxButtons.Ok);

            Log.Error_WriteToFile("Failed to load selected logo image");
        }
        public static void RemoveCompanyLogo()
        {
            Properties.Settings.Default.CompanyLogoPath = "";
            Properties.Settings.Default.Save();

            CustomMessage_Form.AddThingThatHasChangedAndLogMessage(
                MainMenu_Form.SettingsThatHaveChangedInFile, 3, "Company logo removed");
            LoadCompanyLogoImage();

            Log.Write(2, "Company logo removed");
        }
    }
}