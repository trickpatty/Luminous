//
//  LuminousApp.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// Main entry point for the Luminous iOS application.
///
/// The app uses SwiftUI App lifecycle with MVVM architecture.
/// Minimum deployment target: iOS 17.0
@main
struct LuminousApp: App {
    // MARK: - State

    @StateObject private var appState = AppState()
    @StateObject private var authViewModel = AuthViewModel()

    // MARK: - Environment

    @Environment(\.scenePhase) private var scenePhase

    // MARK: - Body

    var body: some Scene {
        WindowGroup {
            ContentView()
                .environmentObject(appState)
                .environmentObject(authViewModel)
                .preferredColorScheme(appState.colorScheme)
        }
        .onChange(of: scenePhase) { _, newPhase in
            handleScenePhaseChange(newPhase)
        }
    }

    // MARK: - Private Methods

    private func handleScenePhaseChange(_ phase: ScenePhase) {
        switch phase {
        case .active:
            Task {
                await appState.onBecomeActive()
            }
        case .inactive:
            appState.onBecomeInactive()
        case .background:
            appState.onEnterBackground()
        @unknown default:
            break
        }
    }
}
