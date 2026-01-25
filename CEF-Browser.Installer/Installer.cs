using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WixSharp;

namespace CEF_Browser_Installer
{
    /// <summary>
    /// WiX installer setup for CEF Browser
    /// </summary>
    public class Installer
    {
        public static void Main(string[] args)
        {
            // Try to find WiX in common locations or PATH
            var possiblePaths = new[]
            {
                Environment.GetEnvironmentVariable("WIX_BIN_PATH"),
                @"C:\Program Files (x86)\WiX Toolset v3.14\bin",
                @"C:\Program Files (x86)\WiX Toolset v3.11\bin",
                Environment.GetEnvironmentVariable("RUNNER_TEMP") + @"\WiX\bin"
            };

            string wixBinPath = null;
            foreach (var path in possiblePaths)
            {
                if (!string.IsNullOrEmpty(path) && System.IO.Directory.Exists(path))
                {
                    wixBinPath = path;
                    break;
                }
            }

            // Try to find WiX in PATH
            if (wixBinPath == null)
            {
                var pathEnv = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(pathEnv))
                {
                    var paths = pathEnv.Split(System.IO.Path.PathSeparator);
                    foreach (var path in paths)
                    {
                        var candlePath = System.IO.Path.Combine(path, "candle.exe");
                        if (System.IO.File.Exists(candlePath))
                        {
                            wixBinPath = path;
                            break;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(wixBinPath))
            {
                WixSharp.Compiler.WixLocation = wixBinPath;
                Console.WriteLine($"WiX Toolset found at: {wixBinPath}");
            }
            else
            {
                Console.WriteLine("Warning: WiX Toolset not found. WixSharp will attempt to find it automatically.");
            }

            // Find the actual build output directory
            var installerDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var currentDir = Environment.CurrentDirectory;

            // Try multiple strategies to find CEF-Browser.exe
            var searchPaths = new List<string>();

            // Strategy 1: Relative paths from installer directory
            searchPaths.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(installerDir, @"..\..\..\CEF-Browser\bin\Release\net48")));
            searchPaths.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(installerDir, @"..\..\..\CEF-Browser\bin\x86\Release\net48")));
            searchPaths.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(installerDir, @"..\..\..\..\CEF-Browser\bin\Release\net48")));
            searchPaths.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(installerDir, @"..\..\..\..\CEF-Browser\bin\x86\Release\net48")));

            // Strategy 2: Relative paths from current directory
            searchPaths.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(currentDir, @"..\CEF-Browser\bin\Release\net48")));
            searchPaths.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(currentDir, @"..\CEF-Browser\bin\x86\Release\net48")));
            searchPaths.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(currentDir, @"CEF-Browser\bin\Release\net48")));
            searchPaths.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(currentDir, @"CEF-Browser\bin\x86\Release\net48")));

            // Strategy 3: Recursive search from solution root
            var solutionRoot = installerDir;
            for (int i = 0; i < 5; i++)
            {
                var testPath1 = System.IO.Path.Combine(solutionRoot, @"CEF-Browser\bin\Release\net48");
                var testPath2 = System.IO.Path.Combine(solutionRoot, @"CEF-Browser\bin\x86\Release\net48");
                if (System.IO.Directory.Exists(testPath1) || System.IO.Directory.Exists(testPath2))
                {
                    searchPaths.Add(System.IO.Path.GetFullPath(testPath1));
                    searchPaths.Add(System.IO.Path.GetFullPath(testPath2));
                    break;
                }
                solutionRoot = System.IO.Path.GetDirectoryName(solutionRoot);
                if (string.IsNullOrEmpty(solutionRoot)) break;
            }

            string buildOutputPath = null;
            string exePath = null;

            // Remove duplicates and check each path
            foreach (var path in searchPaths.Distinct())
            {
                var testExePath = System.IO.Path.Combine(path, "CEF-Browser.exe");
                if (System.IO.File.Exists(testExePath))
                {
                    buildOutputPath = path;
                    exePath = testExePath;
                    Console.WriteLine($"Found build output at: {buildOutputPath}");
                    break;
                }
            }

            if (string.IsNullOrEmpty(buildOutputPath) || string.IsNullOrEmpty(exePath))
            {
                Console.WriteLine("Error: CEF-Browser.exe not found in expected locations");
                Console.WriteLine($"Installer directory: {installerDir}");
                Console.WriteLine($"Current directory: {currentDir}");
                Console.WriteLine("Searched paths:");
                foreach (var path in searchPaths.Distinct())
                {
                    Console.WriteLine($"  - {path}");
                    if (System.IO.Directory.Exists(path))
                    {
                        var files = System.IO.Directory.GetFiles(path, "*.exe");
                        Console.WriteLine($"    (Directory exists, files: {string.Join(", ", files)})");
                    }
                }
                Environment.Exit(1);
            }

            // Change current directory to build output path so WixSharp can resolve relative paths correctly
            var originalDir = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = buildOutputPath;
                Console.WriteLine($"Changed working directory to: {buildOutputPath}");

                var exeFile = new File("CEF-Browser.exe")
                {
                    Shortcuts = new[]
                    {
                        new FileShortcut("CEF Browser", @"%Desktop%")
                        {
                            IconFile = "CEF-Browser.exe"
                        }
                    }
                };

                // Include supporting files (dlls, configs, etc.) but exclude CEF-Browser.exe
                // to avoid ICE30 error (duplicate component)
                // CEF-Browser.exe is already included via exeFile above
                var otherFiles = new Files(@"*.dll")
                {
                    Name = "Supporting DLLs"
                };

                var project = new ManagedProject("CEF Browser",
                    new Dir(@"%ProgramFiles%\CEF Browser",
                        exeFile,
                        otherFiles,
                        new Files(@"*.config"),
                        new Files(@"*.json"),
                        new Files(@"*.pak"),
                        new Files(@"*.dat"),
                        new Files(@"*.bin")
                    ));

                project.GUID = new Guid("B2C3D4E5-F6A7-5B6C-9D0E-1F2A3B4C5D6E");
                project.Version = new Version("1.0.0");
                
                // Calculate absolute path for output directory
                var outputDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(originalDir, @"..\InstallerOutput"));
                project.OutDir = outputDir;
                project.OutFileName = "CEF-Browser-Setup";

                project.BuildMsi();
            }
            finally
            {
                Environment.CurrentDirectory = originalDir;
            }

        }
    }
}
