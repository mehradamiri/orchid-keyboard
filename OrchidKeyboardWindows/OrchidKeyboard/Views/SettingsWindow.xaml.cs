using System.Diagnostics;
using System.Windows;
using OrchidKeyboard.Models;
using OrchidKeyboard.Services;
using OrchidKeyboard.ViewModels;

namespace OrchidKeyboard.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _vm = new();

    public SettingsWindow()
    {
        InitializeComponent();
        DataContext = _vm;

        LaunchAtLoginCheck.IsChecked = _vm.LaunchAtLogin;
        HideTrayCheck.IsChecked = _vm.HideMenuBarIcon;

        KeyboardDisplay.KeyClicked += OnKeyClicked;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RefreshUI();

        if (MappingStore.Shared.IsFirstLaunch)
            OpenSetupWizard();
    }

    private void RefreshUI()
    {
        ProfileNameText.Text = _vm.ProfileName;
        MappedCountText.Text = $"{_vm.MappedKeyCount} keys mapped";
        KeyboardDisplay.Render(_vm.Store, _vm.SelectedKeyId);
    }

    private void OnKeyClicked(string keyId)
    {
        _vm.ToggleKeySelection(keyId);

        if (_vm.SelectedKeyId != null)
        {
            var key = _vm.SelectedPhysicalKey;
            if (key != null)
            {
                var mapping = _vm.GetMapping(key.Id);
                var popup = new KeyEditPopup(key, mapping) { Owner = this };
                if (popup.ShowDialog() == true)
                {
                    switch (popup.Action)
                    {
                        case KeyEditPopup.ResultAction.Save:
                            _vm.SaveKeyEdit(key.Id, popup.LowercaseTarget, popup.UppercaseTarget);
                            break;
                        case KeyEditPopup.ResultAction.Clear:
                            _vm.ClearKeyEdit(key.Id);
                            break;
                    }
                }
                else
                {
                    _vm.SelectedKeyId = null;
                }
            }
        }

        RefreshUI();
    }

    private void OnClearAll(object sender, RoutedEventArgs e)
    {
        _vm.ClearAllCommand.Execute(null);
        RefreshUI();
    }

    private void OnOpenWizard(object sender, RoutedEventArgs e)
    {
        OpenSetupWizard();
    }

    private void OnDonate(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://buymeacoffee.com/orchidkeyboard") { UseShellExecute = true });
    }

    private void OnLaunchAtLoginChanged(object sender, RoutedEventArgs e)
    {
        _vm.LaunchAtLogin = LaunchAtLoginCheck.IsChecked == true;
    }

    private void OnHideTrayChanged(object sender, RoutedEventArgs e)
    {
        _vm.HideMenuBarIcon = HideTrayCheck.IsChecked == true;
    }

    private void OpenSetupWizard()
    {
        var wizard = new SetupWizardWindow { Owner = this };
        wizard.ShowDialog();
        _vm.Refresh();
        RefreshUI();
    }
}
