import SwiftUI

struct SetupWizardView: View {
    @ObservedObject var store: MappingStore
    var onFinish: () -> Void

    @State private var currentStep = 0
    @State private var languageName = ""
    @State private var rowInputs: [String] = ["", "", "", ""]
    @State private var animationDirection: Edge = .trailing

    private let totalSteps = 6 // 0=Welcome, 1-4=Rows, 5=Done

    private let rowTitles = [
        "Number Row",
        "Top Letter Row",
        "Home Row",
        "Bottom Row",
    ]

    private let rowDescriptions = [
        "Type all keys from the number row in order:",
        "Type all keys from the top letter row in order:",
        "Type all keys from the home row in order:",
        "Type all keys from the bottom row in order:",
    ]

    private let rowLabels: [[String]] = MappingStore.rowSourceChars

    private let expectedCounts = MappingStore.expectedCounts

    private func parsedCharacters(for rowIndex: Int) -> [String] {
        let input = rowInputs[rowIndex]
        return input.map { String($0) }
    }

    private func isRowValid(_ rowIndex: Int) -> Bool {
        parsedCharacters(for: rowIndex).count == expectedCounts[rowIndex]
    }

    private var canAdvance: Bool {
        switch currentStep {
        case 0: return !languageName.trimmingCharacters(in: .whitespaces).isEmpty
        case 1: return isRowValid(0)
        case 2: return isRowValid(1)
        case 3: return isRowValid(2)
        case 4: return isRowValid(3)
        case 5: return true
        default: return false
        }
    }

    private var totalMappedKeys: Int {
        (0..<4).reduce(0) { $0 + parsedCharacters(for: $1).count }
    }

    var body: some View {
        VStack(spacing: 0) {
            progressBar
            Divider()

            ZStack {
                switch currentStep {
                case 0: welcomeStep.transition(.asymmetric(insertion: .move(edge: .trailing), removal: .move(edge: .leading)))
                case 1: rowStep(0).transition(.asymmetric(insertion: .move(edge: animationDirection), removal: .move(edge: animationDirection == .trailing ? .leading : .trailing)))
                case 2: rowStep(1).transition(.asymmetric(insertion: .move(edge: animationDirection), removal: .move(edge: animationDirection == .trailing ? .leading : .trailing)))
                case 3: rowStep(2).transition(.asymmetric(insertion: .move(edge: animationDirection), removal: .move(edge: animationDirection == .trailing ? .leading : .trailing)))
                case 4: rowStep(3).transition(.asymmetric(insertion: .move(edge: animationDirection), removal: .move(edge: animationDirection == .trailing ? .leading : .trailing)))
                case 5: doneStep.transition(.asymmetric(insertion: .move(edge: .trailing), removal: .move(edge: .leading)))
                default: EmptyView()
                }
            }
            .animation(.spring(response: 0.4, dampingFraction: 0.85), value: currentStep)

            Divider()
            navigationBar
        }
        .frame(width: 620, height: 480)
        .background(Color(nsColor: .windowBackgroundColor))
    }

    // MARK: - Progress Bar

    private var progressBar: some View {
        VStack(spacing: 4) {
            HStack {
                Text("Step \(currentStep + 1) of \(totalSteps)")
                    .font(.caption)
                    .foregroundColor(.secondary)
                Spacer()
                if currentStep > 0 && currentStep < 5 {
                    Text("\(rowTitles[currentStep - 1])")
                        .font(.caption.weight(.medium))
                        .foregroundColor(.secondary)
                }
            }
            GeometryReader { geo in
                ZStack(alignment: .leading) {
                    RoundedRectangle(cornerRadius: 3)
                        .fill(Color(nsColor: .separatorColor))
                        .frame(height: 6)
                    RoundedRectangle(cornerRadius: 3)
                        .fill(Color.accentColor)
                        .frame(width: geo.size.width * CGFloat(currentStep) / CGFloat(totalSteps - 1), height: 6)
                        .animation(.spring(response: 0.4, dampingFraction: 0.85), value: currentStep)
                }
            }
            .frame(height: 6)
        }
        .padding(.horizontal, 24)
        .padding(.vertical, 12)
    }

    // MARK: - Welcome Step

    private var welcomeStep: some View {
        VStack(spacing: 20) {
            Spacer()

            Image(systemName: "keyboard")
                .font(.system(size: 56))
                .foregroundColor(.accentColor)
                .symbolEffect(.pulse, options: .repeating)

            Text("Set Up Your Keyboard")
                .font(.title.weight(.semibold))

            Text("This wizard will guide you through mapping your keyboard layout\nrow by row. You'll type each row in your target language so\nOrchid can learn the mapping.")
                .font(.body)
                .foregroundColor(.secondary)
                .multilineTextAlignment(.center)
                .lineSpacing(2)

            VStack(alignment: .leading, spacing: 6) {
                Text("Language name:")
                    .font(.subheadline.weight(.medium))
                TextField("e.g. Farsi, Arabic, Hebrew...", text: $languageName)
                    .textFieldStyle(.roundedBorder)
                    .frame(width: 280)
            }
            .padding(.top, 8)

            Spacer()
        }
        .frame(maxWidth: .infinity)
        .padding(.horizontal, 40)
    }

    // MARK: - Row Step

