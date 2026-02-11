namespace OrchidKeyboard.Models;

public class CharacterPair
{
    public string Source { get; set; } = "";
    public string Target { get; set; } = "";

    public CharacterPair() { }

    public CharacterPair(string source, string target)
    {
        Source = source;
        Target = target;
    }
}
