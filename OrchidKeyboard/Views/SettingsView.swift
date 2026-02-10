import SwiftUI

struct SettingsView: View {
    @ObservedObject var store = MappingStore.shared
    @State private var selectedKeyID: String?
    @State private var showingSetupWizard = false

    private var mappedKeyCount: Int {
        store.activeProfile.mappings.filter { m in
            !m.lowercase.target.isEmpty && m.lowercase.source != m.lowercase.target
        }.count
    }

    private var selectedPhysicalKey: PhysicalKey? {
        guard let id = selectedKeyID else { return nil }
        return KeyboardLayoutData.mappableKeys.first { $0.id == id }
    }

    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack {
                VStack(alignment: .leading, spacing: 2) {
                    Text("Orchid Keyboard")
                        .font(.title2.weight(.semibold))
                    Text("Configure your key mappings")
                        .font(.caption)
                        .foregroundColor(.secondary)
                }
                Spacer()
                Text(store.activeProfile.name)
                    .font(.subheadline)
                    .foregroundColor(.secondary)
                    .padding(.horizontal, 10)
                    .padding(.vertical, 4)
                    .background(Color(nsColor: .controlBackgroundColor))
                    .clipShape(RoundedRectangle(cornerRadius: 6, style: .continuous))
            }
            .padding(.horizontal, 24)
            .padding(.top, 20)
            .padding(.bottom, 16)

            Divider()

            // Keyboard
            KeyboardView(store: store, selectedKeyID: $selectedKeyID)
                .padding(.horizontal, 24)
                .padding(.vertical, 20)
                .popover(item: popoverBinding) { item in
                    KeyEditPopover(
                        physicalKey: item.key,
                        currentMapping: store.mapping(for: item.key.id),
                        onSave: { lc, uc in
                            store.updateMapping(keyID: item.key.id, lowercase: lc, uppercase: uc)
                            selectedKeyID = nil
                        },
                        onClear: {
                            store.clearMapping(keyID: item.key.id)
                            selectedKeyID = nil
                        },
                        onCancel: {
                            selectedKeyID = nil
                        }
                    )
                }

            Spacer(minLength: 0)

            Divider()

            // Toolbar
            HStack {
                Text("\(mappedKeyCount) keys mapped")
                    .font(.caption)
                    .foregroundColor(.secondary)
                Spacer()
                Button("Clear All Mappings") {
                    store.clearAllMappings()
                    selectedKeyID = nil
                }
                .controlSize(.small)
                Button("Setup Wizard") {
                    showingSetupWizard = true
                }
                .controlSize(.small)
            }
            .padding(.horizontal, 24)
            .padding(.vertical, 12)
        }
        .frame(width: 860, height: 560)
        .background(Color(nsColor: .windowBackgroundColor))
        .sheet(isPresented: $showingSetupWizard) {
            SetupWizardView(store: store) {
                showingSetupWizard = false
            }
        }
        .onAppear {
            if store.isFirstLaunch {
                showingSetupWizard = true
            }
        }
    }

    // Bridge PhysicalKey selection into a Binding<PhysicalKey?> for popover(item:)
    private var popoverBinding: Binding<IdentifiablePhysicalKey?> {
        Binding<IdentifiablePhysicalKey?>(
            get: {
                guard let key = selectedPhysicalKey else { return nil }
                return IdentifiablePhysicalKey(key: key)
            },
            set: { newValue in
                selectedKeyID = newValue?.key.id
            }
        )
    }
}

// Wrapper to make PhysicalKey work with popover(item:)
private struct IdentifiablePhysicalKey: Identifiable {
    let key: PhysicalKey
    var id: String { key.id }
}
