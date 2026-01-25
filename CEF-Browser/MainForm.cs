using System;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace CEF_Browser
{
    /// <summary>
    /// Main browser form
    /// </summary>
    public partial class MainForm : Form
    {
        private ChromiumWebBrowser browser;

        public MainForm(string initialUrl = "about:blank")
        {
            InitializeComponent();
            InitializeBrowser(initialUrl);
        }

        private void InitializeBrowser(string initialUrl)
        {
            browser = new ChromiumWebBrowser(initialUrl);
            browser.Dock = DockStyle.Fill;
            browser.TitleChanged += OnTitleChanged;

            mainPanel.Controls.Add(browser);
        }

        private void OnTitleChanged(object sender, TitleChangedEventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() =>
            {
                this.Text = $"{e.Title} - CEF Browser";
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            browser?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
