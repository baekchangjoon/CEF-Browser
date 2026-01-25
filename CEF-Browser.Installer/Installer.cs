using System;
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
            // Set WiX location if not found automatically
            var wixBinPath = @"C:\Program Files (x86)\WiX Toolset v3.14\bin";
            if (System.IO.Directory.Exists(wixBinPath))
            {
                WixSharp.Compiler.WixLocation = wixBinPath;
            }

            var exeFile = new File(@"..\CEF-Browser\bin\x86\Release\net48\CEF-Browser.exe")
            {
                Shortcuts = new[]
                {
                    new FileShortcut("CEF Browser", @"%Desktop%")
                    {
                        IconFile = @"..\CEF-Browser\bin\x86\Release\net48\CEF-Browser.exe"
                    }
                }
            };

            var project = new ManagedProject("CEF Browser",
                new Dir(@"%ProgramFiles%\CEF Browser",
                    exeFile,
                    new Files(@"..\CEF-Browser\bin\x86\Release\net48\*.*")
                ));

            project.GUID = new Guid("B2C3D4E5-F6A7-5B6C-9D0E-1F2A3B4C5D6E");
            project.Version = new Version("1.0.0");
            project.OutDir = @"..\InstallerOutput\";
            project.OutFileName = "CEF-Browser-Setup";

            project.BuildMsi();
        }
    }
}
