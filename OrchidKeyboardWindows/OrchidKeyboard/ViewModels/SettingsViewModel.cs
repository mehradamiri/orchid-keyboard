using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using OrchidKeyboard.Models;
using OrchidKeyboard.Services;

namespace OrchidKeyboard.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly MappingStore _store = MappingStore.Shared;

    private string? _selectedKeyId;
    public string? SelectedKeyId
    {
        get => _selectedKeyId;
        set { _selectedKeyId = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedPhysicalKey)); OnPropertyChanged(nameof(HasSelectedKey)); }
    }

    public bool HasSelectedKey => SelectedKeyId != null;

    public PhysicalKey? SelectedPhysicalKey
    {
        get
        {
            if (SelectedKeyId == null) return null;
            return KeyboardLayoutData.MappableKeys.FirstOrDefault(k => k.Id == SelectedKeyId);
        }
    }

    public MappingStore Store => _store;

    public string ProfileName => _store.ActiveProfile.Name;

    public int MappedKeyCount => _store.ActiveProfile.Mappings
        .Count(m => !string.IsNullOrEmpty(m.Lowercase.Target) && m.Lowercase.Source != m.Lowercase.Target);

    private bool _launchAtLogin;
    public bool LaunchAtLogin
    {
        get => _launchAtLogin;
        set
        {
            _launchAtLogin = value;
            AutoStartManager.IsEnabled = value;
            OnPropertyChanged();
        }
    }

    private bool _hideMenuBarIcon;
    public bool HideMenuBarIcon
    {
        get => _hideMenuBarIcon;
        set
        {
            _hideMenuBarIcon = value;
            OnPropertyChanged();
            if (Application.Current is App app)
                app.SetTrayIconVisible(!value);
        }
    }

    private bool _showSetupWizard;
    public bool ShowSetupWizard
    {
        get => _showSetupWizard;
        set { _showSetupWizard = value; OnPropertyChanged(); }
    }

    public ICommand ClearAllCommand { get; }
    public ICommand OpenWizardCommand { get; }
    public ICommand DonateCommand { get; }

    public SettingsViewModel()
    {
        _launchAtLogin = AutoStartManager.IsEnabled;

        ClearAllCommand = new RelayCommand(() =>
        {
            _store.ClearAllMappings();
            SelectedKeyId = null;
            Refresh();
        });

        OpenWizardCommand = new RelayCommand(() => ShowSetupWizard = true);

        DonateCommand = new RelayCommand(() =>
        {
            Process.Start(new ProcessStartInfo("https://buymeacoffee.com/orchidkeyboard") { UseShellExecute = true });
        });

        _store.PropertyChanged += (_, _) => Refresh();
    }

    public void SaveKeyEdit(string keyId, string lowercaseTarget, string uppercaseTarget)
    {
        var key = KeyboardLayoutData.MappableKeys.FirstOrDefault(k => k.Id == keyId);
        if (key == null) return;

        var lc = new CharacterPair(key.LowercaseChar, lowercaseTarget);
        var uc = new CharacterPair(key.UppercaseChar, uppercaseTarget);
        _store.UpdateMapping(keyId, lc, uc);
        SelectedKeyId = null;
        Refresh();
    }

    public void ClearKeyEdit(string keyId)
    {
        _store.ClearMapping(keyId);
        SelectedKeyId = null;
        Refresh();
    }

    public void ToggleKeySelection(string keyId)
    {
        SelectedKeyId = SelectedKeyId == keyId ? null : keyId;
    }

    public KeyMapping? GetMapping(string keyId) => _store.GetMapping(keyId);

    public void Refresh()
    {
        OnPropertyChanged(nameof(MappedKeyCount));
        OnPropertyChanged(nameof(ProfileName));
        OnPropertyChanged(nameof(Store));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
}
