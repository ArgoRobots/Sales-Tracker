
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            label1 = new System.Windows.Forms.Label();
            OpenRecent_FlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            BlankProject_Button = new Guna.UI2.WinForms.Guna2Button();
            LblOpenRecent = new System.Windows.Forms.Label();
            OpenProject_Button = new Guna.UI2.WinForms.Guna2Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI Semibold", 22F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(22, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(181, 41);
            label1.TabIndex = 0;
            label1.Text = "Argo Studio";
            // 
            // OpenRecent_FlowLayoutPanel
            // 
            OpenRecent_FlowLayoutPanel.AutoSize = true;
            OpenRecent_FlowLayoutPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            OpenRecent_FlowLayoutPanel.Location = new System.Drawing.Point(29, 101);
            OpenRecent_FlowLayoutPanel.MaximumSize = new System.Drawing.Size(260, 350);
            OpenRecent_FlowLayoutPanel.Name = "OpenRecent_FlowLayoutPanel";
            OpenRecent_FlowLayoutPanel.Size = new System.Drawing.Size(260, 350);
            OpenRecent_FlowLayoutPanel.TabIndex = 1;
            // 
            // BlankProject_Button
            // 
            BlankProject_Button.BorderThickness = 1;
            BlankProject_Button.CustomizableEdges = customizableEdges1;
            BlankProject_Button.FillColor = System.Drawing.Color.FromArgb(240, 240, 240);
            BlankProject_Button.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            BlankProject_Button.ForeColor = System.Drawing.Color.Black;
            BlankProject_Button.Image = Properties.Resources.CreateFile;
            BlankProject_Button.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            BlankProject_Button.ImageOffset = new System.Drawing.Point(5, 0);
            BlankProject_Button.ImageSize = new System.Drawing.Size(30, 30);
            BlankProject_Button.Location = new System.Drawing.Point(305, 101);
            BlankProject_Button.Margin = new System.Windows.Forms.Padding(10);
            BlankProject_Button.Name = "BlankProject_Button";
            BlankProject_Button.ShadowDecoration.CustomizableEdges = customizableEdges2;
            BlankProject_Button.Size = new System.Drawing.Size(279, 69);
            BlankProject_Button.TabIndex = 2;
            BlankProject_Button.Text = "Create a new project";
            BlankProject_Button.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            BlankProject_Button.TextOffset = new System.Drawing.Point(10, 0);
            BlankProject_Button.Click += CreateNewProject_Click;
            // 
            // LblOpenRecent
            // 
            LblOpenRecent.AutoSize = true;
            LblOpenRecent.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            LblOpenRecent.Location = new System.Drawing.Point(25, 73);
            LblOpenRecent.Name = "LblOpenRecent";
            LblOpenRecent.Size = new System.Drawing.Size(95, 21);
            LblOpenRecent.TabIndex = 0;
            LblOpenRecent.Text = "Open recent";
            // 
            // OpenProject_Button
            // 
            OpenProject_Button.BorderThickness = 1;
            OpenProject_Button.CustomizableEdges = customizableEdges3;
            OpenProject_Button.FillColor = System.Drawing.Color.FromArgb(240, 240, 240);
            OpenProject_Button.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            OpenProject_Button.ForeColor = System.Drawing.Color.Black;
            OpenProject_Button.Image = Properties.Resources.OpenFolder;
            OpenProject_Button.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            OpenProject_Button.ImageOffset = new System.Drawing.Point(5, 0);
            OpenProject_Button.ImageSize = new System.Drawing.Size(30, 30);
            OpenProject_Button.Location = new System.Drawing.Point(305, 185);
            OpenProject_Button.Margin = new System.Windows.Forms.Padding(10);
            OpenProject_Button.Name = "OpenProject_Button";
            OpenProject_Button.ShadowDecoration.CustomizableEdges = customizableEdges4;
            OpenProject_Button.Size = new System.Drawing.Size(279, 69);
            OpenProject_Button.TabIndex = 3;
            OpenProject_Button.Text = "Open a project";
            OpenProject_Button.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            OpenProject_Button.TextOffset = new System.Drawing.Point(10, 0);
            OpenProject_Button.Click += OpenProject_Button_Click;
            // 
            // GetStarted_Form
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
            ClientSize = new System.Drawing.Size(880, 550);
            Controls.Add(OpenProject_Button);
            Controls.Add(BlankProject_Button);
            Controls.Add(label1);
            Controls.Add(OpenRecent_FlowLayoutPanel);
            Controls.Add(LblOpenRecent);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "GetStarted_Form";
            Shown += GetStarted_Form_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel OpenRecent_FlowLayoutPanel;
        private System.Windows.Forms.Label LblOpenRecent;
        private Guna.UI2.WinForms.Guna2Button BlankProject_Button;
        private Guna.UI2.WinForms.Guna2Button OpenProject_Button;
    }
}