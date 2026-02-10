# Orchid Keyboard

A lightweight macOS menu bar app that remaps your keyboard to any language. Type in one layout, output in another — powered by a global hotkey.

## How It Works

1. Orchid runs in your menu bar
2. Type text using your QWERTY keyboard
3. Press **Cmd+Shift+T** to convert the selected text to your target language's characters
4. The text is replaced in-place using the mappings you configured

## Features

- **Language-agnostic** — works with any language or custom layout
- **Setup Wizard** — guided row-by-row keyboard mapping on first launch
- **Visual keyboard editor** — click any key to customize its mapping
- **Auto-detection** — automatically detects conversion direction (forward or reverse)
- **Menu bar app** — runs quietly in the background, no dock icon

## Requirements

- macOS 13+
- Swift toolchain (Xcode or Xcode Command Line Tools)
- Accessibility permission (for hotkey detection and text replacement)

## Build

```bash
git clone https://github.com/user/orchid-keyboard.git
cd orchid-keyboard
bash build.sh
```

The compiled app will be at `build/OrchidKeyboard.app`.

## Run

```bash
open build/OrchidKeyboard.app
```

On first launch, you'll need to:

1. Grant **Accessibility** permission in System Settings → Privacy & Security → Accessibility
2. Complete the **Setup Wizard** to map your keyboard layout

## Usage

| Action | How |
|--------|-----|
| Convert text | Select text, press **Cmd+Shift+T** |
| Open settings | Click menu bar icon → Settings |
| Edit a key | Click any key in the visual keyboard |
| Re-run setup | Click "Setup Wizard" in the toolbar |
| Toggle on/off | Click menu bar icon → Enabled |

## Project Structure

```
OrchidKeyboard/
├── main.swift                  # App entry point
├── AppDelegate.swift           # Menu bar setup, window management
├── HotkeyManager.swift         # Global Cmd+Shift+T hotkey
├── TextReplacer.swift          # Selected text replacement via accessibility APIs
├── Info.plist                  # App bundle configuration
├── Models/
│   ├── KeyMapping.swift        # KeyMapping, CharacterPair, MappingProfile
│   ├── KeyboardLayout.swift    # Physical key layout data (QWERTY)
│   └── MappingStore.swift      # Profile persistence and conversion logic
└── Views/
    ├── SettingsView.swift      # Main settings window
    ├── KeyboardView.swift      # Visual keyboard layout
    ├── KeyView.swift           # Individual key rendering
    ├── KeyEditPopover.swift    # Key mapping editor popover
    └── SetupWizardView.swift   # First-launch setup wizard
```

## License

[MIT](LICENSE)
