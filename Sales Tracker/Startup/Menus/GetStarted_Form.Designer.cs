
namespace Sales_Tracker.Startup
{
    partial class GetStarted_Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            label1 = new Label();
            OpenRecent_FlowLayoutPanel = new FlowLayoutPanel();
            CreateCompany_Button = new Guna.UI2.WinForms.Guna2Button();
            LblOpenRecent = new Label();
            OpenCompany_Button = new Guna.UI2.WinForms.Guna2Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold);
            label1.Location = new Point(22, 9);
            label1.Name = "label1";
            label1.Size = new Size(268, 41);
            label1.TabIndex = 0;
            label1.Text = "Argo Sales Tracker";
            // 
            // OpenRecent_FlowLayoutPanel
            // 
            OpenRecent_FlowLayoutPanel.AutoSize = true;
            OpenRecent_FlowLayoutPanel.BorderStyle = BorderStyle.FixedSingle;
            OpenRecent_FlowLayoutPanel.Location = new Point(29, 101);
            OpenRecent_FlowLayoutPanel.MaximumSize = new Size(260, 350);
            OpenRecent_FlowLayoutPanel.Name = "OpenRecent_FlowLayoutPanel";
            OpenRecent_FlowLayoutPanel.Size = new Size(260, 350);
            OpenRecent_FlowLayoutPanel.TabIndex = 1;
            // 
            // CreateCompany_Button
            // 
            CreateCompany_Button.BorderThickness = 1;
            CreateCompany_Button.CustomizableEdges = customizableEdges5;
            CreateCompany_Button.FillColor = Color.FromArgb(240, 240, 240);
            CreateCompany_Button.Font = new Font("Segoe UI", 12F);
            CreateCompany_Button.ForeColor = Color.Black;
            CreateCompany_Button.Image = Properties.Resources.CreateFile;
            CreateCompany_Button.ImageAlign = HorizontalAlignment.Left;
            CreateCompany_Button.ImageOffset = new Point(5, 0);
            CreateCompany_Button.ImageSize = new Size(30, 30);
            CreateCompany_Button.Location = new Point(305, 101);
            CreateCompany_Button.Margin = new Padding(10);
            CreateCompany_Button.Name = "CreateCompany_Button";
            CreateCompany_Button.ShadowDecoration.CustomizableEdges = customizableEdges6;
            CreateCompany_Button.Size = new Size(279, 69);
            CreateCompany_Button.TabIndex = 2;
            CreateCompany_Button.Text = "Create a new company";
            CreateCompany_Button.TextAlign = HorizontalAlignment.Left;
            CreateCompany_Button.TextOffset = new Point(10, 0);
            CreateCompany_Button.Click += CreateNewCompany_Click;
            // 
            // LblOpenRecent
            // 
            LblOpenRecent.AutoSize = true;
            LblOpenRecent.Font = new Font("Segoe UI", 12F);
            LblOpenRecent.Location = new Point(25, 73);
            LblOpenRecent.Name = "LblOpenRecent";
            LblOpenRecent.Size = new Size(95, 21);
            LblOpenRecent.TabIndex = 0;
            LblOpenRecent.Text = "Open recent";
            // 
            // OpenCompany_Button
            // 
            OpenCompany_Button.BorderThickness = 1;
            OpenCompany_Button.CustomizableEdges = customizableEdges7;
            OpenCompany_Button.FillColor = Color.FromArgb(240, 240, 240);
            OpenCompany_Button.Font = new Font("Segoe UI", 12F);
            OpenCompany_Button.ForeColor = Color.Black;
            OpenCompany_Button.Image = Properties.Resources.OpenFolder;
            OpenCompany_Button.ImageAlign = HorizontalAlignment.Left;
            OpenCompany_Button.ImageOffset = new Point(5, 0);
            OpenCompany_Button.ImageSize = new Size(30, 30);
            OpenCompany_Button.Location = new Point(305, 185);
            OpenCompany_Button.Margin = new Padding(10);
            OpenCompany_Button.Name = "OpenCompany_Button";
            OpenCompany_Button.ShadowDecoration.CustomizableEdges = customizableEdges8;
            OpenCompany_Button.Size = new Size(279, 69);
            OpenCompany_Button.TabIndex = 3;
            OpenCompany_Button.Text = "Open a company";
            OpenCompany_Button.TextAlign = HorizontalAlignment.Left;
            OpenCompany_Button.TextOffset = new Point(10, 0);
            OpenCompany_Button.Click += OpenCompany_Button_Click;
            // 
            // GetStarted_Form
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(250, 250, 250);
            ClientSize = new Size(880, 550);
            Controls.Add(OpenCompany_Button);
            Controls.Add(CreateCompany_Button);
            Controls.Add(label1);
            Controls.Add(OpenRecent_FlowLayoutPanel);
            Controls.Add(LblOpenRecent);
            FormBorderStyle = FormBorderStyle.None;
            Name = "GetStarted_Form";
            Shown += GetStarted_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel OpenRecent_FlowLayoutPanel;
        private System.Windows.Forms.Label LblOpenRecent;
        private Guna.UI2.WinForms.Guna2Button CreateCompany_Button;
        private Guna.UI2.WinForms.Guna2Button OpenCompany_Button;
    }
}