using System.Windows;
using System.Windows.Controls;
using OrchidKeyboard.Models;

namespace OrchidKeyboard.Views;

public partial class KeyboardControl : UserControl
{
    private const double KeySpacing = 4;
    private const double QuarterKeyWidth = 12;
    private static readonly double[] RowStagger = { 0, 0.5, 0.75, 1.25 };

    private string? _selectedKeyId;
    private readonly List<KeyControl> _keyControls = new();

    public event Action<string>? KeyClicked;

    public KeyboardControl()
    {
        InitializeComponent();
    }

    public void Render(MappingStore store, string? selectedKeyId)
    {
        _selectedKeyId = selectedKeyId;
        RowsPanel.Children.Clear();
        _keyControls.Clear();

        for (int rowIndex = 0; rowIndex < KeyboardLayoutData.AllRows.Length; rowIndex++)
        {
            var row = KeyboardLayoutData.AllRows[rowIndex];
            var stagger = RowStagger[rowIndex];

            var rowPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, rowIndex > 0 ? KeySpacing : 0, 0, 0),
            };

            // Left stagger spacer
            if (stagger > 0)
            {
                rowPanel.Children.Add(new Border
                {
                    Width = stagger * QuarterKeyWidth,
                });
            }

            foreach (var key in row)
            {
                var keyControl = new KeyControl();
                var mapping = store.GetMapping(key.Id);
                keyControl.Setup(key, mapping, selectedKeyId == key.Id);
                keyControl.Margin = new Thickness(KeySpacing / 2, 0, KeySpacing / 2, 0);
                keyControl.KeyClicked += id => KeyClicked?.Invoke(id);
                rowPanel.Children.Add(keyControl);
                _keyControls.Add(keyControl);
            }

            // Right stagger spacer
            if (stagger > 0)
            {
                rowPanel.Children.Add(new Border
                {
                    Width = stagger * QuarterKeyWidth,
                });
            }

            RowsPanel.Children.Add(rowPanel);
        }
    }
}
