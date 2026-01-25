//
//  ContentView.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// Root content view that manages navigation between authentication and main app flows.
struct ContentView: View {
    // MARK: - Environment

    @EnvironmentObject private var appState: AppState
    @EnvironmentObject private var authViewModel: AuthViewModel

    // MARK: - Body

    var body: some View {
        Group {
            if appState.isLoading {
                LaunchScreen()
            } else if authViewModel.isAuthenticated {
                MainTabView()
            } else {
                AuthenticationView()
            }
        }
        .animation(.easeInOut(duration: 0.3), value: authViewModel.isAuthenticated)
        .animation(.easeInOut(duration: 0.2), value: appState.isLoading)
    }
}

// MARK: - Launch Screen

/// Displayed while the app is initializing.
struct LaunchScreen: View {
    var body: some View {
        ZStack {
            DesignTokens.Colors.canvas_morning
                .ignoresSafeArea()

            VStack(spacing: DesignTokens.Spacing._4) {
                Image(systemName: "house.fill")
                    .font(.system(size: 64))
                    .foregroundStyle(DesignTokens.Colors.accent_600)

                Text("Luminous")
                    .font(.system(size: DesignTokens.FontSize.title_lg, weight: .semibold))
                    .foregroundStyle(DesignTokens.Colors.text_primary)

                ProgressView()
                    .progressViewStyle(.circular)
                    .tint(DesignTokens.Colors.accent_500)
                    .padding(.top, DesignTokens.Spacing._4)
            }
        }
    }
}

// MARK: - Preview

#Preview {
    ContentView()
        .environmentObject(AppState())
        .environmentObject(AuthViewModel())
}
