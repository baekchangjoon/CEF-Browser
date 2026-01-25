using System;
using System.IO;
using CefSharp;
using CefSharp.WinForms;

namespace CEF_Browser
{
    /// <summary>
    /// Factory for creating CefSettings with proper configuration
    /// </summary>
    public class CefSettingsFactory
    {
        private const int DefaultRemoteDebuggingPort = 19222;

        /// <summary>
        /// Creates CefSettings with absolute path for BrowserSubprocessPath
        /// </summary>
        /// <param name="parser">Command line parser instance</param>
        /// <param name="remoteDebuggingPort">Port for CEF remote debugging (internal)</param>
        /// <returns>Configured CefSettings instance</returns>
        public virtual CefSettings Create(CommandLineParser parser, int remoteDebuggingPort)
        {
            var settings = new CefSettings();
            var subprocessPath = GetBrowserSubprocessPath();
            settings.BrowserSubprocessPath = subprocessPath;
            settings.RemoteDebuggingPort = remoteDebuggingPort;
            settings.CefCommandLineArgs.Add("disable-web-security", "1");

            // Enable remote debugging and DevTools
            // These settings help ensure CDP messages are properly routed
            settings.CefCommandLineArgs.Add("remote-debugging-port", remoteDebuggingPort.ToString());
            settings.CefCommandLineArgs.Add("remote-allow-origins", "*");

            var userDataDir = parser.ParseUserDataDir();
            if (!string.IsNullOrEmpty(userDataDir))
            {
                settings.CefCommandLineArgs.Add("user-data-dir", userDataDir);
            }

            return settings;
        }

        /// <summary>
        /// Gets the absolute path for BrowserSubprocess executable
        /// </summary>
        /// <returns>Absolute path to CefSharp.BrowserSubprocess.exe</returns>
        protected virtual string GetBrowserSubprocessPath()
        {
            var subprocessPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "CefSharp.BrowserSubprocess.exe");
            return Path.GetFullPath(subprocessPath);
        }
    }
}
