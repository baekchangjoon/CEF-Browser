using System.Windows.Forms;

namespace CEF_Browser
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel toolbarPanel;
        private Panel mainPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.toolbarPanel = new System.Windows.Forms.Panel();
            this.backButton = new System.Windows.Forms.Button();
            this.forwardButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.addressBar = new System.Windows.Forms.TextBox();
            this.goButton = new System.Windows.Forms.Button();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.toolbarPanel.SuspendLayout();
            this.SuspendLayout();

            // toolbarPanel
            this.toolbarPanel.Controls.Add(this.backButton);
            this.toolbarPanel.Controls.Add(this.forwardButton);
            this.toolbarPanel.Controls.Add(this.refreshButton);
            this.toolbarPanel.Controls.Add(this.addressBar);
            this.toolbarPanel.Controls.Add(this.goButton);
            this.toolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarPanel.Height = 40;
            this.toolbarPanel.Location = new System.Drawing.Point(0, 0);
            this.toolbarPanel.Name = "toolbarPanel";
            this.toolbarPanel.Size = new System.Drawing.Size(1200, 40);
            this.toolbarPanel.TabIndex = 0;

            // backButton
            this.backButton.Location = new System.Drawing.Point(5, 5);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(60, 30);
            this.backButton.TabIndex = 0;
            this.backButton.Text = "←";
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.BackButton_Click);
            this.backButton.Enabled = false;

            // forwardButton
            this.forwardButton.Location = new System.Drawing.Point(70, 5);
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(60, 30);
            this.forwardButton.TabIndex = 1;
            this.forwardButton.Text = "→";
            this.forwardButton.UseVisualStyleBackColor = true;
            this.forwardButton.Click += new System.EventHandler(this.ForwardButton_Click);
            this.forwardButton.Enabled = false;

            // refreshButton
            this.refreshButton.Location = new System.Drawing.Point(135, 5);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(60, 30);
            this.refreshButton.TabIndex = 2;
            this.refreshButton.Text = "↻";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);

            // addressBar
            this.addressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addressBar.Location = new System.Drawing.Point(205, 8);
            this.addressBar.Name = "addressBar";
            this.addressBar.Size = new System.Drawing.Size(980, 23);
            this.addressBar.TabIndex = 3;
            this.addressBar.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AddressBar_KeyDown);

            // goButton
            this.goButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.goButton.Location = new System.Drawing.Point(1190, 5);
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(50, 30);
            this.goButton.TabIndex = 4;
            this.goButton.Text = "Go";
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.GoButton_Click);

            // mainPanel
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 40);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(1200, 760);
            this.mainPanel.TabIndex = 1;

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.toolbarPanel);
            this.Name = "MainForm";
            this.Text = "CEF Browser";
            this.toolbarPanel.ResumeLayout(false);
            this.toolbarPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
