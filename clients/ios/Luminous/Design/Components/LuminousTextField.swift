//
//  LuminousTextField.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// A text field component following the Luminous design system.
struct LuminousTextField: View {
    // MARK: - Properties

    let placeholder: String
    @Binding var text: String
    let keyboardType: UIKeyboardType
    let textContentType: UITextContentType?
    let isSecure: Bool
    let leadingIcon: String?
    let trailingIcon: String?
    let errorMessage: String?
    let isDisabled: Bool
    let onTrailingIconTap: (() -> Void)?

    // MARK: - State

    @FocusState private var isFocused: Bool
    @State private var isSecureVisible: Bool = false

    // MARK: - Initialization

    init(
        placeholder: String,
        text: Binding<String>,
        keyboardType: UIKeyboardType = .default,
        textContentType: UITextContentType? = nil,
        isSecure: Bool = false,
        leadingIcon: String? = nil,
        trailingIcon: String? = nil,
        errorMessage: String? = nil,
        isDisabled: Bool = false,
        onTrailingIconTap: (() -> Void)? = nil
    ) {
        self.placeholder = placeholder
        self._text = text
        self.keyboardType = keyboardType
        self.textContentType = textContentType
        self.isSecure = isSecure
        self.leadingIcon = leadingIcon
        self.trailingIcon = trailingIcon
        self.errorMessage = errorMessage
        self.isDisabled = isDisabled
        self.onTrailingIconTap = onTrailingIconTap
    }

    // MARK: - Body

    var body: some View {
        VStack(alignment: .leading, spacing: DesignTokens.Spacing._1) {
            HStack(spacing: DesignTokens.Spacing._2) {
                // Leading icon
                if let leadingIcon {
                    Image(systemName: leadingIcon)
                        .font(.system(size: 18))
                        .foregroundStyle(iconColor)
                        .frame(width: 24)
                }

                // Text field
                Group {
                    if isSecure && !isSecureVisible {
                        SecureField(placeholder, text: $text)
                    } else {
                        TextField(placeholder, text: $text)
                    }
                }
                .keyboardType(keyboardType)
                .textContentType(textContentType)
                .autocorrectionDisabled()
                .textInputAutocapitalization(.never)
                .focused($isFocused)
                .disabled(isDisabled)

                // Trailing icon / secure toggle
                if isSecure {
                    Button {
                        isSecureVisible.toggle()
                    } label: {
                        Image(systemName: isSecureVisible ? "eye.slash" : "eye")
                            .font(.system(size: 18))
                            .foregroundStyle(iconColor)
                    }
                    .frame(width: 24)
                } else if let trailingIcon {
                    Button {
                        onTrailingIconTap?()
                    } label: {
                        Image(systemName: trailingIcon)
                            .font(.system(size: 18))
                            .foregroundStyle(iconColor)
                    }
                    .frame(width: 24)
                    .disabled(onTrailingIconTap == nil)
                }
            }
            .padding(.horizontal, DesignTokens.Spacing._4)
            .frame(height: DesignTokens.Touch.md)
            .background(backgroundColor)
            .clipShape(RoundedRectangle(cornerRadius: DesignTokens.Radius.md))
            .overlay {
                RoundedRectangle(cornerRadius: DesignTokens.Radius.md)
                    .stroke(borderColor, lineWidth: 1)
            }

            // Error message
            if let errorMessage {
                Text(errorMessage)
                    .font(.system(size: DesignTokens.FontSize.caption))
                    .foregroundStyle(DesignTokens.Colors.status_danger)
            }
        }
    }

    // MARK: - Private Computed Properties

    private var borderColor: Color {
        if errorMessage != nil {
            return DesignTokens.Colors.status_danger
        }
        if isFocused {
            return DesignTokens.Colors.accent_500
        }
        return DesignTokens.Colors.border_default
    }

    private var backgroundColor: Color {
        if isDisabled {
            return DesignTokens.Colors.surface_secondary
        }
        return DesignTokens.Colors.surface_primary
    }

    private var iconColor: Color {
        if isFocused {
            return DesignTokens.Colors.accent_500
        }
        return DesignTokens.Colors.text_tertiary
    }
}

// MARK: - Preview

#Preview {
    VStack(spacing: 16) {
        LuminousTextField(
            placeholder: "Email address",
            text: .constant(""),
            keyboardType: .emailAddress,
            textContentType: .emailAddress,
            leadingIcon: "envelope"
        )

        LuminousTextField(
            placeholder: "Password",
            text: .constant("secret"),
            isSecure: true,
            leadingIcon: "lock"
        )

        LuminousTextField(
            placeholder: "Search",
            text: .constant(""),
            leadingIcon: "magnifyingglass",
            trailingIcon: "xmark.circle.fill"
        ) {
            // Clear action
        }

        LuminousTextField(
            placeholder: "With error",
            text: .constant("invalid"),
            errorMessage: "Please enter a valid value"
        )

        LuminousTextField(
            placeholder: "Disabled",
            text: .constant(""),
            isDisabled: true
        )
    }
    .padding()
}
