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
        private const int DefaultRemoteDebuggingPort = 9222;

        /// <summary>
        /// Creates CefSettings with absolute path for BrowserSubprocessPath
        /// </summary>
        /// <param name="parser">Command line parser instance</param>
        /// <returns>Configured CefSettings instance</returns>
        public virtual CefSettings Create(CommandLineParser parser)
        {
            var settings = new CefSettings();
            var subprocessPath = GetBrowserSubprocessPath();
            settings.BrowserSubprocessPath = subprocessPath;
            settings.RemoteDebuggingPort = DefaultRemoteDebuggingPort;
            settings.CefCommandLineArgs.Add("disable-web-security", "1");

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