//
//  LuminousCard.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// A card container component following the Luminous design system.
struct LuminousCard<Content: View>: View {
    // MARK: - Properties

    let padding: CGFloat
    let cornerRadius: CGFloat
    let hasShadow: Bool
    let content: Content

    // MARK: - Initialization

    init(
        padding: CGFloat = DesignTokens.Spacing._4,
        cornerRadius: CGFloat = DesignTokens.Radius.lg,
        hasShadow: Bool = true,
        @ViewBuilder content: () -> Content
    ) {
        self.padding = padding
        self.cornerRadius = cornerRadius
        self.hasShadow = hasShadow
        self.content = content()
    }

    // MARK: - Body

    var body: some View {
        content
            .padding(padding)
            .background(DesignTokens.Colors.surface_primary)
            .clipShape(RoundedRectangle(cornerRadius: cornerRadius))
            .if(hasShadow) { view in
                view.cardShadow()
            }
    }
}

// MARK: - Conditional Modifier Extension

extension View {
    @ViewBuilder
    func `if`<Content: View>(_ condition: Bool, transform: (Self) -> Content) -> some View {
        if condition {
            transform(self)
        } else {
            self
        }
    }
}

// MARK: - Preview

#Preview {
    VStack(spacing: 16) {
        LuminousCard {
            VStack(alignment: .leading, spacing: 8) {
                Text("Card Title")
                    .font(.headline)
                Text("This is some card content that demonstrates the card component.")
                    .foregroundStyle(.secondary)
            }
        }

        LuminousCard(padding: DesignTokens.Spacing._6, hasShadow: false) {
            Text("Card without shadow")
        }
    }
    .padding()
    .background(DesignTokens.Colors.canvas_morning)
}
