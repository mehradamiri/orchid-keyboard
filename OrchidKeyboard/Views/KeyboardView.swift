import SwiftUI

struct KeyboardView: View {
    @ObservedObject var store: MappingStore
    @Binding var selectedKeyID: String?

    private let keySpacing: CGFloat = 4
    private let quarterKeyWidth: CGFloat = 12 // stagger unit = 1/4 of standard key

    // Row stagger offsets in quarter-key-widths
    private let rowStagger: [CGFloat] = [0, 0.5, 0.75, 1.25]

    var body: some View {
        VStack(spacing: keySpacing) {
            ForEach(Array(KeyboardLayoutData.allRows.enumerated()), id: \.offset) { rowIndex, row in
                HStack(spacing: keySpacing) {
                    // Stagger offset
                    if rowStagger[rowIndex] > 0 {
                        Spacer()
                            .frame(width: rowStagger[rowIndex] * quarterKeyWidth)
                    }

                    ForEach(row) { key in
                        KeyView(
                            physicalKey: key,
                            mapping: store.mapping(for: key.id),
                            isSelected: selectedKeyID == key.id,
                            onTap: {
                                if selectedKeyID == key.id {
                                    selectedKeyID = nil
                                } else {
                                    selectedKeyID = key.id
                                }
                            }
                        )
                    }

                    // Balance stagger on right side
                    if rowStagger[rowIndex] > 0 {
                        Spacer()
                            .frame(width: rowStagger[rowIndex] * quarterKeyWidth)
                    }
                }
            }
        }
    }
}
