using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OrchidKeyboard.Models;

public class MappingStore : INotifyPropertyChanged
{
    public static readonly MappingStore Shared = new();

    private MappingProfile _activeProfile = new();
    public MappingProfile ActiveProfile
    {
        get => _activeProfile;
        set
        {
            _activeProfile = value;
            RebuildCaches();
            Save();
            OnPropertyChanged();
        }
    }

    public Dictionary<string, string> ForwardLookup { get; private set; } = new();
    public Dictionary<string, string> ReverseLookup { get; private set; } = new();

    private readonly string _filePath;

    public bool IsFirstLaunch => !File.Exists(_filePath);

    // The QWERTY key IDs for each row, matching KeyboardLayoutData mappable keys
    public static readonly string[][] RowKeyIDs =
    {
        new[] { "grave", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "minus", "equal" },
        new[] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "lbracket", "rbracket", "backslash" },
        new[] { "a", "s", "d", "f", "g", "h", "j", "k", "l", "semicolon", "quote" },
        new[] { "z", "x", "c", "v", "b", "n", "m", "comma", "period", "slash" },
    };

    // The QWERTY source characters (lowercase) for each row
    public static readonly string[][] RowSourceChars =
    {
        new[] { "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=" },
        new[] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "[", "]", "\\" },
        new[] { "a", "s", "d", "f", "g", "h", "j", "k", "l", ";", "'" },
        new[] { "z", "x", "c", "v", "b", "n", "m", ",", ".", "/" },
    };

    // The QWERTY source characters (uppercase) for each row
    public static readonly string[][] RowSourceUpperChars =
    {
        new[] { "~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+" },
        new[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "{", "}", "|" },
        new[] { "A", "S", "D", "F", "G", "H", "J", "K", "L", ":", "\"" },
        new[] { "Z", "X", "C", "V", "B", "N", "M", "<", ">", "?" },
    };

    public static readonly int[] ExpectedCounts = { 13, 13, 11, 10 };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private MappingStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDir = Path.Combine(appData, "OrchidKeyboard");
        Directory.CreateDirectory(appDir);
        _filePath = Path.Combine(appDir, "profile.json");

        Load();
        RebuildCaches();
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var profile = JsonSerializer.Deserialize<MappingProfile>(json, JsonOptions);
                if (profile != null)
                {
                    _activeProfile = profile;
                    return;
                }
            }
        }
        catch { /* ignore corrupt file */ }

        _activeProfile = new MappingProfile { Name = "Custom" };
    }

    private void RebuildCaches()
    {
        ForwardLookup = _activeProfile.ForwardLookup();
        ReverseLookup = _activeProfile.ReverseLookup();
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_activeProfile, JsonOptions);
            File.WriteAllText(_filePath, json);
        }
        catch { /* ignore write errors */ }
    }

    public void UpdateMapping(string keyId, CharacterPair lowercase, CharacterPair uppercase)
    {
        var existing = _activeProfile.Mappings.FindIndex(m => m.Id == keyId);
        if (existing >= 0)
        {
            _activeProfile.Mappings[existing].Lowercase = lowercase;
            _activeProfile.Mappings[existing].Uppercase = uppercase;
        }
        else
        {
            _activeProfile.Mappings.Add(new KeyMapping(keyId, lowercase, uppercase));
        }
        RebuildCaches();
        Save();
        OnPropertyChanged(nameof(ActiveProfile));
    }

    public void ClearMapping(string keyId)
    {
        _activeProfile.Mappings.RemoveAll(m => m.Id == keyId);
        RebuildCaches();
        Save();
        OnPropertyChanged(nameof(ActiveProfile));
    }

    public void ClearAllMappings()
    {
        ActiveProfile = new MappingProfile { Name = "Custom" };
    }

    public KeyMapping? GetMapping(string keyId)
    {
        return _activeProfile.Mappings.FirstOrDefault(m => m.Id == keyId);
    }

    /// <summary>
    /// Build mappings from Setup Wizard row data.
    /// rows contains 4 arrays of target characters (one per keyboard row).
    /// </summary>
    public void ApplySetupMappings(string languageName, string[][] rows)
    {
        var mappings = new List<KeyMapping>();

        for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
        {
            var targetChars = rows[rowIndex];
            var keyIDs = RowKeyIDs[rowIndex];
            var sourceChars = RowSourceChars[rowIndex];
            var sourceUpperChars = RowSourceUpperChars[rowIndex];

            for (int i = 0; i < targetChars.Length && i < keyIDs.Length; i++)
            {
                var mapping = new KeyMapping(
                    keyIDs[i],
                    new CharacterPair(sourceChars[i], targetChars[i]),
                    new CharacterPair(sourceUpperChars[i], targetChars[i])
                );
                mappings.Add(mapping);
            }
        }

        ActiveProfile = new MappingProfile(Guid.NewGuid(), languageName, mappings);
    }

    public string ConvertText(string text)
    {
        // Count forward vs reverse matches to auto-detect direction
        int forwardHits = 0;
        int reverseHits = 0;
        foreach (char c in text)
        {
            var s = c.ToString();
            if (ForwardLookup.ContainsKey(s)) forwardHits++;
            if (ReverseLookup.ContainsKey(s)) reverseHits++;
        }

        var lookup = forwardHits >= reverseHits ? ForwardLookup : ReverseLookup;

        var result = new System.Text.StringBuilder(text.Length);
        foreach (char c in text)
        {
            var s = c.ToString();
            result.Append(lookup.TryGetValue(s, out var replacement) ? replacement : s);
        }
        return result.ToString();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
