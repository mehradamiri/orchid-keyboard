using Microsoft.Win32;

namespace OrchidKeyboard.Services;

public static class AutoStartManager
{
    private const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "OrchidKeyboard";

    public static bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
            return key?.GetValue(AppName) != null;
        }
        set
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (key == null) return;

            if (value)
            {
                var exePath = Environment.ProcessPath ?? "";
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
    }
}
