using System;
using System.Windows.Forms;

namespace CEF_Browser
{
    /// <summary>
    /// Extension methods for UI thread operations
    /// </summary>
    public static class Extensions
    {
        public static void InvokeOnUiThreadIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
