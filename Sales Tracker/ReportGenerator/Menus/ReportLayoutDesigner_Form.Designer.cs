namespace Sales_Tracker.ReportGenerator
{
    partial class ReportLayoutDesigner_Form
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            RightCanvas_Panel = new Guna.UI2.WinForms.Guna2Panel();
            Canvas_GroupBox = new Guna.UI2.WinForms.Guna2GroupBox();
            Canvas_Panel = new Panel();
            LeftTools_Panel = new Guna.UI2.WinForms.Guna2Panel();
            Properties_GroupBox = new Guna.UI2.WinForms.Guna2GroupBox();
            Properties_Separator = new Guna.UI2.WinForms.Guna2Separator();
            ElementProperties_Label = new Label();
            PropertiesContainer_Panel = new Panel();
            Tools_GroupBox = new Guna.UI2.WinForms.Guna2GroupBox();
            ToolsContainer_Panel = new Panel();
            RightCanvas_Panel.SuspendLayout();
            Canvas_GroupBox.SuspendLayout();
            LeftTools_Panel.SuspendLayout();
            Properties_GroupBox.SuspendLayout();
            Tools_GroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // RightCanvas_Panel
            // 
            RightCanvas_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            RightCanvas_Panel.Controls.Add(Canvas_GroupBox);
            RightCanvas_Panel.CustomizableEdges = customizableEdges3;
            RightCanvas_Panel.FillColor = Color.Transparent;
            RightCanvas_Panel.Location = new Point(380, 0);
            RightCanvas_Panel.Margin = new Padding(4, 5, 4, 5);
            RightCanvas_Panel.Name = "RightCanvas_Panel";
            RightCanvas_Panel.Padding = new Padding(7, 28, 28, 14);
            RightCanvas_Panel.ShadowDecoration.CustomizableEdges = customizableEdges4;
            RightCanvas_Panel.Size = new Size(1120, 900);
            RightCanvas_Panel.TabIndex = 1;
            // 
            // Canvas_GroupBox
            // 
            Canvas_GroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Canvas_GroupBox.BackColor = Color.Transparent;
            Canvas_GroupBox.BorderRadius = 8;
            Canvas_GroupBox.BorderThickness = 0;
            Canvas_GroupBox.Controls.Add(Canvas_Panel);
            Canvas_GroupBox.CustomBorderColor = Color.FromArgb(94, 148, 255);
            Canvas_GroupBox.CustomizableEdges = customizableEdges1;
            Canvas_GroupBox.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Canvas_GroupBox.ForeColor = Color.White;
            Canvas_GroupBox.Location = new Point(7, 28);
            Canvas_GroupBox.Margin = new Padding(4, 5, 4, 5);
            Canvas_GroupBox.Name = "Canvas_GroupBox";
            Canvas_GroupBox.Padding = new Padding(14, 17, 14, 17);
            Canvas_GroupBox.ShadowDecoration.CustomizableEdges = customizableEdges2;
            Canvas_GroupBox.Size = new Size(1085, 858);
            Canvas_GroupBox.TabIndex = 0;
            Canvas_GroupBox.Text = "Report Canvas - Drag elements to arrange layout";
            // 
            // Canvas_Panel
            // 
            Canvas_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Canvas_Panel.BackColor = Color.White;
            Canvas_Panel.BorderStyle = BorderStyle.FixedSingle;
            Canvas_Panel.Location = new Point(21, 83);
            Canvas_Panel.Margin = new Padding(4, 5, 4, 5);
            Canvas_Panel.Name = "Canvas_Panel";
            Canvas_Panel.Size = new Size(1041, 749);
            Canvas_Panel.TabIndex = 0;
            // 
            // LeftTools_Panel
            // 
            LeftTools_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LeftTools_Panel.Controls.Add(Properties_GroupBox);
            LeftTools_Panel.Controls.Add(Tools_GroupBox);
            LeftTools_Panel.CustomizableEdges = customizableEdges9;
            LeftTools_Panel.FillColor = Color.Transparent;
            LeftTools_Panel.Location = new Point(0, 0);
            LeftTools_Panel.Margin = new Padding(4, 5, 4, 5);
            LeftTools_Panel.Name = "LeftTools_Panel";
            LeftTools_Panel.Padding = new Padding(28, 28, 7, 14);
            LeftTools_Panel.ShadowDecoration.CustomizableEdges = customizableEdges10;
            LeftTools_Panel.Size = new Size(380, 900);
            LeftTools_Panel.TabIndex = 0;
            // 
            // Properties_GroupBox
            // 
            Properties_GroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Properties_GroupBox.BackColor = Color.Transparent;
            Properties_GroupBox.BorderRadius = 8;
            Properties_GroupBox.BorderThickness = 0;
            Properties_GroupBox.Controls.Add(Properties_Separator);
            Properties_GroupBox.Controls.Add(ElementProperties_Label);
            Properties_GroupBox.Controls.Add(PropertiesContainer_Panel);
            Properties_GroupBox.CustomBorderColor = Color.FromArgb(94, 148, 255);
            Properties_GroupBox.CustomizableEdges = customizableEdges5;
            Properties_GroupBox.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Properties_GroupBox.ForeColor = Color.White;
            Properties_GroupBox.Location = new Point(28, 448);
            Properties_GroupBox.Margin = new Padding(4, 5, 4, 5);
            Properties_GroupBox.Name = "Properties_GroupBox";
            Properties_GroupBox.Padding = new Padding(0, 17, 0, 10);
            Properties_GroupBox.ShadowDecoration.CustomizableEdges = customizableEdges6;
            Properties_GroupBox.Size = new Size(345, 438);
            Properties_GroupBox.TabIndex = 1;
            Properties_GroupBox.Text = "Element Properties";
            // 
            // Properties_Separator
            // 
            Properties_Separator.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Properties_Separator.Location = new Point(0, 74);
            Properties_Separator.Name = "Properties_Separator";
            Properties_Separator.Size = new Size(345, 3);
            Properties_Separator.TabIndex = 2;
            // 
            // ElementProperties_Label
            // 
            ElementProperties_Label.AutoSize = true;
            ElementProperties_Label.BackColor = Color.Transparent;
            ElementProperties_Label.Font = new Font("Segoe UI", 9F);
            ElementProperties_Label.ForeColor = SystemColors.ControlText;
            ElementProperties_Label.Location = new Point(4, 45);
            ElementProperties_Label.Margin = new Padding(4, 0, 4, 0);
            ElementProperties_Label.Name = "ElementProperties_Label";
            ElementProperties_Label.Size = new Size(173, 25);
            ElementProperties_Label.TabIndex = 0;
            ElementProperties_Label.Text = "No element selected";
            // 
            // PropertiesContainer_Panel
            // 
            PropertiesContainer_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PropertiesContainer_Panel.AutoScroll = true;
            PropertiesContainer_Panel.BackColor = Color.Transparent;
            PropertiesContainer_Panel.Location = new Point(0, 75);
            PropertiesContainer_Panel.Margin = new Padding(4, 5, 4, 5);
            PropertiesContainer_Panel.Name = "PropertiesContainer_Panel";
            PropertiesContainer_Panel.Size = new Size(345, 353);
            PropertiesContainer_Panel.TabIndex = 1;
            // 
            // Tools_GroupBox
            // 
            Tools_GroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Tools_GroupBox.BackColor = Color.Transparent;
            Tools_GroupBox.BorderRadius = 8;
            Tools_GroupBox.BorderThickness = 0;
            Tools_GroupBox.Controls.Add(ToolsContainer_Panel);
            Tools_GroupBox.CustomBorderColor = Color.FromArgb(94, 148, 255);
            Tools_GroupBox.CustomizableEdges = customizableEdges7;
            Tools_GroupBox.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Tools_GroupBox.ForeColor = Color.White;
            Tools_GroupBox.Location = new Point(28, 28);
            Tools_GroupBox.Margin = new Padding(4, 5, 4, 5);
            Tools_GroupBox.Name = "Tools_GroupBox";
            Tools_GroupBox.Padding = new Padding(14, 17, 14, 17);
            Tools_GroupBox.ShadowDecoration.CustomizableEdges = customizableEdges8;
            Tools_GroupBox.Size = new Size(345, 400);
            Tools_GroupBox.TabIndex = 0;
            Tools_GroupBox.Text = "Report Elements";
            // 
            // ToolsContainer_Panel
            // 
            ToolsContainer_Panel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ToolsContainer_Panel.AutoScroll = true;
            ToolsContainer_Panel.BackColor = Color.Transparent;
            ToolsContainer_Panel.Location = new Point(14, 54);
            ToolsContainer_Panel.Margin = new Padding(4, 5, 4, 5);
            ToolsContainer_Panel.Name = "ToolsContainer_Panel";
            ToolsContainer_Panel.Size = new Size(316, 332);
            ToolsContainer_Panel.TabIndex = 1;
            // 
            // ReportLayoutDesigner_Form
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1500, 900);
            Controls.Add(LeftTools_Panel);
            Controls.Add(RightCanvas_Panel);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4, 5, 4, 5);
            Name = "ReportLayoutDesigner_Form";
            VisibleChanged += ReportLayoutDesigner_Form_VisibleChanged;
            Resize += ReportLayoutDesigner_Form_Resize;
            RightCanvas_Panel.ResumeLayout(false);
            Canvas_GroupBox.ResumeLayout(false);
            LeftTools_Panel.ResumeLayout(false);
            Properties_GroupBox.ResumeLayout(false);
            Properties_GroupBox.PerformLayout();
            Tools_GroupBox.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Guna.UI2.WinForms.Guna2Panel LeftTools_Panel;
        private Guna.UI2.WinForms.Guna2Panel RightCanvas_Panel;
        private Guna.UI2.WinForms.Guna2GroupBox Tools_GroupBox;
        private Guna.UI2.WinForms.Guna2GroupBox Properties_GroupBox;
        private System.Windows.Forms.Label ElementProperties_Label;
        private Guna.UI2.WinForms.Guna2GroupBox Canvas_GroupBox;
        private System.Windows.Forms.Panel Canvas_Panel;
        public Panel ToolsContainer_Panel;
        public Panel PropertiesContainer_Panel;
        private Guna.UI2.WinForms.Guna2Separator Properties_Separator;
    }
}