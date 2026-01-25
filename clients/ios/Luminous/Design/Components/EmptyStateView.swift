//
//  EmptyStateView.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// A view for displaying empty states with an icon, title, and optional action.
struct EmptyStateView: View {
    // MARK: - Properties

    let icon: String
    let title: String
    let subtitle: String?
    let actionTitle: String?
    let action: (() -> Void)?

    // MARK: - Initialization

    init(
        icon: String,
        title: String,
        subtitle: String? = nil,
        actionTitle: String? = nil,
        action: (() -> Void)? = nil
    ) {
        self.icon = icon
        self.title = title
        self.subtitle = subtitle
        self.actionTitle = actionTitle
        self.action = action
    }

    // MARK: - Body

    var body: some View {
        VStack(spacing: DesignTokens.Spacing._4) {
            Image(systemName: icon)
                .font(.system(size: 48))
                .foregroundStyle(DesignTokens.Colors.text_tertiary)

            VStack(spacing: DesignTokens.Spacing._1) {
                Text(title)
                    .font(.system(size: DesignTokens.FontSize.title_sm, weight: .semibold))
                    .foregroundStyle(DesignTokens.Colors.text_primary)
                    .multilineTextAlignment(.center)

                if let subtitle {
                    Text(subtitle)
                        .font(.system(size: DesignTokens.FontSize.body_sm))
                        .foregroundStyle(DesignTokens.Colors.text_secondary)
                        .multilineTextAlignment(.center)
                }
            }

            if let actionTitle, let action {
                Button(action: action) {
                    Text(actionTitle)
                        .font(.system(size: DesignTokens.FontSize.body_md, weight: .medium))
                        .foregroundStyle(DesignTokens.Colors.accent_600)
                }
                .padding(.top, DesignTokens.Spacing._2)
            }
        }
        .frame(maxWidth: .infinity)
        .padding(.vertical, DesignTokens.Spacing._8)
    }
}

// MARK: - Preview

#Preview {
    VStack(spacing: 32) {
        EmptyStateView(
            icon: "calendar.badge.exclamationmark",
            title: "No Events",
            subtitle: "You don't have any events scheduled"
        )

        EmptyStateView(
            icon: "checkmark.circle",
            title: "All Done!",
            subtitle: "You've completed all your tasks",
            actionTitle: "Add a new task"
        ) {
            // Action
        }
    }
    .padding()
}
