import Foundation

struct CharacterPair: Codable, Equatable {
    var source: String
    var target: String
}

struct KeyMapping: Codable, Equatable, Identifiable {
    var id: String // matches PhysicalKey.id (e.g. "q", "1", "tab")
    var lowercase: CharacterPair
    var uppercase: CharacterPair
}

struct MappingProfile: Codable, Identifiable {
    var id: UUID
    var name: String
    var mappings: [KeyMapping]

    func forwardLookup() -> [String: String] {
        var dict = [String: String]()
        for m in mappings {
            if !m.lowercase.source.isEmpty && !m.lowercase.target.isEmpty {
                dict[m.lowercase.source] = m.lowercase.target
            }
            if !m.uppercase.source.isEmpty && !m.uppercase.target.isEmpty {
                dict[m.uppercase.source] = m.uppercase.target
            }
        }
        return dict
    }

    func reverseLookup() -> [String: String] {
        var dict = [String: String]()
        for m in mappings {
            if !m.lowercase.source.isEmpty && !m.lowercase.target.isEmpty {
                dict[m.lowercase.target] = m.lowercase.source
            }
            if !m.uppercase.source.isEmpty && !m.uppercase.target.isEmpty {
                dict[m.uppercase.target] = m.uppercase.source
            }
        }
        return dict
    }
}