    private func rowStep(_ rowIndex: Int) -> some View {
        let chars = parsedCharacters(for: rowIndex)
        let expected = expectedCounts[rowIndex]
        let valid = chars.count == expected
        let sourceChars = rowLabels[rowIndex]

        return VStack(spacing: 16) {
            Text(rowTitles[rowIndex])
                .font(.title2.weight(.semibold))
                .padding(.top, 20)

            Text(rowDescriptions[rowIndex])
                .font(.subheadline)
                .foregroundColor(.secondary)

            // Show expected QWERTY keys
            HStack(spacing: 4) {
                ForEach(Array(sourceChars.enumerated()), id: \.offset) { idx, key in
                    Text(key)
                        .font(.system(size: 12, design: .monospaced))
                        .frame(width: 28, height: 28)
                        .background(
                            RoundedRectangle(cornerRadius: 4)
                                .fill(idx < chars.count ? Color.accentColor.opacity(0.15) : Color(nsColor: .controlBackgroundColor))
                        )
                        .overlay(
                            RoundedRectangle(cornerRadius: 4)
                                .stroke(idx < chars.count ? Color.accentColor : Color(nsColor: .separatorColor), lineWidth: 1)
                        )
                }
            }

            // Input field
            VStack(alignment: .leading, spacing: 6) {
                HStack {
                    Text("Type the \(expected) characters:")
                        .font(.subheadline.weight(.medium))
                    Spacer()
                    HStack(spacing: 4) {
                        Text("\(chars.count)/\(expected)")
                            .font(.caption.weight(.medium))
                            .foregroundColor(valid ? .green : (chars.count > expected ? .red : .secondary))
                        if valid {
                            Image(systemName: "checkmark.circle.fill")
                                .foregroundColor(.green)
                                .font(.caption)
                        }
                    }
                }
                TextField("Paste or type all \(expected) characters here...", text: $rowInputs[rowIndex])
                    .textFieldStyle(.roundedBorder)
                    .font(.system(size: 16, design: .monospaced))
            }
            .padding(.horizontal, 40)

            // Mapping preview
            if !chars.isEmpty {
                VStack(alignment: .leading, spacing: 4) {
                    Text("Mapping Preview")
                        .font(.caption.weight(.medium))
                        .foregroundColor(.secondary)

                    ScrollView(.horizontal, showsIndicators: false) {
                        HStack(spacing: 4) {
                            ForEach(Array(chars.prefix(expected).enumerated()), id: \.offset) { idx, char in
                                VStack(spacing: 2) {
                                    Text(sourceChars[idx])
                                        .font(.system(size: 10, design: .monospaced))
                                        .foregroundColor(.secondary)
                                    Image(systemName: "arrow.down")
                                        .font(.system(size: 8))
                                        .foregroundColor(.secondary)
                                    Text(char)
                                        .font(.system(size: 14, design: .monospaced))
                                }
                                .frame(width: 32, height: 52)
                                .background(
                                    RoundedRectangle(cornerRadius: 4)
                                        .fill(Color(nsColor: .controlBackgroundColor))
                                )
                            }
                        }
                    }
                }
                .padding(.horizontal, 40)
            }

            Spacer()
        }
        .frame(maxWidth: .infinity)
    }

    // MARK: - Done Step

    private var doneStep: some View {
        VStack(spacing: 20) {
            Spacer()

            Image(systemName: "checkmark.circle")
                .font(.system(size: 56))
                .foregroundColor(.green)
                .symbolEffect(.bounce, options: .nonRepeating)

            Text("Setup Complete!")
                .font(.title.weight(.semibold))

            Text("Your \(languageName) keyboard layout is ready.\n\(totalMappedKeys) keys have been mapped.")
                .font(.body)
                .foregroundColor(.secondary)
                .multilineTextAlignment(.center)
                .lineSpacing(2)

            // Summary
            VStack(alignment: .leading, spacing: 8) {
                ForEach(0..<4, id: \.self) { rowIndex in
                    let chars = parsedCharacters(for: rowIndex)
                    HStack {
                        Text(rowTitles[rowIndex])
                            .font(.subheadline.weight(.medium))
                            .frame(width: 120, alignment: .leading)
                        Text(chars.joined())
                            .font(.system(size: 13, design: .monospaced))
                            .lineLimit(1)
                            .foregroundColor(.secondary)
                        Spacer()
                        Image(systemName: "checkmark.circle.fill")
                            .foregroundColor(.green)
                            .font(.caption)
                    }
                }
            }
            .padding()
            .background(
                RoundedRectangle(cornerRadius: 8)
                    .fill(Color(nsColor: .controlBackgroundColor))
            )
            .padding(.horizontal, 40)

            Spacer()
        }
        .frame(maxWidth: .infinity)
    }

    // MARK: - Navigation

    private var navigationBar: some View {
        HStack {
            if currentStep > 0 && currentStep < 5 {
                Button("Back") {
                    animationDirection = .leading
                    withAnimation {
                        currentStep -= 1
                    }
                }
                .controlSize(.large)
            }

            Spacer()

            if currentStep < 5 {
                Button("Next") {
                    animationDirection = .trailing
                    if currentStep == 4 {
                        applyMappings()
                    }
                    withAnimation {
                        currentStep += 1
                    }
                }
                .controlSize(.large)
                .keyboardShortcut(.defaultAction)
                .disabled(!canAdvance)
            } else {
                Button("Finish") {
                    onFinish()
                }
                .controlSize(.large)
                .keyboardShortcut(.defaultAction)
            }
        }
        .padding(.horizontal, 24)
        .padding(.vertical, 12)
    }

    // MARK: - Apply

    private func applyMappings() {
        let rows = (0..<4).map { parsedCharacters(for: $0) }
        store.applySetupMappings(languageName: languageName.trimmingCharacters(in: .whitespaces), rows: rows)
    }
}
