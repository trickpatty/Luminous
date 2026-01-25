//
//  AppState.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI
import Combine

/// Global application state that manages app-wide concerns.
///
/// AppState is responsible for:
/// - App lifecycle events
/// - Theme/appearance settings
/// - Network connectivity monitoring
/// - Global error handling
@MainActor
final class AppState: ObservableObject {
    // MARK: - Published Properties

    /// Whether the app is currently loading/initializing.
    @Published var isLoading: Bool = true

    /// Current color scheme preference.
    @Published var colorScheme: ColorScheme? = nil

    /// Whether the device has network connectivity.
    @Published var isConnected: Bool = true

    /// Global error to display to the user.
    @Published var globalError: AppError? = nil

    /// Current canvas color based on time of day.
    @Published var canvasColor: Color = DesignTokens.Colors.canvas_morning

    // MARK: - Private Properties

    private var cancellables = Set<AnyCancellable>()
    private var canvasUpdateTimer: Timer?

    // MARK: - Initialization

    init() {
        setupCanvasColorUpdater()
        Task {
            await initialize()
        }
    }

    deinit {
        canvasUpdateTimer?.invalidate()
    }

    // MARK: - Public Methods

    /// Called when the app becomes active (foreground).
    func onBecomeActive() async {
        updateCanvasColor()
        // Resume any paused operations
    }

    /// Called when the app becomes inactive.
    func onBecomeInactive() {
        // Pause non-essential operations
    }

    /// Called when the app enters background.
    func onEnterBackground() {
        // Save state, prepare for potential termination
    }

    /// Display a global error to the user.
    func showError(_ error: AppError) {
        globalError = error
    }

    /// Clear the current global error.
    func clearError() {
        globalError = nil
    }

    // MARK: - Private Methods

    private func initialize() async {
        // Simulate initialization delay for smooth transition
        try? await Task.sleep(nanoseconds: 500_000_000) // 0.5 seconds

        // Load saved preferences
        loadPreferences()

        // Mark initialization complete
        isLoading = false
    }

    private func loadPreferences() {
        // Load color scheme preference from UserDefaults
        if let schemeRawValue = UserDefaults.standard.string(forKey: "colorScheme") {
            switch schemeRawValue {
            case "light":
                colorScheme = .light
            case "dark":
                colorScheme = .dark
            default:
                colorScheme = nil // System default
            }
        }
    }

    private func setupCanvasColorUpdater() {
        updateCanvasColor()

        // Update canvas color every minute
        canvasUpdateTimer = Timer.scheduledTimer(withTimeInterval: 60, repeats: true) { [weak self] _ in
            Task { @MainActor in
                self?.updateCanvasColor()
            }
        }
    }

    private func updateCanvasColor() {
        let hour = Calendar.current.component(.hour, from: Date())

        switch hour {
        case 5..<7:
            canvasColor = DesignTokens.Colors.canvas_dawn
        case 7..<12:
            canvasColor = DesignTokens.Colors.canvas_morning
        case 12..<17:
            canvasColor = DesignTokens.Colors.canvas_afternoon
        case 17..<21:
            canvasColor = DesignTokens.Colors.canvas_evening
        default:
            canvasColor = DesignTokens.Colors.canvas_night
        }
    }
}

// MARK: - AppError

/// Represents an error that can be displayed to the user.
struct AppError: Identifiable, Equatable {
    let id = UUID()
    let title: String
    let message: String
    let isRecoverable: Bool
    let retryAction: (() -> Void)?

    init(title: String, message: String, isRecoverable: Bool = true, retryAction: (() -> Void)? = nil) {
        self.title = title
        self.message = message
        self.isRecoverable = isRecoverable
        self.retryAction = retryAction
    }

    static func == (lhs: AppError, rhs: AppError) -> Bool {
        lhs.id == rhs.id
    }

    // MARK: - Factory Methods

    static func networkError(retryAction: (() -> Void)? = nil) -> AppError {
        AppError(
            title: "Connection Error",
            message: "Unable to connect to the server. Please check your internet connection and try again.",
            isRecoverable: true,
            retryAction: retryAction
        )
    }

    static func authenticationError() -> AppError {
        AppError(
            title: "Authentication Required",
            message: "Your session has expired. Please sign in again.",
            isRecoverable: true,
            retryAction: nil
        )
    }

    static func genericError(message: String) -> AppError {
        AppError(
            title: "Something Went Wrong",
            message: message,
            isRecoverable: true,
            retryAction: nil
        )
    }
}
