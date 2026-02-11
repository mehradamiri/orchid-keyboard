using System.Runtime.InteropServices;
using System.Windows;
using OrchidKeyboard.Models;

namespace OrchidKeyboard.Services;

public static class TextReplacer
{
    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUTUNION u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const ushort VK_CONTROL = 0x11;
    private const ushort VK_C = 0x43;
    private const ushort VK_V = 0x56;

    /// <summary>
    /// Called from a background thread so sleeps don't block the UI.
    /// All clipboard operations run on a dedicated STA thread.
    /// </summary>
    public static void ReplaceSelectedText()
    {
        // All clipboard ops must run on an STA thread
        var tcs = new TaskCompletionSource<bool>();
        var staThread = new Thread(() =>
        {
            try
            {
                DoReplace();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.IsBackground = true;
        staThread.Start();
        tcs.Task.Wait();
    }

    private static void DoReplace()
    {
        // 1. Save current clipboard state
        IDataObject? savedClipboard = null;
        try
        {
            savedClipboard = CloneClipboard();
        }
        catch { /* clipboard may be locked */ }

        // 2. Clear clipboard so we can detect when Ctrl+C writes to it
        try { Clipboard.Clear(); } catch { }
        uint clearedSeq = GetClipboardSequenceNumber();

        // 3. Simulate Ctrl+C to copy selected text
        SimulateCtrlKey(VK_C);

        // 4. Poll for clipboard change (up to 500ms)
        bool copied = false;
        for (int i = 0; i < 50; i++)
        {
            Thread.Sleep(10);
            if (GetClipboardSequenceNumber() != clearedSeq)
            {
                copied = true;
                break;
            }
        }

        // 5. Validate that we actually got new content
        if (!copied || !Clipboard.ContainsText())
        {
            RestoreClipboard(savedClipboard);
            return;
        }

        string selectedText = Clipboard.GetText();
        if (string.IsNullOrEmpty(selectedText))
        {
            RestoreClipboard(savedClipboard);
            return;
        }

        // 6. Convert text using MappingStore (auto-detects direction)
        string replacement = MappingStore.Shared.ConvertText(selectedText);

        // 7. Set clipboard to replacement text
        Clipboard.SetText(replacement);

        // 8. Simulate Ctrl+V to paste
        SimulateCtrlKey(VK_V);

        // 9. Restore original clipboard after paste completes
        Thread.Sleep(300);
        RestoreClipboard(savedClipboard);
    }

    private static IDataObject? CloneClipboard()
    {
        var current = Clipboard.GetDataObject();
        if (current == null) return null;

        var clone = new DataObject();
        foreach (var format in current.GetFormats())
        {
            try
            {
                var data = current.GetData(format);
                if (data != null)
                    clone.SetData(format, data);
            }
            catch { /* skip formats we can't read */ }
        }
        return clone;
    }

    private static void RestoreClipboard(IDataObject? savedData)
    {
        try
        {
            if (savedData != null)
                Clipboard.SetDataObject(savedData, true);
            else
                Clipboard.Clear();
        }
        catch { /* ignore restore errors */ }
    }

    private static void SimulateCtrlKey(ushort vkKey)
    {
        var inputs = new INPUT[4];
        int size = Marshal.SizeOf<INPUT>();

        // Ctrl down
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].u.ki.wVk = VK_CONTROL;

        // Key down
        inputs[1].type = INPUT_KEYBOARD;
        inputs[1].u.ki.wVk = vkKey;

        // Key up
        inputs[2].type = INPUT_KEYBOARD;
        inputs[2].u.ki.wVk = vkKey;
        inputs[2].u.ki.dwFlags = KEYEVENTF_KEYUP;

        // Ctrl up
        inputs[3].type = INPUT_KEYBOARD;
        inputs[3].u.ki.wVk = VK_CONTROL;
        inputs[3].u.ki.dwFlags = KEYEVENTF_KEYUP;

        SendInput(4, inputs, size);
    }
}
