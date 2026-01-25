using System;
using System.IO;
using NUnit.Framework;
using CEF_Browser;
using CefSharp;

namespace CEF_Browser.Tests
{
    /// <summary>
    /// Tests for CefSettingsFactory
    /// </summary>
    [TestFixture]
    public class CefSettingsFactoryTests
    {
        private CefSettingsFactory factory;

        [SetUp]
        public void Setup()
        {
            factory = new CefSettingsFactory();
        }

        [Test]
        public void Create_BrowserSubprocessPath_IsAbsolute()
        {
            string[] args = { };
            var parser = new CommandLineParser(args);
            var settings = factory.Create(parser);

            Assert.IsTrue(Path.IsPathRooted(settings.BrowserSubprocessPath),
                "BrowserSubprocessPath should be an absolute path");
            Assert.IsTrue(settings.BrowserSubprocessPath.EndsWith(
                "CefSharp.BrowserSubprocess.exe",
                StringComparison.OrdinalIgnoreCase),
                "BrowserSubprocessPath should end with CefSharp.BrowserSubprocess.exe");
        }

        [Test]
        public void Create_RemoteDebuggingPort_IsSet()
        {
            string[] args = { };
            var parser = new CommandLineParser(args);
            var settings = factory.Create(parser);

            Assert.AreEqual(9222, settings.RemoteDebuggingPort);
        }

        [Test]
        public void Create_DisableWebSecurity_IsAdded()
        {
            string[] args = { };
            var parser = new CommandLineParser(args);
            var settings = factory.Create(parser);

            Assert.IsTrue(settings.CefCommandLineArgs.ContainsKey("disable-web-security"));
            Assert.AreEqual("1", settings.CefCommandLineArgs["disable-web-security"]);
        }

        [Test]
        public void Create_WithUserDataDir_AddsUserDataDirArg()
        {
            string[] args = { "--user-data-dir", "C:\\MyData" };
            var parser = new CommandLineParser(args);
            var settings = factory.Create(parser);

            Assert.IsTrue(settings.CefCommandLineArgs.ContainsKey("user-data-dir"));
            Assert.AreEqual("C:\\MyData", settings.CefCommandLineArgs["user-data-dir"]);
        }

        [Test]
        public void Create_WithoutUserDataDir_DoesNotAddUserDataDirArg()
        {
            string[] args = { };
            var parser = new CommandLineParser(args);
            var settings = factory.Create(parser);

            Assert.IsFalse(settings.CefCommandLineArgs.ContainsKey("user-data-dir"));
        }
    }
}