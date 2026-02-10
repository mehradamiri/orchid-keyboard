import Cocoa
import Carbon
import SwiftUI

class AppDelegate: NSObject, NSApplicationDelegate {
    private var statusItem: NSStatusItem?
    private var hotkeyManager: HotkeyManager!
    private var enabledMenuItem: NSMenuItem!
    private var isEnabled = true
    private var settingsWindow: NSWindow?

    private let hideIconKey = "hideMenuBarIcon"

    var isMenuBarIconHidden: Bool {
        get { UserDefaults.standard.bool(forKey: hideIconKey) }
        set {
            UserDefaults.standard.set(newValue, forKey: hideIconKey)
            if newValue {
                removeMenuBarIcon()
            } else {
                setupMenuBar()
            }
        }
    }

    func applicationDidFinishLaunching(_ notification: Notification) {
        requestAccessibilityPermission()

        if !UserDefaults.standard.bool(forKey: hideIconKey) {
            setupMenuBar()
        }

        hotkeyManager = HotkeyManager()
        hotkeyManager.register()

        if MappingStore.shared.isFirstLaunch {
            openSettings()
        }
    }

    func applicationShouldHandleReopen(_ sender: NSApplication, hasVisibleWindows flag: Bool) -> Bool {
        openSettings()
        return true
    }

    private func requestAccessibilityPermission() {
        let options = [kAXTrustedCheckOptionPrompt.takeUnretainedValue(): true] as CFDictionary
        let trusted = AXIsProcessTrustedWithOptions(options)
        if !trusted {
            NSLog("Orchid Keyboard: Accessibility permission not yet granted. Please enable it in System Settings.")
        }
    }

    private func setupMenuBar() {
        if statusItem != nil { return }

        let item = NSStatusBar.system.statusItem(withLength: NSStatusItem.squareLength)

        if let button = item.button {
            if let image = NSImage(systemSymbolName: "keyboard", accessibilityDescription: "Orchid Keyboard") {
                image.isTemplate = true
                button.image = image
            } else {
                button.title = "⌨"
            }
        }

        let menu = NSMenu()

        enabledMenuItem = NSMenuItem(title: "Enabled", action: #selector(toggleEnabled), keyEquivalent: "")
        enabledMenuItem.target = self
        enabledMenuItem.state = isEnabled ? .on : .off
        menu.addItem(enabledMenuItem)

        let settingsItem = NSMenuItem(title: "Settings...", action: #selector(openSettings), keyEquivalent: ",")
        settingsItem.target = self
        menu.addItem(settingsItem)

        menu.addItem(NSMenuItem.separator())

        let quitItem = NSMenuItem(title: "Quit", action: #selector(quitApp), keyEquivalent: "q")
        quitItem.target = self
        menu.addItem(quitItem)

        item.menu = menu
        statusItem = item
    }

    private func removeMenuBarIcon() {
        if let item = statusItem {
            NSStatusBar.system.removeStatusItem(item)
            statusItem = nil
        }
    }

    @objc private func toggleEnabled() {
        isEnabled.toggle()
        enabledMenuItem.state = isEnabled ? .on : .off

        if isEnabled {
            hotkeyManager.register()
        } else {
            hotkeyManager.unregister()
        }
    }

    @objc func openSettings() {
        if let window = settingsWindow {
            window.makeKeyAndOrderFront(nil)
            NSApp.activate(ignoringOtherApps: true)
            return
        }

        let settingsView = SettingsView()
        let hostingController = NSHostingController(rootView: settingsView)

        let window = NSWindow(contentViewController: hostingController)
        window.title = "Orchid Keyboard Settings"
        window.styleMask = [.titled, .closable]
        window.setContentSize(NSSize(width: 860, height: 560))
        window.center()
        window.isReleasedWhenClosed = false

        self.settingsWindow = window
        window.makeKeyAndOrderFront(nil)
        NSApp.activate(ignoringOtherApps: true)
    }

    @objc private func quitApp() {
        hotkeyManager.unregister()
        NSApplication.shared.terminate(nil)
    }
}
