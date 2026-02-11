import Carbon
import Foundation

// Global callback — Carbon hotkey API requires a C-style function pointer
private func hotkeyCallback(
    nextHandler: EventHandlerCallRef?,
    event: EventRef?,
    userData: UnsafeMutableRawPointer?
) -> OSStatus {
    // Dispatch off the main thread so the run loop stays free
    // to process the simulated key events (Cmd+C / Cmd+V)
    DispatchQueue.global(qos: .userInitiated).async {
        TextReplacer.replaceSelectedText()
    }
    return noErr
}

class HotkeyManager {
    private var hotkeyRef: EventHotKeyRef?
    private var eventHandlerRef: EventHandlerRef?

    func register() {
        guard hotkeyRef == nil else { return }

        // Register the event handler for hotkey events
        var eventType = EventTypeSpec(eventClass: OSType(kEventClassKeyboard), eventKind: UInt32(kEventHotKeyPressed))
        InstallEventHandler(
            GetApplicationEventTarget(),
            hotkeyCallback,
            1,
            &eventType,
            nil,
            &eventHandlerRef
        )

        // Register Cmd+Shift+T
        // Carbon key code for 'T' is 17
        let hotkeyID = EventHotKeyID(signature: OSType(0x4F524348), id: 1) // 'ORCH'
        let modifiers: UInt32 = UInt32(cmdKey | shiftKey)

        RegisterEventHotKey(
            UInt32(kVK_ANSI_T),
            modifiers,
            hotkeyID,
            GetApplicationEventTarget(),
            0,
            &hotkeyRef
        )
    }

    func unregister() {
        if let ref = hotkeyRef {
            UnregisterEventHotKey(ref)
            hotkeyRef = nil
        }
        if let handler = eventHandlerRef {
            RemoveEventHandler(handler)
            eventHandlerRef = nil
        }
    }
}
