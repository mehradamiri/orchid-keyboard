using System.Windows;
using System.Windows.Input;
using OrchidKeyboard.Models;

namespace OrchidKeyboard.Views;

public partial class KeyEditPopup : Window
{
    public enum ResultAction { None, Save, Clear }

    public ResultAction Action { get; private set; } = ResultAction.None;
    public string LowercaseTarget => LowercaseInput.Text;
    public string UppercaseTarget => UppercaseInput.Text;

    public KeyEditPopup(PhysicalKey key, KeyMapping? currentMapping)
    {
        InitializeComponent();

        TitleText.Text = $"Edit Key: {key.Label}";
        LowercaseLabel.Text = $"Lowercase ({key.LowercaseChar})";
        UppercaseLabel.Text = $"Uppercase ({key.UppercaseChar})";

        LowercaseInput.Text = currentMapping?.Lowercase.Target ?? "";
        UppercaseInput.Text = currentMapping?.Uppercase.Target ?? "";

        Loaded += (_, _) => LowercaseInput.Focus();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        Action = ResultAction.Save;
        DialogResult = true;
    }

    private void OnClear(object sender, RoutedEventArgs e)
    {
        Action = ResultAction.Clear;
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        Action = ResultAction.None;
        DialogResult = false;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Action = ResultAction.None;
            DialogResult = false;
        }
    }
}
