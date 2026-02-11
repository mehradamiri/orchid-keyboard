namespace OrchidKeyboard.Models;

public static class KeyboardLayoutData
{
    // Row 0: Number row (` 1 2 3 4 5 6 7 8 9 0 - = Delete)
    public static readonly PhysicalKey[] Row0 =
    {
        new("grave",  "`",  "`",  "~"),
        new("1",      "1",  "1",  "!"),
        new("2",      "2",  "2",  "@"),
        new("3",      "3",  "3",  "#"),
        new("4",      "4",  "4",  "$"),
        new("5",      "5",  "5",  "%"),
        new("6",      "6",  "6",  "^"),
        new("7",      "7",  "7",  "&"),
        new("8",      "8",  "8",  "*"),
        new("9",      "9",  "9",  "("),
        new("0",      "0",  "0",  ")"),
        new("minus",  "-",  "-",  "_"),
        new("equal",  "=",  "=",  "+"),
        new("delete", "Delete", width: 1.5, isModifier: true),
    };

    // Row 1: Tab Q W E R T Y U I O P [ ] backslash
    public static readonly PhysicalKey[] Row1 =
    {
        new("tab",       "Tab", width: 1.5, isModifier: true),
        new("q",         "Q",  "q",  "Q"),
        new("w",         "W",  "w",  "W"),
        new("e",         "E",  "e",  "E"),
        new("r",         "R",  "r",  "R"),
        new("t",         "T",  "t",  "T"),
        new("y",         "Y",  "y",  "Y"),
        new("u",         "U",  "u",  "U"),
        new("i",         "I",  "i",  "I"),
        new("o",         "O",  "o",  "O"),
        new("p",         "P",  "p",  "P"),
        new("lbracket",  "[",  "[",  "{"),
        new("rbracket",  "]",  "]",  "}"),
        new("backslash", "\\", "\\", "|"),
    };

    // Row 2: Caps A S D F G H J K L ; ' Return
    public static readonly PhysicalKey[] Row2 =
    {
        new("caps",      "Caps", width: 1.75, isModifier: true),
        new("a",         "A",  "a",  "A"),
        new("s",         "S",  "s",  "S"),
        new("d",         "D",  "d",  "D"),
        new("f",         "F",  "f",  "F"),
        new("g",         "G",  "g",  "G"),
        new("h",         "H",  "h",  "H"),
        new("j",         "J",  "j",  "J"),
        new("k",         "K",  "k",  "K"),
        new("l",         "L",  "l",  "L"),
        new("semicolon", ";",  ";",  ":"),
        new("quote",     "'",  "'",  "\""),
        new("return",    "Return", width: 1.75, isModifier: true),
    };

    // Row 3: Shift Z X C V B N M , . / Shift
    public static readonly PhysicalKey[] Row3 =
    {
        new("lshift",  "Shift", width: 2.25, isModifier: true),
        new("z",       "Z",  "z",  "Z"),
        new("x",       "X",  "x",  "X"),
        new("c",       "C",  "c",  "C"),
        new("v",       "V",  "v",  "V"),
        new("b",       "B",  "b",  "B"),
        new("n",       "N",  "n",  "N"),
        new("m",       "M",  "m",  "M"),
        new("comma",   ",",  ",",  "<"),
        new("period",  ".",  ".",  ">"),
        new("slash",   "/",  "/",  "?"),
        new("rshift",  "Shift", width: 2.25, isModifier: true),
    };

    public static readonly PhysicalKey[][] AllRows = { Row0, Row1, Row2, Row3 };

    public static readonly PhysicalKey[] MappableKeys = AllRows
        .SelectMany(r => r)
        .Where(k => !k.IsModifier)
        .ToArray();
}
