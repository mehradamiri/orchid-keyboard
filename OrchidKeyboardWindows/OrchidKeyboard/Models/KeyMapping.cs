namespace OrchidKeyboard.Models;

public class KeyMapping
{
    public string Id { get; set; } = "";
    public CharacterPair Lowercase { get; set; } = new();
    public CharacterPair Uppercase { get; set; } = new();

    public KeyMapping() { }

    public KeyMapping(string id, CharacterPair lowercase, CharacterPair uppercase)
    {
        Id = id;
        Lowercase = lowercase;
        Uppercase = uppercase;
    }
}
