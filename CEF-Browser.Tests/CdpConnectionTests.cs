using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using CEF_Browser;
using CefSharp;

namespace CEF_Browser.Tests
{
    /// <summary>
    /// Integration tests for CDP (Chrome DevTools Protocol) connection
    /// </summary>
    [TestFixture]
    public class CdpConnectionTests
    {
        private const int RemoteDebuggingPort = 9222;
        private const string CdpVersionEndpoint = "http://localhost:9222/json/version";
        private const int TimeoutMs = 5000;

        [Test]
        public void Create_RemoteDebuggingPort_IsConfigured()
        {
            string[] args = { };
            var parser = new CommandLineParser(args);
            var factory = new CefSettingsFactory();
            var settings = factory.Create(parser, RemoteDebuggingPort);

            Assert.AreEqual(RemoteDebuggingPort, settings.RemoteDebuggingPort,
                "RemoteDebuggingPort should be set to 9222 for CDP support");
        }

        [Test]
        [Explicit("Requires running CEF-Browser instance")]
        public async Task Verify_CdpEndpoint_IsAccessible()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(TimeoutMs);

                try
                {
                    var response = await httpClient.GetStringAsync(CdpVersionEndpoint);
                    Assert.IsNotNull(response, "CDP version endpoint should return a response");
                    Assert.IsTrue(response.Contains("Protocol-Version") || 
                                 response.Contains("Browser") ||
                                 response.Contains("User-Agent"),
                        "CDP version endpoint should return valid JSON with CDP information");
                }
                catch (HttpRequestException ex)
                {
                    Assert.Fail($"CDP endpoint is not accessible. " +
                               $"Make sure CEF-Browser.exe is running. Error: {ex.Message}");
                }
                catch (TaskCanceledException)
                {
                    Assert.Fail($"CDP endpoint connection timeout. " +
                               $"Make sure CEF-Browser.exe is running and CDP is enabled.");
                }
            }
        }
    }
}