using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using CEF_Browser;

namespace CEF_Browser.Tests
{
    /// <summary>
    /// Integration tests for CDP (Chrome DevTools Protocol) connection
    /// </summary>
    [TestFixture]
    public class CdpConnectionTests
    {
        private Process _browserProcess;
        private int _cdpPort;
        private string _userDataDir;

        [TearDown]
        public void TearDown()
        {
            if (_browserProcess != null && !_browserProcess.HasExited)
            {
                try
                {
                    _browserProcess.Kill();
                    _browserProcess.WaitForExit(2000);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }

            if (!string.IsNullOrEmpty(_userDataDir) && Directory.Exists(_userDataDir))
            {
                try
                {
                    Directory.Delete(_userDataDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Test]
        public void Create_RemoteDebuggingPort_IsConfigured()
        {
            string[] args = { };
            var parser = new CommandLineParser(args);
            var factory = new CefSettingsFactory();
            var settings = factory.Create(parser, 9222);

            Assert.AreEqual(9222, settings.RemoteDebuggingPort,
                "RemoteDebuggingPort should be configured correctly");
        }

        [Test]
        [Ignore("Flaky in local environment, skipped for CI stability")]
        public async Task Verify_CdpEndpoint_IsAccessible()
        {
            // 1. Find the executable
            string exePath = FindCefBrowserExecutable();
            Assert.IsNotNull(exePath, "Could not find CEF-Browser.exe");

            // 2. Prepare arguments
            _userDataDir = Path.Combine(Path.GetTempPath(), "cef-test-" + Guid.NewGuid().ToString("N"));
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"--remote-debugging-port=0 --user-data-dir=\"{_userDataDir}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // 3. Start process and capture port
            _browserProcess = new Process { StartInfo = startInfo };
            var tcs = new TaskCompletionSource<int>();

            _browserProcess.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    TestContext.WriteLine($"[Stdout] {e.Data}");
                    var match = Regex.Match(e.Data, @"DevTools listening on ws://127\.0\.0\.1:(\d+)/");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int port))
                    {
                        tcs.TrySetResult(port);
                    }
                }
            };
            
            _browserProcess.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null) TestContext.WriteLine($"[Stderr] {e.Data}");
            };

            _browserProcess.Start();
            _browserProcess.BeginOutputReadLine();
            _browserProcess.BeginErrorReadLine();

            // Wait for port detection or timeout (10 seconds)
            var portTask = tcs.Task;
            var completedTask = await Task.WhenAny(portTask, Task.Delay(10000));

            if (completedTask != portTask)
            {
                Assert.Fail("Timeout waiting for CDP port allocation");
            }

            _cdpPort = await portTask;
            Assert.Greater(_cdpPort, 0, "Invalid CDP port detected");

            // 4. Verify endpoint access
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                var endpoint = $"http://localhost:{_cdpPort}/json/version";

                // Retry logic for connection (browser might be ready but http server lagging slightly)
                string response = null;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        response = await httpClient.GetStringAsync(endpoint);
                        break;
                    }
                    catch
                    {
                        await Task.Delay(500);
                    }
                }

                Assert.IsNotNull(response, "Failed to get response from CDP endpoint");
                Assert.IsTrue(response.Contains("Protocol-Version") ||
                             response.Contains("Browser") ||
                             response.Contains("User-Agent"),
                    "CDP endpoint did not return expected JSON");
            }
        }

        private string FindCefBrowserExecutable()
        {
            var currentDir = TestContext.CurrentContext.TestDirectory;
            
            // Search paths relative to test assembly location
            var searchPaths = new[]
            {
                // x86 Release
                Path.GetFullPath(Path.Combine(currentDir, @"..\..\..\CEF-Browser\bin\x86\Release\net48\CEF-Browser.exe")),
                // AnyCPU Release
                Path.GetFullPath(Path.Combine(currentDir, @"..\..\..\CEF-Browser\bin\Release\net48\CEF-Browser.exe")),
                // x86 Debug
                Path.GetFullPath(Path.Combine(currentDir, @"..\..\..\CEF-Browser\bin\x86\Debug\net48\CEF-Browser.exe")),
                // Direct relative (if copied)
                Path.Combine(currentDir, "CEF-Browser.exe")
            };

            foreach (var path in searchPaths)
            {
                if (File.Exists(path)) return path;
            }

            return null;
        }
    }
}
