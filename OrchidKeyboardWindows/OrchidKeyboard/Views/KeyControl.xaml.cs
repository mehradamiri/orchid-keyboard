using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using OrchidKeyboard.Models;

namespace OrchidKeyboard.Views;

public partial class KeyControl : UserControl
{
    private const double BaseSize = 48;
    private const double KeySpacing = 4;

    private PhysicalKey? _physicalKey;
    private bool _isSelected;
    private bool _isMapped;

    public event Action<string>? KeyClicked;

    public KeyControl()
    {
        InitializeComponent();
    }

    public void Setup(PhysicalKey key, KeyMapping? mapping, bool isSelected)
    {
        _physicalKey = key;
        _isSelected = isSelected;

        double width = BaseSize * key.RelativeWidth + (key.RelativeWidth - 1) * KeySpacing;
        Width = width;
        Height = BaseSize;

        _isMapped = mapping != null
            && !string.IsNullOrEmpty(mapping.Lowercase.Target)
            && mapping.Lowercase.Source != mapping.Lowercase.Target;

        if (key.IsModifier)
        {
            ModifierLabel.Text = key.Label;
            ModifierLabel.Visibility = Visibility.Visible;
            KeyContent.Visibility = Visibility.Collapsed;
            Cursor = Cursors.Arrow;
            KeyBorder.Background = new SolidColorBrush(Color.FromArgb(80, 200, 200, 200));
        }
        else
        {
            ModifierLabel.Visibility = Visibility.Collapsed;
            KeyContent.Visibility = Visibility.Visible;
            SourceLabel.Text = key.LowercaseChar;
            TargetLabel.Text = mapping?.Lowercase.Target ?? "";
            Cursor = Cursors.Hand;

            if (_isMapped)
            {
                TargetLabel.Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                KeyBorder.Background = new LinearGradientBrush(
                    Color.FromArgb(255, 230, 230, 230),
                    Color.FromArgb(218, 230, 230, 230),
                    90);
            }
            else
            {
                TargetLabel.Foreground = new SolidColorBrush(Color.FromArgb(60, 128, 128, 128));
                KeyBorder.Background = new SolidColorBrush(Color.FromArgb(50, 200, 200, 200));
            }
        }

        UpdateSelectionVisual();
    }

    private void UpdateSelectionVisual()
    {
        if (_isSelected)
        {
            var accent = Color.FromRgb(108, 92, 231); // #6C5CE7
            KeyBorder.BorderBrush = new SolidColorBrush(accent);
            KeyShadow.Color = accent;
            KeyShadow.Opacity = 0.4;
            KeyShadow.BlurRadius = 12;
        }
        else
        {
            KeyBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
            KeyShadow.Color = Colors.Black;
            KeyShadow.Opacity = 0.05;
            KeyShadow.BlurRadius = 4;
        }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (_physicalKey?.IsModifier == true) return;

        var anim = new DoubleAnimation(1.08, TimeSpan.FromMilliseconds(150))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        KeyScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        KeyScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);

        if (!_isSelected)
        {
            KeyShadow.Opacity = 0.15;
            KeyShadow.BlurRadius = 8;
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (_physicalKey?.IsModifier == true) return;

        var anim = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        KeyScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        KeyScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);

        if (!_isSelected)
        {
            KeyShadow.Opacity = 0.05;
            KeyShadow.BlurRadius = 4;
        }
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        if (_physicalKey?.IsModifier == true) return;
        if (_physicalKey != null)
            KeyClicked?.Invoke(_physicalKey.Id);
    }
}
