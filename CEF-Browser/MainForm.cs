using System;
using System.IO;
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
        private IRegistration cdpHandlerRegistration;

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
            browser.IsBrowserInitializedChanged += OnBrowserInitialized;

            mainPanel.Controls.Add(browser);

            // Try to register handler early if browser is already initialized
            // This handles the case where browser initializes before the event fires
            if (browser.IsBrowserInitialized)
            {
                System.Diagnostics.Debug.WriteLine("[CDP] Browser already initialized, registering handler immediately");
                RegisterCdpCommandHandler();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CDP] Browser not yet initialized, will register on IsBrowserInitializedChanged event");
            }
        }

        private void OnBrowserInitialized(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[CDP] OnBrowserInitialized event fired. IsBrowserInitialized: {browser.IsBrowserInitialized}");
            WriteRegistrationLog($"OnBrowserInitialized event fired. IsBrowserInitialized: {browser.IsBrowserInitialized}");

            if (browser.IsBrowserInitialized)
            {
                try
                {
                    var browserInstance = browser.GetBrowser();
                    if (browserInstance != null)
                    {
                        var browserInfo = $"Browser ID: {browserInstance.Identifier}, CanGoBack: {browserInstance.CanGoBack}, CanGoForward: {browserInstance.CanGoForward}";
                        WriteRegistrationLog($"Browser instance info: {browserInfo}");
                        System.Diagnostics.Debug.WriteLine($"[CDP] {browserInfo}");
                    }

                    var host = browserInstance?.GetHost();
                    if (host != null)
                    {
                        var hostInfo = $"HasDevTools: {host.HasDevTools}";
                        WriteRegistrationLog($"Browser host info: {hostInfo}");
                        System.Diagnostics.Debug.WriteLine($"[CDP] {hostInfo}");
                    }
                }
                catch (Exception ex)
                {
                    WriteRegistrationLog($"Error getting browser info: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[CDP ERROR] Error getting browser info: {ex.Message}");
                }

                // Only register if not already registered
                if (cdpHandlerRegistration == null)
                {
                    System.Diagnostics.Debug.WriteLine("[CDP] Registering handler from OnBrowserInitialized event");
                    WriteRegistrationLog("Registering handler from OnBrowserInitialized event");
                    RegisterCdpCommandHandler();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[CDP] Handler already registered, skipping");
                    WriteRegistrationLog("Handler already registered, skipping");
                }
            }
            else
            {
                WriteRegistrationLog("Browser not initialized yet, handler registration skipped");
            }
        }

        private void RegisterCdpCommandHandler()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[CDP] Attempting to register CDP Command Handler...");
                WriteRegistrationLog("Starting CDP handler registration");

                var browserInstance = browser.GetBrowser();
                if (browserInstance == null)
                {
                    var errorMsg = "Browser instance is null, cannot register CDP handler";
                    System.Diagnostics.Debug.WriteLine($"[CDP ERROR] {errorMsg}");
                    WriteRegistrationLog(errorMsg);
                    return;
                }

                WriteRegistrationLog($"Browser instance obtained - ID: {browserInstance.Identifier}, CanGoBack: {browserInstance.CanGoBack}");

                var host = browserInstance.GetHost();
                if (host == null)
                {
                    var errorMsg = "Browser host is null, cannot register CDP handler";
                    System.Diagnostics.Debug.WriteLine($"[CDP ERROR] {errorMsg}");
                    WriteRegistrationLog(errorMsg);
                    return;
                }

                WriteRegistrationLog("Browser host obtained successfully");

                // Check if DevTools is available
                var hasDevTools = host.HasDevTools;
                WriteRegistrationLog($"DevTools available: {hasDevTools}");
                System.Diagnostics.Debug.WriteLine($"[CDP] DevTools available: {hasDevTools}");

                var cdpHandler = new CdpCommandHandler();
                WriteRegistrationLog("CdpCommandHandler instance created");

                cdpHandlerRegistration = host.AddDevToolsMessageObserver(cdpHandler);

                if (cdpHandlerRegistration == null)
                {
                    var errorMsg = "AddDevToolsMessageObserver returned null registration";
                    System.Diagnostics.Debug.WriteLine($"[CDP ERROR] {errorMsg}");
                    WriteRegistrationLog(errorMsg);
                    return;
                }

                var successMsg = $"CDP Command Handler registered successfully. Registration object: {cdpHandlerRegistration.GetType().Name}";
                System.Diagnostics.Debug.WriteLine($"[CDP SUCCESS] {successMsg}");
                WriteRegistrationLog(successMsg);

                // Try to trigger DevTools agent attachment by sending a test message
                // This helps ensure the agent is attached and ready to receive messages
                try
                {
                    var testMessage = "{\"id\":0,\"method\":\"Runtime.enable\"}";
                    host.SendDevToolsMessage(testMessage);
                    WriteRegistrationLog("Sent test DevTools message to trigger agent attachment");
                    System.Diagnostics.Debug.WriteLine("[CDP] Sent test DevTools message to trigger agent attachment");
                }
                catch (Exception ex)
                {
                    WriteRegistrationLog($"Failed to send test DevTools message: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[CDP] Failed to send test DevTools message: {ex.Message}");
                }

                // Log CDP endpoint info
                var cdpEndpoint = $"http://localhost:9222";
                WriteRegistrationLog($"CDP endpoint should be available at: {cdpEndpoint}");
                System.Diagnostics.Debug.WriteLine($"[CDP] CDP endpoint should be available at: {cdpEndpoint}");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Failed to register CDP Command Handler: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[CDP ERROR] {errorMsg}");
                System.Diagnostics.Debug.WriteLine($"[CDP ERROR] Stack trace: {ex.StackTrace}");
                WriteRegistrationLog($"{errorMsg}\n{ex.StackTrace}");
            }
        }

        private void WriteRegistrationLog(string message)
        {
            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CEF-Browser", "Logs");
                Directory.CreateDirectory(logDir);
                var logFile = Path.Combine(logDir, "registration.log");
                File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
            catch
            {
                // Ignore logging errors
            }
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
            cdpHandlerRegistration?.Dispose();
            browser?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
