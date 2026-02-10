import Foundation
import Combine

class MappingStore: ObservableObject {
    static let shared = MappingStore()

    @Published var activeProfile: MappingProfile {
        didSet {
            rebuildCaches()
            save()
        }
    }

    private(set) var forwardLookup: [String: String] = [:]
    private(set) var reverseLookup: [String: String] = [:]

    private let fileURL: URL

    var isFirstLaunch: Bool {
        !FileManager.default.fileExists(atPath: fileURL.path)
    }

    // The QWERTY key IDs for each row, matching KeyboardLayoutData mappable keys
    static let rowKeyIDs: [[String]] = [
        ["grave", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "minus", "equal"],                     // 13
        ["q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "lbracket", "rbracket", "backslash"],           // 13
        ["a", "s", "d", "f", "g", "h", "j", "k", "l", "semicolon", "quote"],                                // 11
        ["z", "x", "c", "v", "b", "n", "m", "comma", "period", "slash"],                                    // 10
    ]

    // The QWERTY source characters (lowercase) for each row
    static let rowSourceChars: [[String]] = [
        ["`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "="],
        ["q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "[", "]", "\\"],
        ["a", "s", "d", "f", "g", "h", "j", "k", "l", ";", "'"],
        ["z", "x", "c", "v", "b", "n", "m", ",", ".", "/"],
    ]

    // The QWERTY source characters (uppercase) for each row
    static let rowSourceUpperChars: [[String]] = [
        ["~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+"],
        ["Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "{", "}", "|"],
        ["A", "S", "D", "F", "G", "H", "J", "K", "L", ":", "\""],
        ["Z", "X", "C", "V", "B", "N", "M", "<", ">", "?"],
    ]

    static let expectedCounts: [Int] = [13, 13, 11, 10]

    private init() {
        let appSupport = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask).first!
        let appDir = appSupport.appendingPathComponent("OrchidKeyboard")
        try? FileManager.default.createDirectory(at: appDir, withIntermediateDirectories: true)
        fileURL = appDir.appendingPathComponent("profile.json")

        if let data = try? Data(contentsOf: fileURL),
           let profile = try? JSONDecoder().decode(MappingProfile.self, from: data) {
            activeProfile = profile
        } else {
            activeProfile = MappingProfile(id: UUID(), name: "Custom", mappings: [])
        }
        rebuildCaches()
    }

    private func rebuildCaches() {
        forwardLookup = activeProfile.forwardLookup()
        reverseLookup = activeProfile.reverseLookup()
    }

    private func save() {
        if let data = try? JSONEncoder().encode(activeProfile) {
            try? data.write(to: fileURL, options: .atomic)
        }
    }

    func updateMapping(keyID: String, lowercase: CharacterPair, uppercase: CharacterPair) {
        if let idx = activeProfile.mappings.firstIndex(where: { $0.id == keyID }) {
            activeProfile.mappings[idx].lowercase = lowercase
            activeProfile.mappings[idx].uppercase = uppercase
        } else {
            activeProfile.mappings.append(KeyMapping(id: keyID, lowercase: lowercase, uppercase: uppercase))
        }
    }

    func clearMapping(keyID: String) {
        activeProfile.mappings.removeAll { $0.id == keyID }
    }

    func clearAllMappings() {
        activeProfile = MappingProfile(id: UUID(), name: "Custom", mappings: [])
    }

    func mapping(for keyID: String) -> KeyMapping? {
        activeProfile.mappings.first { $0.id == keyID }
    }

    /// Build mappings from Setup Wizard row data.
    /// `rows` contains 4 arrays of target characters (one per keyboard row).
    func applySetupMappings(languageName: String, rows: [[String]]) {
        var mappings: [KeyMapping] = []

        for (rowIndex, targetChars) in rows.enumerated() {
            let keyIDs = MappingStore.rowKeyIDs[rowIndex]
            let sourceChars = MappingStore.rowSourceChars[rowIndex]
            let sourceUpperChars = MappingStore.rowSourceUpperChars[rowIndex]

            for (i, targetChar) in targetChars.enumerated() where i < keyIDs.count {
                let mapping = KeyMapping(
                    id: keyIDs[i],
                    lowercase: CharacterPair(source: sourceChars[i], target: targetChar),
                    uppercase: CharacterPair(source: sourceUpperChars[i], target: targetChar)
                )
                mappings.append(mapping)
            }
        }

        activeProfile = MappingProfile(id: UUID(), name: languageName, mappings: mappings)
    }

    func convertText(_ text: String) -> String {
        // Count forward vs reverse matches to auto-detect direction
        var forwardHits = 0
        var reverseHits = 0
        for char in text {
            let s = String(char)
            if forwardLookup[s] != nil { forwardHits += 1 }
            if reverseLookup[s] != nil { reverseHits += 1 }
        }

        let lookup = forwardHits >= reverseHits ? forwardLookup : reverseLookup

        var result = ""
        for char in text {
            let s = String(char)
            result += lookup[s] ?? s
        }
        return result
    }
}
