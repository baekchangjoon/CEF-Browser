using System;

namespace CEF_Browser
{
    /// <summary>
    /// Service for handling URL navigation logic
    /// </summary>
    public class NavigationService
    {
        public string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                return "https://" + url;
            }

            return url;
        }
    }
}
