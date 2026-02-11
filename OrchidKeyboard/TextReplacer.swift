import Cocoa
import Carbon

class TextReplacer {
    /// Called from a background queue so sleeps don't block the main run loop.
    static func replaceSelectedText() {
        let pasteboard = NSPasteboard.general

        // 1. Save current clipboard state
        let savedContents = pasteboard.pasteboardItems?.compactMap { item -> (String, Data)? in
            guard let type = item.types.first, let data = item.data(forType: type) else { return nil }
            return (type.rawValue, data)
        } ?? []

        // 2. Clear clipboard so we can detect when Cmd+C writes to it
        pasteboard.clearContents()
        pasteboard.setString("", forType: .string)
        let clearedChangeCount = pasteboard.changeCount

        // 3. Simulate Cmd+C to copy selected text
        simulateKeyPress(keyCode: UInt16(kVK_ANSI_C), flags: .maskCommand)

        // 4. Poll for clipboard change (up to 500ms)
        var copied = false
        for _ in 0..<50 {
            Thread.sleep(forTimeInterval: 0.01) // 10ms ticks on background thread
            if pasteboard.changeCount != clearedChangeCount {
                copied = true
                break
            }
        }

        // 5. Validate that we actually got new content
        guard copied, let selectedText = pasteboard.string(forType: .string), !selectedText.isEmpty else {
            // Copy failed — restore clipboard and bail
            restoreClipboard(pasteboard: pasteboard, contents: savedContents)
            return
        }

        // 6. Convert text using MappingStore (auto-detects direction)
        let replacement = MappingStore.shared.convertText(selectedText)

        // 7. Set clipboard to replacement text
        pasteboard.clearContents()
        pasteboard.setString(replacement, forType: .string)

        // 8. Simulate Cmd+V to paste
        simulateKeyPress(keyCode: UInt16(kVK_ANSI_V), flags: .maskCommand)

        // 9. Restore original clipboard after paste completes
        DispatchQueue.global(qos: .utility).asyncAfter(deadline: .now() + 0.3) {
            restoreClipboard(pasteboard: pasteboard, contents: savedContents)
        }
    }

    private static func restoreClipboard(pasteboard: NSPasteboard, contents: [(String, Data)]) {
        pasteboard.clearContents()
        for (typeRaw, data) in contents {
            let type = NSPasteboard.PasteboardType(rawValue: typeRaw)
            pasteboard.setData(data, forType: type)
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
