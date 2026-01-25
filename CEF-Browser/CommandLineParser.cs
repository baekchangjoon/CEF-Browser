using System;

namespace CEF_Browser
{
    /// <summary>
    /// Parses command line arguments for CEF Browser
    /// </summary>
    public class CommandLineParser
    {
        private readonly string[] args;

        public CommandLineParser(string[] args)
        {
            this.args = args ?? new string[0];
        }

        public string ParseUrl()
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (IsOptionArgument(args[i]))
                {
                    // Skip value if it's a known option that takes a value
                    if (IsUserDataDirArgument(i) || IsRemoteDebuggingPortArgument(i))
                    {
                        i++;
                    }
                    continue;
                }

                return args[i];
            }

            return "www.google.com";
        }

        public string ParseUserDataDir()
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (IsUserDataDirArgument(i))
                {
                    return args[i + 1];
                }

                // Also support --user-data-dir=PATH format
                if (args[i].StartsWith("--user-data-dir="))
                {
                    return args[i].Substring("--user-data-dir=".Length);
                }
            }

            return null;
        }

        public int ParseRemoteDebuggingPort()
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (IsRemoteDebuggingPortArgument(i))
                {
                    if (int.TryParse(args[i + 1], out int port))
                    {
                        return port;
                    }
                }

                // Also support --remote-debugging-port=PORT format
                if (args[i].StartsWith("--remote-debugging-port="))
                {
                    var value = args[i].Substring("--remote-debugging-port=".Length);
                    if (int.TryParse(value, out int port))
                    {
                        return port;
                    }
                }
            }

            return -1; // Not specified
        }

        private bool IsUserDataDirArgument(int index)
        {
            return args[index] == "--user-data-dir" &&
                   index + 1 < args.Length;
        }

        private bool IsRemoteDebuggingPortArgument(int index)
        {
            return args[index] == "--remote-debugging-port" &&
                   index + 1 < args.Length;
        }

        private bool IsOptionArgument(string arg)
        {
            return arg.StartsWith("--");
        }
    }
}
