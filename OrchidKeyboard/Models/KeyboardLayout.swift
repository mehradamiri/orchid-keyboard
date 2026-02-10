import Foundation

struct PhysicalKey: Identifiable {
    let id: String
    let label: String
    let lowercaseChar: String
    let uppercaseChar: String
    let relativeWidth: CGFloat // 1.0 = standard key
    let isModifier: Bool

    init(id: String, label: String, lowercase: String = "", uppercase: String = "", width: CGFloat = 1.0, isModifier: Bool = false) {
        self.id = id
        self.label = label
        self.lowercaseChar = lowercase.isEmpty ? label.lowercased() : lowercase
        self.uppercaseChar = uppercase.isEmpty ? label.uppercased() : uppercase
        self.relativeWidth = width
        self.isModifier = isModifier
    }
}

struct KeyboardLayoutData {
    // Row 0: Number row (` 1 2 3 4 5 6 7 8 9 0 - = Delete)
    static let row0: [PhysicalKey] = [
        PhysicalKey(id: "grave",  label: "`",  lowercase: "`",  uppercase: "~"),
        PhysicalKey(id: "1",      label: "1",  lowercase: "1",  uppercase: "!"),
        PhysicalKey(id: "2",      label: "2",  lowercase: "2",  uppercase: "@"),
        PhysicalKey(id: "3",      label: "3",  lowercase: "3",  uppercase: "#"),
        PhysicalKey(id: "4",      label: "4",  lowercase: "4",  uppercase: "$"),
        PhysicalKey(id: "5",      label: "5",  lowercase: "5",  uppercase: "%"),
        PhysicalKey(id: "6",      label: "6",  lowercase: "6",  uppercase: "^"),
        PhysicalKey(id: "7",      label: "7",  lowercase: "7",  uppercase: "&"),
        PhysicalKey(id: "8",      label: "8",  lowercase: "8",  uppercase: "*"),
        PhysicalKey(id: "9",      label: "9",  lowercase: "9",  uppercase: "("),
        PhysicalKey(id: "0",      label: "0",  lowercase: "0",  uppercase: ")"),
        PhysicalKey(id: "minus",  label: "-",  lowercase: "-",  uppercase: "_"),
        PhysicalKey(id: "equal",  label: "=",  lowercase: "=",  uppercase: "+"),
        PhysicalKey(id: "delete", label: "Delete", width: 1.5, isModifier: true),
    ]

    // Row 1: Tab Q W E R T Y U I O P [ ] backslash
    static let row1: [PhysicalKey] = [
        PhysicalKey(id: "tab",       label: "Tab", width: 1.5, isModifier: true),
        PhysicalKey(id: "q",         label: "Q",  lowercase: "q",  uppercase: "Q"),
        PhysicalKey(id: "w",         label: "W",  lowercase: "w",  uppercase: "W"),
        PhysicalKey(id: "e",         label: "E",  lowercase: "e",  uppercase: "E"),
        PhysicalKey(id: "r",         label: "R",  lowercase: "r",  uppercase: "R"),
        PhysicalKey(id: "t",         label: "T",  lowercase: "t",  uppercase: "T"),
        PhysicalKey(id: "y",         label: "Y",  lowercase: "y",  uppercase: "Y"),
        PhysicalKey(id: "u",         label: "U",  lowercase: "u",  uppercase: "U"),
        PhysicalKey(id: "i",         label: "I",  lowercase: "i",  uppercase: "I"),
        PhysicalKey(id: "o",         label: "O",  lowercase: "o",  uppercase: "O"),
        PhysicalKey(id: "p",         label: "P",  lowercase: "p",  uppercase: "P"),
        PhysicalKey(id: "lbracket",  label: "[",  lowercase: "[",  uppercase: "{"),
        PhysicalKey(id: "rbracket",  label: "]",  lowercase: "]",  uppercase: "}"),
        PhysicalKey(id: "backslash", label: "\\", lowercase: "\\", uppercase: "|"),
    ]

    // Row 2: Caps A S D F G H J K L ; ' Return
    static let row2: [PhysicalKey] = [
        PhysicalKey(id: "caps",      label: "Caps", width: 1.75, isModifier: true),
        PhysicalKey(id: "a",         label: "A",  lowercase: "a",  uppercase: "A"),
        PhysicalKey(id: "s",         label: "S",  lowercase: "s",  uppercase: "S"),
        PhysicalKey(id: "d",         label: "D",  lowercase: "d",  uppercase: "D"),
        PhysicalKey(id: "f",         label: "F",  lowercase: "f",  uppercase: "F"),
        PhysicalKey(id: "g",         label: "G",  lowercase: "g",  uppercase: "G"),
        PhysicalKey(id: "h",         label: "H",  lowercase: "h",  uppercase: "H"),
        PhysicalKey(id: "j",         label: "J",  lowercase: "j",  uppercase: "J"),
        PhysicalKey(id: "k",         label: "K",  lowercase: "k",  uppercase: "K"),
        PhysicalKey(id: "l",         label: "L",  lowercase: "l",  uppercase: "L"),
        PhysicalKey(id: "semicolon", label: ";",  lowercase: ";",  uppercase: ":"),
        PhysicalKey(id: "quote",     label: "'",  lowercase: "'",  uppercase: "\""),
        PhysicalKey(id: "return",    label: "Return", width: 1.75, isModifier: true),
    ]

    // Row 3: Shift Z X C V B N M , . / Shift
    static let row3: [PhysicalKey] = [
        PhysicalKey(id: "lshift",  label: "Shift", width: 2.25, isModifier: true),
        PhysicalKey(id: "z",       label: "Z",  lowercase: "z",  uppercase: "Z"),
        PhysicalKey(id: "x",       label: "X",  lowercase: "x",  uppercase: "X"),
        PhysicalKey(id: "c",       label: "C",  lowercase: "c",  uppercase: "C"),
        PhysicalKey(id: "v",       label: "V",  lowercase: "v",  uppercase: "V"),
        PhysicalKey(id: "b",       label: "B",  lowercase: "b",  uppercase: "B"),
        PhysicalKey(id: "n",       label: "N",  lowercase: "n",  uppercase: "N"),
        PhysicalKey(id: "m",       label: "M",  lowercase: "m",  uppercase: "M"),
        PhysicalKey(id: "comma",   label: ",",  lowercase: ",",  uppercase: "<"),
        PhysicalKey(id: "period",  label: ".",  lowercase: ".",  uppercase: ">"),
        PhysicalKey(id: "slash",   label: "/",  lowercase: "/",  uppercase: "?"),
        PhysicalKey(id: "rshift",  label: "Shift", width: 2.25, isModifier: true),
    ]

    static let allRows: [[PhysicalKey]] = [row0, row1, row2, row3]

    static let mappableKeys: [PhysicalKey] = {
        allRows.flatMap { $0 }.filter { !$0.isModifier }
    }()
}
