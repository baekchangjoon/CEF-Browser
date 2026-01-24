using System;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace CEF_Browser
{
    /// <summary>
    /// Main browser form with navigation controls
    /// </summary>
    public partial class MainForm : Form
    {
        private ChromiumWebBrowser browser;
        private TextBox addressBar;
        private Button backButton;
        private Button forwardButton;
        private Button refreshButton;
        private Button goButton;
        private readonly NavigationService navigationService;

        public MainForm(string initialUrl = "about:blank") 
            : this(initialUrl, new NavigationService())
        {
        }

        public MainForm(string initialUrl, NavigationService navigationService)
        {
            this.navigationService = navigationService;
            InitializeComponent();
            InitializeBrowser(initialUrl);
        }

        private void InitializeBrowser(string initialUrl)
        {
            browser = new ChromiumWebBrowser(initialUrl);
            browser.Dock = DockStyle.Fill;
            browser.AddressChanged += OnAddressChanged;
            browser.TitleChanged += OnTitleChanged;
            browser.LoadingStateChanged += OnLoadingStateChanged;

            mainPanel.Controls.Add(browser);
        }

        private void OnAddressChanged(object sender, AddressChangedEventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() =>
            {
                addressBar.Text = e.Address;
            });
        }

        private void OnTitleChanged(object sender, TitleChangedEventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() =>
            {
                this.Text = $"{e.Title} - CEF Browser";
            });
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() =>
            {
                backButton.Enabled = e.CanGoBack;
                forwardButton.Enabled = e.CanGoForward;
            });
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            if (browser.CanGoBack)
            {
                browser.Back();
            }
        }

        private void ForwardButton_Click(object sender, EventArgs e)
        {
            if (browser.CanGoForward)
            {
                browser.Forward();
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            browser.Reload();
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            NavigateToUrl(addressBar.Text);
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NavigateToUrl(addressBar.Text);
                e.Handled = true;
            }
        }

        private void NavigateToUrl(string url)
        {
            var normalizedUrl = navigationService.NormalizeUrl(url);
            if (normalizedUrl != null)
            {
                browser.LoadUrl(normalizedUrl);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            browser?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
