using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OrchidKeyboard.Models;
using OrchidKeyboard.ViewModels;

namespace OrchidKeyboard.Views;

public partial class SetupWizardWindow : Window
{
    private readonly SetupWizardViewModel _vm = new();
    private TextBox? _activeInput;

    public SetupWizardWindow()
    {
        InitializeComponent();
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SetupWizardViewModel.CurrentStep))
                RenderStep();
        };
        Loaded += (_, _) => RenderStep();
    }

    private void RenderStep()
    {
        var step = _vm.CurrentStep;

        // Update progress
        StepLabel.Text = $"Step {step + 1} of {SetupWizardViewModel.TotalSteps}";
        if (step > 0 && step < 5)
            RowLabel.Text = SetupWizardViewModel.RowTitles[step - 1];
        else
            RowLabel.Text = "";

        // Progress bar fill
        double fraction = _vm.ProgressFraction;
        ProgressFill.Width = (ActualWidth > 0 ? ActualWidth - 48 : 572) * fraction;

        // Navigation buttons
        BackButton.Visibility = step > 0 && step < 5 ? Visibility.Visible : Visibility.Collapsed;
        NextButton.Content = step >= 5 ? "Finish" : "Next";
        NextButton.IsEnabled = _vm.CanAdvance;

        // Render content
        ContentArea.Children.Clear();

        switch (step)
        {
            case 0:
                RenderWelcomeStep();
                break;
            case >= 1 and <= 4:
                RenderRowStep(step - 1);
                break;
            case 5:
                RenderDoneStep();
                break;
        }
    }

    private void RenderWelcomeStep()
    {
        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        // Keyboard icon
        var icon = new TextBlock
        {
            Text = "\u2328", // keyboard symbol
            FontSize = 56,
            Foreground = new SolidColorBrush(Color.FromRgb(108, 92, 231)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16),
        };
        panel.Children.Add(icon);

        var title = new TextBlock
        {
            Text = "Set Up Your Keyboard",
            FontSize = 22,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12),
        };
        panel.Children.Add(title);

        var desc = new TextBlock
        {
            Text = "This wizard will guide you through mapping your keyboard layout\nrow by row. You'll type each row in your target language so\nOrchid can learn the mapping.",
            FontSize = 13,
            Foreground = Brushes.Gray,
            TextAlignment = TextAlignment.Center,
            LineHeight = 20,
            Margin = new Thickness(0, 0, 0, 24),
        };
        panel.Children.Add(desc);

        var fieldPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        fieldPanel.Children.Add(new TextBlock
        {
            Text = "Language name:",
            FontSize = 12,
            FontWeight = FontWeights.Medium,
            Margin = new Thickness(0, 0, 0, 6),
        });

        var input = new TextBox
        {
            Text = _vm.LanguageName,
            Width = 280,
            FontSize = 14,
            Padding = new Thickness(6, 4, 6, 4),
        };
        input.TextChanged += (_, _) =>
        {
            _vm.LanguageName = input.Text;
            NextButton.IsEnabled = _vm.CanAdvance;
        };
        fieldPanel.Children.Add(input);
        panel.Children.Add(fieldPanel);

        ContentArea.Children.Add(panel);
        input.Focus();
    }

    private void RenderRowStep(int rowIndex)
    {
        var chars = _vm.ParsedCharacters(rowIndex);
        int expected = MappingStore.ExpectedCounts[rowIndex];
        bool valid = chars.Length == expected;
        var sourceChars = MappingStore.RowSourceChars[rowIndex];

        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var panel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(40, 0, 40, 0),
        };

        // Title
        panel.Children.Add(new TextBlock
        {
            Text = SetupWizardViewModel.RowTitles[rowIndex],
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 20, 0, 8),
        });

        // Description
        panel.Children.Add(new TextBlock
        {
            Text = SetupWizardViewModel.RowDescriptions[rowIndex],
            FontSize = 12,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12),
        });

        // QWERTY key reference
        var keyRefPanel = new WrapPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16),
        };
        for (int i = 0; i < sourceChars.Length; i++)
        {
            bool filled = i < chars.Length;
            var keyBox = new Border
            {
                Width = 28,
                Height = 28,
                Margin = new Thickness(2),
                CornerRadius = new CornerRadius(4),
                Background = filled
                    ? new SolidColorBrush(Color.FromArgb(38, 108, 92, 231))
                    : new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                BorderBrush = filled
                    ? new SolidColorBrush(Color.FromRgb(108, 92, 231))
                    : new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(1),
            };
            keyBox.Child = new TextBlock
            {
                Text = sourceChars[i],
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            keyRefPanel.Children.Add(keyBox);
        }
        panel.Children.Add(keyRefPanel);

        // Input field with count
        var inputHeader = new Grid { Margin = new Thickness(0, 0, 0, 6) };
        inputHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        inputHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var inputLabel = new TextBlock
        {
            Text = $"Type the {expected} characters:",
            FontSize = 12,
            FontWeight = FontWeights.Medium,
        };
        Grid.SetColumn(inputLabel, 0);
        inputHeader.Children.Add(inputLabel);

        var countText = new TextBlock
        {
            Text = $"{chars.Length}/{expected}" + (valid ? " \u2713" : ""),
            FontSize = 11,
            FontWeight = FontWeights.Medium,
            Foreground = valid ? Brushes.Green : (chars.Length > expected ? Brushes.Red : Brushes.Gray),
        };
        Grid.SetColumn(countText, 1);
        inputHeader.Children.Add(countText);
        panel.Children.Add(inputHeader);

        var input = new TextBox
        {
            Text = _vm.GetRowInput(rowIndex),
            FontSize = 16,
            FontFamily = new FontFamily("Consolas"),
            Padding = new Thickness(6, 4, 6, 4),
            Margin = new Thickness(0, 0, 0, 16),
        };
        _activeInput = input;
        input.TextChanged += (_, _) =>
        {
            _vm.SetRowInput(rowIndex, input.Text);
            NextButton.IsEnabled = _vm.CanAdvance;
            // Refresh count indicator
            var newChars = _vm.ParsedCharacters(rowIndex);
            bool nowValid = newChars.Length == expected;
            countText.Text = $"{newChars.Length}/{expected}" + (nowValid ? " \u2713" : "");
            countText.Foreground = nowValid ? Brushes.Green : (newChars.Length > expected ? Brushes.Red : Brushes.Gray);

            // Refresh key reference highlights
            int idx = 0;
            foreach (var child in keyRefPanel.Children)
            {
                if (child is Border b)
                {
                    bool f = idx < newChars.Length;
                    b.Background = f
                        ? new SolidColorBrush(Color.FromArgb(38, 108, 92, 231))
                        : new SolidColorBrush(Color.FromRgb(230, 230, 230));
                    b.BorderBrush = f
                        ? new SolidColorBrush(Color.FromRgb(108, 92, 231))
                        : new SolidColorBrush(Color.FromRgb(200, 200, 200));
                    idx++;
                }
            }

            // Refresh mapping preview
            RenderMappingPreview(previewPanel, newChars, expected, sourceChars);
        };
        panel.Children.Add(input);

        // Mapping preview
        var previewPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };
        panel.Children.Add(previewPanel);
        RenderMappingPreview(previewPanel, chars, expected, sourceChars);

        scroll.Content = panel;
        ContentArea.Children.Add(scroll);
        input.Focus();
    }

    private static void RenderMappingPreview(StackPanel container, string[] chars, int expected, string[] sourceChars)
    {
        container.Children.Clear();
        if (chars.Length == 0) return;

        container.Children.Add(new TextBlock
        {
            Text = "Mapping Preview",
            FontSize = 11,
            FontWeight = FontWeights.Medium,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 4),
        });

        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        var previewRow = new StackPanel { Orientation = Orientation.Horizontal };

        int count = Math.Min(chars.Length, expected);
        for (int i = 0; i < count; i++)
        {
            var cell = new Border
            {
                Width = 32,
                Height = 52,
                Margin = new Thickness(2),
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                CornerRadius = new CornerRadius(4),
            };
            var cellContent = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            cellContent.Children.Add(new TextBlock
            {
                Text = sourceChars[i],
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            cellContent.Children.Add(new TextBlock
            {
                Text = "\u2193", // down arrow
                FontSize = 8,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            cellContent.Children.Add(new TextBlock
            {
                Text = chars[i],
                FontSize = 14,
                FontFamily = new FontFamily("Consolas"),
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            cell.Child = cellContent;
            previewRow.Children.Add(cell);
        }

        scrollViewer.Content = previewRow;
        container.Children.Add(scrollViewer);
    }

    private void RenderDoneStep()
    {
        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        // Checkmark icon
        panel.Children.Add(new TextBlock
        {
            Text = "\u2705", // checkmark
            FontSize = 56,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16),
        });

        panel.Children.Add(new TextBlock
        {
            Text = "Setup Complete!",
            FontSize = 22,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12),
        });

        panel.Children.Add(new TextBlock
        {
            Text = $"Your {_vm.LanguageName} keyboard layout is ready.\n{_vm.TotalMappedKeys} keys have been mapped.",
            FontSize = 13,
            Foreground = Brushes.Gray,
            TextAlignment = TextAlignment.Center,
            LineHeight = 20,
            Margin = new Thickness(0, 0, 0, 24),
        });

        // Summary
        var summaryBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Margin = new Thickness(40, 0, 40, 0),
            MaxWidth = 500,
        };
        var summaryPanel = new StackPanel();

        for (int i = 0; i < 4; i++)
        {
            var chars = _vm.ParsedCharacters(i);
            var rowGrid = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var rowTitle = new TextBlock
            {
                Text = SetupWizardViewModel.RowTitles[i],
                FontSize = 12,
                FontWeight = FontWeights.Medium,
            };
            Grid.SetColumn(rowTitle, 0);
            rowGrid.Children.Add(rowTitle);

            var rowChars = new TextBlock
            {
                Text = string.Join("", chars),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                Foreground = Brushes.Gray,
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            Grid.SetColumn(rowChars, 1);
            rowGrid.Children.Add(rowChars);

            var check = new TextBlock
            {
                Text = "\u2713",
                FontSize = 12,
                Foreground = Brushes.Green,
                Margin = new Thickness(8, 0, 0, 0),
            };
            Grid.SetColumn(check, 2);
            rowGrid.Children.Add(check);

            summaryPanel.Children.Add(rowGrid);
        }

        summaryBorder.Child = summaryPanel;
        panel.Children.Add(summaryBorder);

        ContentArea.Children.Add(panel);
    }

    private void OnNext(object sender, RoutedEventArgs e)
    {
        if (_vm.CurrentStep >= 5)
        {
            DialogResult = true;
            Close();
            return;
        }
        _vm.GoNext();
    }

    private void OnBack(object sender, RoutedEventArgs e)
    {
        _vm.GoBack();
    }
}
