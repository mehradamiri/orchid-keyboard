using System.ComponentModel;
using System.Runtime.CompilerServices;
using OrchidKeyboard.Models;

namespace OrchidKeyboard.ViewModels;

public class SetupWizardViewModel : INotifyPropertyChanged
{
    private readonly MappingStore _store = MappingStore.Shared;

    public const int TotalSteps = 6; // 0=Welcome, 1-4=Rows, 5=Done

    public static readonly string[] RowTitles =
    {
        "Number Row",
        "Top Letter Row",
        "Home Row",
        "Bottom Row",
    };

    public static readonly string[] RowDescriptions =
    {
        "Type all keys from the number row in order:",
        "Type all keys from the top letter row in order:",
        "Type all keys from the home row in order:",
        "Type all keys from the bottom row in order:",
    };

    private int _currentStep;
    public int CurrentStep
    {
        get => _currentStep;
        set { _currentStep = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanAdvance)); OnPropertyChanged(nameof(ProgressFraction)); }
    }

    private string _languageName = "";
    public string LanguageName
    {
        get => _languageName;
        set { _languageName = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanAdvance)); }
    }

    private readonly string[] _rowInputs = { "", "", "", "" };

    public string GetRowInput(int index) => _rowInputs[index];

    public void SetRowInput(int index, string value)
    {
        _rowInputs[index] = value;
        OnPropertyChanged(nameof(CanAdvance));
    }

    public double ProgressFraction => (double)CurrentStep / (TotalSteps - 1);

    public string[] ParsedCharacters(int rowIndex)
    {
        return _rowInputs[rowIndex]
            .Select(c => c.ToString())
            .ToArray();
    }

    public bool IsRowValid(int rowIndex)
    {
        return ParsedCharacters(rowIndex).Length == MappingStore.ExpectedCounts[rowIndex];
    }

    public bool CanAdvance => CurrentStep switch
    {
        0 => !string.IsNullOrWhiteSpace(LanguageName),
        1 => IsRowValid(0),
        2 => IsRowValid(1),
        3 => IsRowValid(2),
        4 => IsRowValid(3),
        5 => true,
        _ => false,
    };

    public int TotalMappedKeys => Enumerable.Range(0, 4).Sum(i => ParsedCharacters(i).Length);

    public void GoNext()
    {
        if (!CanAdvance) return;

        if (CurrentStep == 4)
            ApplyMappings();

        CurrentStep++;
    }

    public void GoBack()
    {
        if (CurrentStep > 0)
            CurrentStep--;
    }

    private void ApplyMappings()
    {
        var rows = Enumerable.Range(0, 4)
            .Select(i => ParsedCharacters(i))
            .ToArray();
        _store.ApplySetupMappings(LanguageName.Trim(), rows);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
