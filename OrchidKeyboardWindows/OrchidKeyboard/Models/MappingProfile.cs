namespace OrchidKeyboard.Models;

public class MappingProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Custom";
    public List<KeyMapping> Mappings { get; set; } = new();

    public MappingProfile() { }

    public MappingProfile(Guid id, string name, List<KeyMapping> mappings)
    {
        Id = id;
        Name = name;
        Mappings = mappings;
    }

    public Dictionary<string, string> ForwardLookup()
    {
        var dict = new Dictionary<string, string>();
        foreach (var m in Mappings)
        {
            if (!string.IsNullOrEmpty(m.Lowercase.Source) && !string.IsNullOrEmpty(m.Lowercase.Target))
                dict[m.Lowercase.Source] = m.Lowercase.Target;
            if (!string.IsNullOrEmpty(m.Uppercase.Source) && !string.IsNullOrEmpty(m.Uppercase.Target))
                dict[m.Uppercase.Source] = m.Uppercase.Target;
        }
        return dict;
    }

    public Dictionary<string, string> ReverseLookup()
    {
        var dict = new Dictionary<string, string>();
        foreach (var m in Mappings)
        {
            // Process uppercase first so lowercase takes priority when
            // both map to the same target (e.g. scripts without case distinction).
            if (!string.IsNullOrEmpty(m.Uppercase.Source) && !string.IsNullOrEmpty(m.Uppercase.Target))
                dict[m.Uppercase.Target] = m.Uppercase.Source;
            if (!string.IsNullOrEmpty(m.Lowercase.Source) && !string.IsNullOrEmpty(m.Lowercase.Target))
                dict[m.Lowercase.Target] = m.Lowercase.Source;
        }
        return dict;
    }
}
