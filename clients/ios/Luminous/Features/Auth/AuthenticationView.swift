//
//  AuthenticationView.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI
import AuthenticationServices

/// Authentication view supporting passkey and email OTP login.
struct AuthenticationView: View {
    // MARK: - State

    @EnvironmentObject private var authViewModel: AuthViewModel
    @EnvironmentObject private var appState: AppState

    @State private var authMode: AuthMode = .signIn
    @State private var email = ""
    @State private var otpCode = ""
    @State private var showingOTPEntry = false

    // MARK: - Body

    var body: some View {
        NavigationStack {
            ZStack {
                appState.canvasColor.ignoresSafeArea()

                ScrollView {
                    VStack(spacing: DesignTokens.Spacing._8) {
                        // Logo and branding
                        VStack(spacing: DesignTokens.Spacing._3) {
                            Image(systemName: "house.fill")
                                .font(.system(size: 56))
                                .foregroundStyle(DesignTokens.Colors.accent_600)

                            Text("Luminous")
                                .font(.system(size: DesignTokens.FontSize.display_sm, weight: .bold))
                                .foregroundStyle(DesignTokens.Colors.text_primary)

                            Text("Your family command center")
                                .font(.system(size: DesignTokens.FontSize.body_md))
                                .foregroundStyle(DesignTokens.Colors.text_secondary)
                        }
                        .padding(.top, DesignTokens.Spacing._16)

                        // Auth mode picker
                        Picker("", selection: $authMode) {
                            Text("Sign In").tag(AuthMode.signIn)
                            Text("Sign Up").tag(AuthMode.signUp)
                        }
                        .pickerStyle(.segmented)
                        .padding(.horizontal, DesignTokens.Spacing._8)

                        // Main content
                        VStack(spacing: DesignTokens.Spacing._6) {
                            if showingOTPEntry {
                                OTPEntryView(
                                    code: $otpCode,
                                    email: email,
                                    onVerify: verifyOTP,
                                    onResend: requestOTP,
                                    onBack: { showingOTPEntry = false }
                                )
                            } else {
                                // Passkey sign in button
                                PasskeyButton(
                                    mode: authMode,
                                    action: handlePasskeyAuth
                                )

                                // Divider
                                HStack {
                                    Rectangle()
                                        .fill(DesignTokens.Colors.border_default)
                                        .frame(height: 1)

                                    Text("or")
                                        .font(.system(size: DesignTokens.FontSize.body_sm))
                                        .foregroundStyle(DesignTokens.Colors.text_tertiary)
                                        .padding(.horizontal, DesignTokens.Spacing._3)

                                    Rectangle()
                                        .fill(DesignTokens.Colors.border_default)
                                        .frame(height: 1)
                                }

                                // Email input
                                VStack(spacing: DesignTokens.Spacing._3) {
                                    LuminousTextField(
                                        placeholder: "Email address",
                                        text: $email,
                                        keyboardType: .emailAddress,
                                        textContentType: .emailAddress
                                    )

                                    LuminousButton(
                                        title: authMode == .signIn ? "Continue with Email" : "Create Account",
                                        style: .secondary,
                                        isLoading: authViewModel.isLoading
                                    ) {
                                        requestOTP()
                                    }
                                    .disabled(email.isEmpty || !isValidEmail(email))
                                }
                            }
                        }
                        .padding(.horizontal, DesignTokens.Spacing._6)

                        Spacer()

                        // Error display
                        if let error = authViewModel.error {
                            ErrorBanner(message: error.localizedDescription)
                                .padding(.horizontal, DesignTokens.Spacing._4)
                        }

                        // Footer
                        VStack(spacing: DesignTokens.Spacing._2) {
                            Text("By continuing, you agree to our")
                                .font(.system(size: DesignTokens.FontSize.caption))
                                .foregroundStyle(DesignTokens.Colors.text_tertiary)

                            HStack(spacing: DesignTokens.Spacing._1) {
                                Link("Terms of Service", destination: URL(string: "https://luminous.app/terms")!)
                                Text("and")
                                    .foregroundStyle(DesignTokens.Colors.text_tertiary)
                                Link("Privacy Policy", destination: URL(string: "https://luminous.app/privacy")!)
                            }
                            .font(.system(size: DesignTokens.FontSize.caption))
                        }
                        .padding(.bottom, DesignTokens.Spacing._6)
                    }
                }
            }
        }
    }

    // MARK: - Private Methods

