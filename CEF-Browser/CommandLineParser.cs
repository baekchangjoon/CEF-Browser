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
                if (IsUserDataDirArgument(i))
                {
                    i++;
                    continue;
                }

                if (!IsOptionArgument(args[i]))
                {
                    return args[i];
                }
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
            }

            return null;
        }

        private bool IsUserDataDirArgument(int index)
        {
            return args[index] == "--user-data-dir" && 
                   index + 1 < args.Length;
        }

        private bool IsOptionArgument(string arg)
        {
            return arg.StartsWith("--");
        }
    }
}
