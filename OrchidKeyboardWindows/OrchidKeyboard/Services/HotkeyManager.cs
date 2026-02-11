using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace OrchidKeyboard.Services;

public class HotkeyManager : IDisposable
{
    private const int HOTKEY_ID = 0x4F52; // 'OR'
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint VK_T = 0x54;
    private const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private HwndSource? _hwndSource;
    private IntPtr _hWnd;
    private bool _registered;

    public event Action? HotkeyPressed;

    public void Register(Window hiddenWindow)
    {
        if (_registered) return;

        var helper = new WindowInteropHelper(hiddenWindow);
        helper.EnsureHandle();
        _hWnd = helper.Handle;

        _hwndSource = HwndSource.FromHwnd(_hWnd);
        _hwndSource?.AddHook(WndProc);

        _registered = RegisterHotKey(_hWnd, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_T);
    }

    public void Unregister()
    {
        if (!_registered) return;

        UnregisterHotKey(_hWnd, HOTKEY_ID);
        _hwndSource?.RemoveHook(WndProc);
        _registered = false;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            HotkeyPressed?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
        GC.SuppressFinalize(this);
    }
}
