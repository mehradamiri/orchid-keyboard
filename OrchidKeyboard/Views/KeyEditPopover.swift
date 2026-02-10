import SwiftUI

struct KeyEditPopover: View {
    let physicalKey: PhysicalKey
    let currentMapping: KeyMapping?
    let onSave: (CharacterPair, CharacterPair) -> Void
    let onClear: () -> Void
    let onCancel: () -> Void

    @State private var lowercaseTarget: String
    @State private var uppercaseTarget: String

    init(physicalKey: PhysicalKey, currentMapping: KeyMapping?,
         onSave: @escaping (CharacterPair, CharacterPair) -> Void,
         onClear: @escaping () -> Void,
         onCancel: @escaping () -> Void) {
        self.physicalKey = physicalKey
        self.currentMapping = currentMapping
        self.onSave = onSave
        self.onClear = onClear
        self.onCancel = onCancel
        _lowercaseTarget = State(initialValue: currentMapping?.lowercase.target ?? "")
        _uppercaseTarget = State(initialValue: currentMapping?.uppercase.target ?? "")
    }

    var body: some View {
        VStack(alignment: .leading, spacing: 12) {
            Text("Edit Key: \(physicalKey.label)")
                .font(.headline)

            HStack(spacing: 16) {
                VStack(alignment: .leading, spacing: 4) {
                    Text("Lowercase (\(physicalKey.lowercaseChar))")
                        .font(.caption)
                        .foregroundColor(.secondary)
                    TextField("Target", text: $lowercaseTarget)
                        .textFieldStyle(.roundedBorder)
                        .frame(width: 100)
                }

                VStack(alignment: .leading, spacing: 4) {
                    Text("Uppercase (\(physicalKey.uppercaseChar))")
                        .font(.caption)
                        .foregroundColor(.secondary)
                    TextField("Target", text: $uppercaseTarget)
                        .textFieldStyle(.roundedBorder)
                        .frame(width: 100)
                }
            }

            HStack {
                Button("Clear") {
                    onClear()
                }
                Spacer()
                Button("Cancel") {
                    onCancel()
                }
                .keyboardShortcut(.cancelAction)
                Button("Save") {
                    let lc = CharacterPair(source: physicalKey.lowercaseChar, target: lowercaseTarget)
                    let uc = CharacterPair(source: physicalKey.uppercaseChar, target: uppercaseTarget)
                    onSave(lc, uc)
                }
                .keyboardShortcut(.defaultAction)
            }
        }
        .padding()
        .frame(width: 280)
    }
}
