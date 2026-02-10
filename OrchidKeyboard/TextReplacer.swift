import Cocoa
import Carbon

class TextReplacer {
    static func replaceSelectedText() {
        let pasteboard = NSPasteboard.general

        // 1. Save current clipboard contents
        let savedContents = pasteboard.pasteboardItems?.compactMap { item -> (String, Data)? in
            guard let type = item.types.first, let data = item.data(forType: type) else { return nil }
            return (type.rawValue, data)
        } ?? []

        // 2. Simulate Cmd+C to copy selected text
        simulateKeyPress(keyCode: UInt16(kVK_ANSI_C), flags: .maskCommand)

        // 3. Wait for clipboard to update
        usleep(100_000) // 100ms

        // 4. Read the selected text
        let selectedText = pasteboard.string(forType: .string) ?? ""

        // 5. Convert text using MappingStore (auto-detects direction)
        let replacement = MappingStore.shared.convertText(selectedText)

        // 6. Set clipboard to replacement text
        pasteboard.clearContents()
        pasteboard.setString(replacement, forType: .string)

        // 7. Simulate Cmd+V to paste
        simulateKeyPress(keyCode: UInt16(kVK_ANSI_V), flags: .maskCommand)

        // 8. Restore original clipboard after a short delay
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.05) {
            pasteboard.clearContents()
            for (typeRaw, data) in savedContents {
                let type = NSPasteboard.PasteboardType(rawValue: typeRaw)
                pasteboard.setData(data, forType: type)
            }
        }
    }

    private static func simulateKeyPress(keyCode: UInt16, flags: CGEventFlags) {
        let source = CGEventSource(stateID: .hidSystemState)

        guard let keyDown = CGEvent(keyboardEventSource: source, virtualKey: keyCode, keyDown: true),
              let keyUp = CGEvent(keyboardEventSource: source, virtualKey: keyCode, keyDown: false) else {
            return
        }

        keyDown.flags = flags
        keyUp.flags = flags

        keyDown.post(tap: .cghidEventTap)
        keyUp.post(tap: .cghidEventTap)
    }
}
