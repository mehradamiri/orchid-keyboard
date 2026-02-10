import SwiftUI

struct KeyView: View {
    let physicalKey: PhysicalKey
    let mapping: KeyMapping?
    let isSelected: Bool
    let onTap: () -> Void

    @State private var isHovering = false

    private let baseSize: CGFloat = 48

    private var isMapped: Bool {
        guard let m = mapping else { return false }
        return !m.lowercase.target.isEmpty && m.lowercase.source != m.lowercase.target
    }

    private var sourceLabel: String {
        physicalKey.lowercaseChar
    }

    private var targetLabel: String {
        mapping?.lowercase.target ?? ""
    }

    var body: some View {
        let width = baseSize * physicalKey.relativeWidth + (physicalKey.relativeWidth - 1) * 4

        VStack(spacing: 1) {
            if physicalKey.isModifier {
                Text(physicalKey.label)
                    .font(.system(size: 10, weight: .medium))
                    .foregroundColor(.secondary)
            } else {
                Text(sourceLabel)
                    .font(.system(size: 11))
                    .foregroundColor(.secondary)
                Text(targetLabel)
                    .font(.system(size: 16, weight: .medium))
                    .foregroundColor(isMapped ? .primary : .secondary.opacity(0.4))
            }
        }
        .frame(width: width, height: baseSize)
        .background(keyBackground)
        .clipShape(RoundedRectangle(cornerRadius: 7, style: .continuous))
        .overlay(
            RoundedRectangle(cornerRadius: 7, style: .continuous)
                .stroke(isSelected ? Color.accentColor : Color.clear, lineWidth: 2)
        )
        .shadow(color: isSelected ? Color.accentColor.opacity(0.4) : Color.black.opacity(isHovering ? 0.15 : 0.05),
                radius: isSelected ? 6 : (isHovering ? 4 : 2),
                y: isHovering ? 2 : 1)
        .scaleEffect(isHovering && !physicalKey.isModifier ? 1.08 : 1.0)
        .animation(.spring(response: 0.3, dampingFraction: 0.7), value: isHovering)
        .animation(.spring(response: 0.3, dampingFraction: 0.7), value: isSelected)
        .onHover { hovering in
            isHovering = hovering
        }
        .onTapGesture {
            if !physicalKey.isModifier {
                onTap()
            }
        }
    }

    @ViewBuilder
    private var keyBackground: some View {
        if physicalKey.isModifier {
            Color(nsColor: .controlBackgroundColor).opacity(0.5)
        } else if isMapped {
            LinearGradient(
                colors: [Color(nsColor: .controlBackgroundColor),
                         Color(nsColor: .controlBackgroundColor).opacity(0.85)],
                startPoint: .top,
                endPoint: .bottom
            )
        } else {
            Color(nsColor: .controlBackgroundColor).opacity(0.3)
        }
    }
}
