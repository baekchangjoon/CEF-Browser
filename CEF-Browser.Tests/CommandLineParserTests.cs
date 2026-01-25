using System;
using NUnit.Framework;
using CEF_Browser;

namespace CEF_Browser.Tests
{
    /// <summary>
    /// Tests for command line argument parsing
    /// </summary>
    [TestFixture]
    public class CommandLineParserTests
    {
        [Test]
        public void ParseUrl_WithUrlArgument_ReturnsUrl()
        {
            string[] args = { "https://www.google.com" };
            var parser = new CommandLineParser(args);
            string url = parser.ParseUrl();
            Assert.AreEqual("https://www.google.com", url);
        }

        [Test]
        public void ParseUrl_WithUserDataDir_ReturnsUrl()
        {
            string[] args = { "--user-data-dir", "C:\\Data", "https://www.naver.com" };
            var parser = new CommandLineParser(args);
            string url = parser.ParseUrl();
            Assert.AreEqual("https://www.naver.com", url);
        }

        [Test]
        public void ParseUrl_NoUrl_ReturnsGoogle()
        {
            string[] args = { "--user-data-dir", "C:\\Data" };
            var parser = new CommandLineParser(args);
            string url = parser.ParseUrl();
            Assert.AreEqual("www.google.com", url);
        }

        [Test]
        public void ParseUserDataDir_WithArgument_ReturnsPath()
        {
            string[] args = { "--user-data-dir", "C:\\MyData" };
            var parser = new CommandLineParser(args);
            string path = parser.ParseUserDataDir();
            Assert.AreEqual("C:\\MyData", path);
        }

        [Test]
        public void ParseUserDataDir_WithoutArgument_ReturnsNull()
        {
            string[] args = { "https://www.google.com" };
            var parser = new CommandLineParser(args);
            string path = parser.ParseUserDataDir();
            Assert.IsNull(path);
        }

        [Test]
        public void ParseUrl_WithNullArgs_ReturnsGoogle()
        {
            var parser = new CommandLineParser(null);
            string url = parser.ParseUrl();
            Assert.AreEqual("www.google.com", url);
        }
    }
}
