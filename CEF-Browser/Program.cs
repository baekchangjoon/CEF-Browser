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
        [STAThread]
        private static void Main(string[] args)
        {
            var parser = new CommandLineParser(args);
            var settingsFactory = new CefSettingsFactory();
            var settings = settingsFactory.Create(parser);
            
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var navigationService = new NavigationService();
            var parsedUrl = parser.ParseUrl();
            var initialUrl = navigationService.NormalizeUrl(parsedUrl);
            var mainForm = new MainForm(initialUrl);
            Application.Run(mainForm);

            Cef.Shutdown();
        }
    }
}
