using System;
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
        private const int DefaultRemoteDebuggingPort = 9222;

        [STAThread]
        private static void Main(string[] args)
        {
            var parser = new CommandLineParser(args);
            var settings = CreateCefSettings(parser);
            
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var initialUrl = parser.ParseUrl();
            var mainForm = new MainForm(initialUrl);
            Application.Run(mainForm);

            Cef.Shutdown();
        }

        private static CefSettings CreateCefSettings(CommandLineParser parser)
        {
            var settings = new CefSettings();
            settings.BrowserSubprocessPath = "CefSharp.BrowserSubprocess.exe";
            settings.RemoteDebuggingPort = DefaultRemoteDebuggingPort;
            settings.CefCommandLineArgs.Add("disable-web-security", "1");

            var userDataDir = parser.ParseUserDataDir();
            if (!string.IsNullOrEmpty(userDataDir))
            {
                settings.CefCommandLineArgs.Add("user-data-dir", userDataDir);
            }

            return settings;
        }
    }
}
