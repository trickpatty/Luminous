//
//  SettingsView.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// Settings feature for app and account configuration.
struct SettingsView: View {
    // MARK: - State

    @EnvironmentObject private var appState: AppState
    @EnvironmentObject private var authViewModel: AuthViewModel

    @State private var showingSignOutConfirmation = false
    @State private var showingDeleteAccountConfirmation = false

    // MARK: - Body

    var body: some View {
        NavigationStack {
            List {
                // Account Section
                Section("Account") {
                    NavigationLink {
                        ProfileSettingsView()
                    } label: {
                        SettingsRow(icon: "person.circle", title: "Profile", color: DesignTokens.Colors.accent_500)
                    }

                    NavigationLink {
                        SecuritySettingsView()
                    } label: {
                        SettingsRow(icon: "lock.shield", title: "Security", color: DesignTokens.Colors.status_success)
                    }

                    NavigationLink {
                        NotificationSettingsView()
                    } label: {
                        SettingsRow(icon: "bell.badge", title: "Notifications", color: DesignTokens.Colors.status_warning)
                    }
                }

                // Family Section
                Section("Family") {
                    NavigationLink {
                        FamilySettingsView()
                    } label: {
                        SettingsRow(icon: "house", title: "Family Settings", color: DesignTokens.Colors.member_violet)
                    }

                    NavigationLink {
                        DeviceLinkingView()
                    } label: {
                        SettingsRow(icon: "display.and.arrow.down", title: "Link a Display", color: DesignTokens.Colors.member_teal)
                    }
                }

                // Appearance Section
                Section("Appearance") {
                    NavigationLink {
                        AppearanceSettingsView()
                    } label: {
                        SettingsRow(icon: "paintbrush", title: "Theme & Colors", color: DesignTokens.Colors.member_pink)
                    }
                }

                // Support Section
                Section("Support") {
                    NavigationLink {
                        HelpCenterView()
                    } label: {
                        SettingsRow(icon: "questionmark.circle", title: "Help Center", color: DesignTokens.Colors.accent_500)
                    }

                    NavigationLink {
                        AboutView()
                    } label: {
                        SettingsRow(icon: "info.circle", title: "About", color: DesignTokens.Colors.text_secondary)
                    }
                }

                // Sign Out Section
                Section {
                    Button(role: .destructive) {
                        showingSignOutConfirmation = true
                    } label: {
                        HStack {
                            Image(systemName: "rectangle.portrait.and.arrow.right")
                            Text("Sign Out")
                        }
                    }
                }

                // App Version
                Section {
                    HStack {
                        Text("Version")
                        Spacer()
                        Text(AppConfiguration.fullVersion)
                            .foregroundStyle(DesignTokens.Colors.text_secondary)
                    }
                }
            }
            .navigationTitle("Settings")
            .alert("Sign Out", isPresented: $showingSignOutConfirmation) {
                Button("Cancel", role: .cancel) {}
                Button("Sign Out", role: .destructive) {
                    authViewModel.signOut()
                }
            } message: {
                Text("Are you sure you want to sign out?")
            }
        }
    }
}

// MARK: - Settings Row

private struct SettingsRow: View {
    let icon: String
    let title: String
    let color: Color

    var body: some View {
        HStack(spacing: DesignTokens.Spacing._3) {
            Image(systemName: icon)
                .font(.system(size: 18))
                .foregroundStyle(color)
                .frame(width: 28)

            Text(title)
                .font(.system(size: DesignTokens.FontSize.body_md))
        }
    }
}

// MARK: - Placeholder Settings Views

struct ProfileSettingsView: View {
    var body: some View {
        Text("Profile settings - Coming soon")
            .navigationTitle("Profile")
    }
}

