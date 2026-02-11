namespace OrchidKeyboard.Models;

public class PhysicalKey
{
    public string Id { get; }
    public string Label { get; }
    public string LowercaseChar { get; }
    public string UppercaseChar { get; }
    public double RelativeWidth { get; }
    public bool IsModifier { get; }

    public PhysicalKey(string id, string label, string lowercase = "", string uppercase = "", double width = 1.0, bool isModifier = false)
    {
        Id = id;
        Label = label;
        LowercaseChar = string.IsNullOrEmpty(lowercase) ? label.ToLowerInvariant() : lowercase;
        UppercaseChar = string.IsNullOrEmpty(uppercase) ? label.ToUpperInvariant() : uppercase;
        RelativeWidth = width;
        IsModifier = isModifier;
    }
}
