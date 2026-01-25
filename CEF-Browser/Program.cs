using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace CEF_Browser
{
    /// <summary>
    /// Main entry point for CEF Browser application
    /// </summary>
    internal static class Program
    {
        private static CdpProxyService _cdpProxy;

        [STAThread]
        private static void Main(string[] args)
        {
            var parser = new CommandLineParser(args);
            int requestedPort = parser.ParseRemoteDebuggingPort();
            int proxyPort = requestedPort;
            int cefPort = 19222; // Default internal port

            // Handle port allocation logic:
            // -1: Not specified -> Use default 9222
            // 0: Explicitly requested random port -> Use random port
            // >0: Explicitly requested specific port -> Use that port

            if (requestedPort == -1)
            {
                proxyPort = 9222;
            }
            else if (requestedPort == 0)
            {
                proxyPort = GetFreePort();
            }
            // else use requestedPort as is

            // Always allocate a free port for internal CEF usage to avoid conflicts in parallel execution
            cefPort = GetFreePort();

            // Ensure ports are different
            while (cefPort == proxyPort)
            {
                cefPort = GetFreePort();
            }

            // Start CDP Proxy
            try
            {
                _cdpProxy = new CdpProxyService(proxyPort, cefPort);
                _cdpProxy.Start();

                // Print the DevTools listening message that Playwright expects
                // The format MUST be exact for Playwright to parse it:
                // "DevTools listening on ws://127.0.0.1:{port}/devtools/browser/{uuid}"
                // But since we don't know the UUID yet (it comes from CEF), we just print the base message
                // Playwright's launchPersistentContext might need the full URL,
                // but connectOverCDP usually just needs the port if we handle it manually.
                // However, our test utility will parse this.
                Console.WriteLine($"DevTools listening on ws://127.0.0.1:{proxyPort}/devtools/browser/proxy");
                Console.WriteLine($"[CDP Proxy] Mapping {proxyPort} (External) -> {cefPort} (Internal)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start CDP Proxy: {ex.Message}", "CDP Proxy Error");
                return;
            }

            var settingsFactory = new CefSettingsFactory();
            // Pass the internal CEF port
            var settings = settingsFactory.Create(parser, cefPort);

            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var navigationService = new NavigationService();
            var parsedUrl = parser.ParseUrl();
            var initialUrl = navigationService.NormalizeUrl(parsedUrl);
            var mainForm = new MainForm(initialUrl);
            Application.Run(mainForm);

            _cdpProxy?.Stop();
            Cef.Shutdown();
        }

        private static int GetFreePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }
    }
}
