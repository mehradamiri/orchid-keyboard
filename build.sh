#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SRC_DIR="$SCRIPT_DIR/OrchidKeyboard"
BUILD_DIR="$SCRIPT_DIR/build"
APP_DIR="$BUILD_DIR/OrchidKeyboard.app"
CONTENTS_DIR="$APP_DIR/Contents"
MACOS_DIR="$CONTENTS_DIR/MacOS"

echo "Building Orchid Keyboard..."

# Clean previous build
rm -rf "$APP_DIR"

# Create .app bundle structure
mkdir -p "$MACOS_DIR"

# Compile
swiftc \
    -o "$MACOS_DIR/OrchidKeyboard" \
    "$SRC_DIR/main.swift" \
    "$SRC_DIR/AppDelegate.swift" \
    "$SRC_DIR/HotkeyManager.swift" \
    "$SRC_DIR/TextReplacer.swift" \
    "$SRC_DIR/Models/KeyMapping.swift" \
    "$SRC_DIR/Models/KeyboardLayout.swift" \
    "$SRC_DIR/Models/MappingStore.swift" \
    "$SRC_DIR/Views/SetupWizardView.swift" \
    "$SRC_DIR/Views/SettingsView.swift" \
    "$SRC_DIR/Views/KeyboardView.swift" \
    "$SRC_DIR/Views/KeyView.swift" \
    "$SRC_DIR/Views/KeyEditPopover.swift" \
    -framework Cocoa \
    -framework Carbon \
    -framework SwiftUI \
    -framework ServiceManagement

# Copy Info.plist
cp "$SRC_DIR/Info.plist" "$CONTENTS_DIR/Info.plist"

echo "Build complete: $APP_DIR"
echo ""
echo "To run:"
echo "  open $APP_DIR"
echo ""
echo "Note: You will need to grant Accessibility permission in"
echo "  System Settings → Privacy & Security → Accessibility"