struct SecuritySettingsView: View {
    var body: some View {
        List {
            Section("Passkeys") {
                Text("Manage your passkeys")
            }

            Section("Biometric") {
                Toggle("Use Face ID", isOn: .constant(true))
            }

            Section("Sessions") {
                Text("Active sessions")
            }
        }
        .navigationTitle("Security")
    }
}

struct NotificationSettingsView: View {
    @State private var eventsEnabled = true
    @State private var tasksEnabled = true
    @State private var remindersEnabled = true

    var body: some View {
        List {
            Section {
                Toggle("Event Notifications", isOn: $eventsEnabled)
                Toggle("Task Reminders", isOn: $tasksEnabled)
                Toggle("Daily Summary", isOn: $remindersEnabled)
            }

            Section("Quiet Hours") {
                Text("Configure quiet hours")
            }
        }
        .navigationTitle("Notifications")
    }
}

struct FamilySettingsView: View {
    var body: some View {
        Text("Family settings - Coming soon")
            .navigationTitle("Family Settings")
    }
}

struct DeviceLinkingView: View {
    var body: some View {
        VStack(spacing: DesignTokens.Spacing._6) {
            Image(systemName: "display")
                .font(.system(size: 64))
                .foregroundStyle(DesignTokens.Colors.accent_500)

            Text("Link a Display")
                .font(.system(size: DesignTokens.FontSize.title_lg, weight: .semibold))

            Text("Enter the 6-digit code shown on your display to link it to your family.")
                .font(.system(size: DesignTokens.FontSize.body_md))
                .foregroundStyle(DesignTokens.Colors.text_secondary)
                .multilineTextAlignment(.center)
                .padding(.horizontal)

            // Code entry field placeholder
            HStack(spacing: DesignTokens.Spacing._2) {
                ForEach(0..<6, id: \.self) { _ in
                    RoundedRectangle(cornerRadius: DesignTokens.Radius.md)
                        .stroke(DesignTokens.Colors.border_default, lineWidth: 1)
                        .frame(width: 48, height: 56)
                }
            }
            .padding(.vertical)

            Spacer()
        }
        .padding()
        .navigationTitle("Link Display")
    }
}

struct AppearanceSettingsView: View {
    @State private var selectedScheme = 0

    var body: some View {
        List {
            Section("Theme") {
                Picker("Appearance", selection: $selectedScheme) {
                    Text("System").tag(0)
                    Text("Light").tag(1)
                    Text("Dark").tag(2)
                }
                .pickerStyle(.segmented)
            }
        }
        .navigationTitle("Appearance")
    }
}

struct HelpCenterView: View {
    var body: some View {
        List {
            Section {
                Link(destination: URL(string: "https://luminous.app/help")!) {
                    SettingsRow(icon: "safari", title: "Visit Help Center", color: DesignTokens.Colors.accent_500)
                }

                Link(destination: URL(string: "mailto:support@luminous.app")!) {
                    SettingsRow(icon: "envelope", title: "Contact Support", color: DesignTokens.Colors.accent_500)
                }
            }
        }
        .navigationTitle("Help Center")
    }
}

struct AboutView: View {
    var body: some View {
        List {
            Section {
                HStack {
                    Text("Version")
                    Spacer()
                    Text(AppConfiguration.fullVersion)
                        .foregroundStyle(DesignTokens.Colors.text_secondary)
                }

                HStack {
                    Text("Environment")
                    Spacer()
                    Text(AppConfiguration.environment.name)
                        .foregroundStyle(DesignTokens.Colors.text_secondary)
                }
            }

            Section {
                Link("Privacy Policy", destination: URL(string: "https://luminous.app/privacy")!)
                Link("Terms of Service", destination: URL(string: "https://luminous.app/terms")!)
                Link("Open Source Licenses", destination: URL(string: "https://luminous.app/licenses")!)
            }
        }
        .navigationTitle("About")
    }
}

// MARK: - Preview

#Preview {
    SettingsView()
        .environmentObject(AppState())
        .environmentObject(AuthViewModel())
}
