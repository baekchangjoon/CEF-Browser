using NUnit.Framework;
using CEF_Browser;

namespace CEF_Browser.Tests
{
    /// <summary>
    /// Tests for NavigationService
    /// </summary>
    [TestFixture]
    public class NavigationServiceTests
    {
        private NavigationService navigationService;

        [SetUp]
        public void Setup()
        {
            navigationService = new NavigationService();
        }

        [Test]
        public void NormalizeUrl_WithHttpUrl_ReturnsSameUrl()
        {
            string url = "http://www.google.com";
            string result = navigationService.NormalizeUrl(url);
            Assert.AreEqual("http://www.google.com", result);
        }

        [Test]
        public void NormalizeUrl_WithHttpsUrl_ReturnsSameUrl()
        {
            string url = "https://www.naver.com";
            string result = navigationService.NormalizeUrl(url);
            Assert.AreEqual("https://www.naver.com", result);
        }

        [Test]
        public void NormalizeUrl_WithoutProtocol_AddsHttps()
        {
            string url = "www.google.com";
            string result = navigationService.NormalizeUrl(url);
            Assert.AreEqual("https://www.google.com", result);
        }

        [Test]
        public void NormalizeUrl_WithEmptyString_ReturnsNull()
        {
            string url = "";
            string result = navigationService.NormalizeUrl(url);
            Assert.IsNull(result);
        }

        [Test]
        public void NormalizeUrl_WithWhitespace_ReturnsNull()
        {
            string url = "   ";
            string result = navigationService.NormalizeUrl(url);
            Assert.IsNull(result);
        }

        [Test]
        public void NormalizeUrl_WithNull_ReturnsNull()
        {
            string result = navigationService.NormalizeUrl(null);
            Assert.IsNull(result);
        }
    }
}
