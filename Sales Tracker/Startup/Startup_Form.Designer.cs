namespace Sales_Tracker.Startup
{
    partial class Startup_Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Startup_Form));
            SuspendLayout();
            // 
            // Startup_Form
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1296, 766);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 4, 4, 4);
            MinimumSize = new Size(1039, 722);
            Name = "Startup_Form";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Argo Sales Tracker";
            ResumeLayout(false);
        }

        #endregion
    }
}