    private func handlePasskeyAuth() {
        Task {
            if authMode == .signIn {
                await authViewModel.signInWithPasskey()
            } else {
                // For sign up, we need email first
                if email.isEmpty {
                    // Focus email field or show prompt
                } else {
                    await authViewModel.signUpWithPasskey(email: email)
                }
            }
        }
    }

    private func requestOTP() {
        Task {
            let success = await authViewModel.requestOTP(email: email)
            if success {
                showingOTPEntry = true
            }
        }
    }

    private func verifyOTP() {
        Task {
            await authViewModel.verifyOTP(email: email, code: otpCode)
        }
    }

    private func isValidEmail(_ email: String) -> Bool {
        let emailRegex = "[A-Z0-9a-z._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,64}"
        let emailPredicate = NSPredicate(format: "SELF MATCHES %@", emailRegex)
        return emailPredicate.evaluate(with: email)
    }
}

// MARK: - Auth Mode

enum AuthMode {
    case signIn
    case signUp
}

// MARK: - Passkey Button

private struct PasskeyButton: View {
    let mode: AuthMode
    let action: () -> Void

    var body: some View {
        Button(action: action) {
            HStack(spacing: DesignTokens.Spacing._3) {
                Image(systemName: "person.badge.key.fill")
                    .font(.system(size: 20))

                Text(mode == .signIn ? "Sign in with Passkey" : "Sign up with Passkey")
                    .font(.system(size: DesignTokens.FontSize.body_md, weight: .semibold))
            }
            .foregroundStyle(.white)
            .frame(maxWidth: .infinity)
            .frame(height: DesignTokens.Touch.lg)
            .background(DesignTokens.Colors.accent_600)
            .clipShape(RoundedRectangle(cornerRadius: DesignTokens.Radius.md))
        }
        .buttonStyle(.plain)
    }
}

// MARK: - OTP Entry View

private struct OTPEntryView: View {
    @Binding var code: String
    let email: String
    let onVerify: () -> Void
    let onResend: () -> Void
    let onBack: () -> Void

    @FocusState private var isFocused: Bool

    var body: some View {
        VStack(spacing: DesignTokens.Spacing._6) {
            VStack(spacing: DesignTokens.Spacing._2) {
                Text("Enter verification code")
                    .font(.system(size: DesignTokens.FontSize.title_md, weight: .semibold))
                    .foregroundStyle(DesignTokens.Colors.text_primary)

                Text("We sent a code to \(email)")
                    .font(.system(size: DesignTokens.FontSize.body_sm))
                    .foregroundStyle(DesignTokens.Colors.text_secondary)
            }

            // OTP code input
            TextField("", text: $code)
                .font(.system(size: DesignTokens.FontSize.display_sm, weight: .medium, design: .monospaced))
                .keyboardType(.numberPad)
                .textContentType(.oneTimeCode)
                .multilineTextAlignment(.center)
                .frame(height: DesignTokens.Touch.xl)
                .focused($isFocused)
                .onAppear { isFocused = true }

            LuminousButton(
                title: "Verify",
                style: .primary,
                isLoading: false
            ) {
                onVerify()
            }
            .disabled(code.count < 6)

            HStack(spacing: DesignTokens.Spacing._4) {
                Button("Resend code") {
                    onResend()
                }
                .font(.system(size: DesignTokens.FontSize.body_sm))
                .foregroundStyle(DesignTokens.Colors.accent_600)

                Button("Use different email") {
                    onBack()
                }
                .font(.system(size: DesignTokens.FontSize.body_sm))
                .foregroundStyle(DesignTokens.Colors.text_secondary)
            }
        }
    }
}

// MARK: - Error Banner

private struct ErrorBanner: View {
    let message: String

    var body: some View {
        HStack(spacing: DesignTokens.Spacing._2) {
            Image(systemName: "exclamationmark.triangle.fill")
                .foregroundStyle(DesignTokens.Colors.status_danger)

            Text(message)
                .font(.system(size: DesignTokens.FontSize.body_sm))
                .foregroundStyle(DesignTokens.Colors.status_danger_dark)
        }
        .padding()
        .frame(maxWidth: .infinity, alignment: .leading)
        .background(DesignTokens.Colors.status_danger_light)
        .clipShape(RoundedRectangle(cornerRadius: DesignTokens.Radius.md))
    }
}

// MARK: - Preview

#Preview {
    AuthenticationView()
        .environmentObject(AppState())
        .environmentObject(AuthViewModel())
}
