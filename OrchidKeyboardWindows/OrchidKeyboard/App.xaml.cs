using System.Drawing;
using System.IO;
using System.Windows;
using OrchidKeyboard.Models;
using OrchidKeyboard.Services;
using OrchidKeyboard.Views;

namespace OrchidKeyboard;

public partial class App : Application
{
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private HotkeyManager? _hotkeyManager;
    private Window? _hiddenWindow;
    private SettingsWindow? _settingsWindow;
    private bool _isEnabled = true;
    private System.Windows.Forms.ToolStripMenuItem? _enabledMenuItem;
    private Mutex? _singleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Single-instance check
        _singleInstanceMutex = new Mutex(true, "OrchidKeyboard_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("Orchid Keyboard is already running.", "Orchid Keyboard",
                MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        // Create hidden window for hotkey message pump
        _hiddenWindow = new Window
        {
            Width = 0,
            Height = 0,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = false,
        };
        _hiddenWindow.Show();
        _hiddenWindow.Hide();

        SetupTrayIcon();
        SetupHotkey();

        // On first launch, open settings (which triggers the wizard)
        if (MappingStore.Shared.IsFirstLaunch)
        {
            OpenSettings();
        }
    }

    private void SetupTrayIcon()
    {
        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = LoadTrayIcon(),
            Text = "Orchid Keyboard",
            Visible = true,
        };

        var menu = new System.Windows.Forms.ContextMenuStrip();

        _enabledMenuItem = new System.Windows.Forms.ToolStripMenuItem("Enabled")
        {
            Checked = _isEnabled,
            CheckOnClick = true,
        };
        _enabledMenuItem.CheckedChanged += (_, _) => ToggleEnabled();
        menu.Items.Add(_enabledMenuItem);

        var settingsItem = new System.Windows.Forms.ToolStripMenuItem("Settings...");
        settingsItem.Click += (_, _) => Dispatcher.Invoke(OpenSettings);
        menu.Items.Add(settingsItem);

        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        var quitItem = new System.Windows.Forms.ToolStripMenuItem("Quit");
        quitItem.Click += (_, _) => Dispatcher.Invoke(QuitApp);
        menu.Items.Add(quitItem);

        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick += (_, _) => Dispatcher.Invoke(OpenSettings);
    }

    private static Icon LoadTrayIcon()
    {
        // Try to load embedded icon resource
        try
        {
            var uri = new Uri("pack://application:,,,/Resources/keyboard.ico", UriKind.Absolute);
            var streamInfo = GetResourceStream(uri);
            if (streamInfo != null)
                return new Icon(streamInfo.Stream);
        }
        catch { /* fallback below */ }

        // Fallback: generate a simple icon programmatically
        return CreateFallbackIcon();
    }

    private static Icon CreateFallbackIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using var g = Graphics.FromImage(bmp);
        g.Clear(System.Drawing.Color.Transparent);

        using var pen = new System.Drawing.Pen(System.Drawing.Color.White, 2);
        g.DrawRectangle(pen, 4, 8, 24, 16);

        // Draw keyboard-like dots
        using var brush = new SolidBrush(System.Drawing.Color.White);
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                g.FillRectangle(brush, 8 + col * 5, 12 + row * 6, 3, 3);
            }
        }

        var handle = bmp.GetHicon();
        return Icon.FromHandle(handle);
    }

    private void SetupHotkey()
    {
        _hotkeyManager = new HotkeyManager();
        _hotkeyManager.HotkeyPressed += OnHotkeyPressed;
        _hotkeyManager.Register(_hiddenWindow!);
    }

    private void OnHotkeyPressed()
    {
        if (!_isEnabled) return;

        // Run on background thread to avoid blocking the UI message pump
        Task.Run(() =>
        {
            try { TextReplacer.ReplaceSelectedText(); }
            catch { /* swallow errors */ }
        });
    }

    private void ToggleEnabled()
    {
        _isEnabled = _enabledMenuItem?.Checked ?? true;

        if (_isEnabled)
            _hotkeyManager?.Register(_hiddenWindow!);
        else
            _hotkeyManager?.Unregister();
    }

    public void OpenSettings()
    {
        if (_settingsWindow != null)
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow();
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    public void SetTrayIconVisible(bool visible)
    {
        if (_notifyIcon != null)
            _notifyIcon.Visible = visible;
    }

    private void QuitApp()
    {
        _hotkeyManager?.Dispose();
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        Shutdown();
    }
}
