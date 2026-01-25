//
//  LuminousButton.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// A button component following the Luminous design system.
struct LuminousButton: View {
    // MARK: - Properties

    let title: String
    let style: ButtonStyle
    let size: ButtonSize
    let isLoading: Bool
    let isFullWidth: Bool
    let icon: String?
    let action: () -> Void

    // MARK: - Initialization

    init(
        title: String,
        style: ButtonStyle = .primary,
        size: ButtonSize = .medium,
        isLoading: Bool = false,
        isFullWidth: Bool = true,
        icon: String? = nil,
        action: @escaping () -> Void
    ) {
        self.title = title
        self.style = style
        self.size = size
        self.isLoading = isLoading
        self.isFullWidth = isFullWidth
        self.icon = icon
        self.action = action
    }

    // MARK: - Body

    var body: some View {
        Button(action: action) {
            HStack(spacing: DesignTokens.Spacing._2) {
                if isLoading {
                    ProgressView()
                        .progressViewStyle(.circular)
                        .tint(style.textColor)
                        .scaleEffect(0.8)
                } else {
                    if let icon {
                        Image(systemName: icon)
                            .font(.system(size: size.iconSize))
                    }

                    Text(title)
                        .font(.system(size: size.fontSize, weight: .semibold))
                }
            }
            .foregroundStyle(style.textColor)
            .frame(maxWidth: isFullWidth ? .infinity : nil)
            .frame(height: size.height)
            .padding(.horizontal, size.horizontalPadding)
            .background(style.backgroundColor)
            .clipShape(RoundedRectangle(cornerRadius: DesignTokens.Radius.md))
            .overlay {
                if style == .secondary || style == .outline {
                    RoundedRectangle(cornerRadius: DesignTokens.Radius.md)
                        .stroke(style.borderColor, lineWidth: 1)
                }
            }
        }
        .buttonStyle(.plain)
        .disabled(isLoading)
        .opacity(isLoading ? 0.7 : 1.0)
    }

    // MARK: - Button Style

    enum ButtonStyle {
        case primary
        case secondary
        case outline
        case destructive
        case ghost

        var backgroundColor: Color {
            switch self {
            case .primary: return DesignTokens.Colors.accent_600
            case .secondary: return DesignTokens.Colors.surface_secondary
            case .outline: return .clear
            case .destructive: return DesignTokens.Colors.status_danger
            case .ghost: return .clear
            }
        }

        var textColor: Color {
            switch self {
            case .primary: return .white
            case .secondary: return DesignTokens.Colors.text_primary
            case .outline: return DesignTokens.Colors.accent_600
            case .destructive: return .white
            case .ghost: return DesignTokens.Colors.accent_600
            }
        }

        var borderColor: Color {
            switch self {
            case .secondary: return DesignTokens.Colors.border_default
            case .outline: return DesignTokens.Colors.accent_600
            default: return .clear
            }
        }
    }

    // MARK: - Button Size

    enum ButtonSize {
        case small
        case medium
        case large

        var height: CGFloat {
            switch self {
            case .small: return DesignTokens.Touch.min
            case .medium: return DesignTokens.Touch.md
            case .large: return DesignTokens.Touch.lg
            }
        }

        var fontSize: CGFloat {
            switch self {
            case .small: return DesignTokens.FontSize.body_sm
            case .medium: return DesignTokens.FontSize.body_md
            case .large: return DesignTokens.FontSize.body_lg
            }
        }

        var iconSize: CGFloat {
            switch self {
            case .small: return 14
            case .medium: return 16
            case .large: return 18
            }
        }

        var horizontalPadding: CGFloat {
            switch self {
            case .small: return DesignTokens.Spacing._3
            case .medium: return DesignTokens.Spacing._4
            case .large: return DesignTokens.Spacing._6
            }
        }
    }
}

// MARK: - Preview

#Preview {
    VStack(spacing: 16) {
        LuminousButton(title: "Primary Button", style: .primary) {}
        LuminousButton(title: "Secondary Button", style: .secondary) {}
        LuminousButton(title: "Outline Button", style: .outline) {}
        LuminousButton(title: "Destructive Button", style: .destructive) {}
        LuminousButton(title: "Ghost Button", style: .ghost) {}
        LuminousButton(title: "Loading...", style: .primary, isLoading: true) {}
        LuminousButton(title: "With Icon", style: .primary, icon: "plus") {}
    }
    .padding()
}
