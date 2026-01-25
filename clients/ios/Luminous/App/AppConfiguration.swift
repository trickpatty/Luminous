//
//  AppConfiguration.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import Foundation

/// Application configuration loaded from environment and build settings.
///
/// Provides access to API URLs, feature flags, and other configuration values
/// that may vary between debug, staging, and production builds.
enum AppConfiguration {
    // MARK: - Environment

    /// The current build environment.
    enum Environment: String {
        case debug
        case staging
        case production

        var name: String {
            switch self {
            case .debug: return "Debug"
            case .staging: return "Staging"
            case .production: return "Production"
            }
        }
    }

    /// Current build environment.
    static var environment: Environment {
        #if DEBUG
        return .debug
        #else
        // Check for staging flag in Info.plist or environment
        if let env = Bundle.main.infoDictionary?["APP_ENVIRONMENT"] as? String,
           env.lowercased() == "staging" {
            return .staging
        }
        return .production
        #endif
    }

    // MARK: - API Configuration

    /// Base URL for the Luminous API.
    static var apiBaseURL: URL {
        switch environment {
        case .debug:
            // Local development server
            return URL(string: "http://localhost:5000")!
        case .staging:
            return URL(string: "https://api-staging.luminous.app")!
        case .production:
            return URL(string: "https://api.luminous.app")!
        }
    }

    /// URL for the SignalR real-time sync hub.
    static var signalRURL: URL {
        apiBaseURL.appendingPathComponent("hubs/sync")
    }

    /// API request timeout in seconds.
    static var apiTimeout: TimeInterval {
        switch environment {
        case .debug: return 30
        default: return 15
        }
    }

    // MARK: - Authentication

    /// WebAuthn Relying Party ID.
    static var webAuthnRelyingPartyID: String {
        switch environment {
        case .debug:
            return "localhost"
        case .staging:
            return "staging.luminous.app"
        case .production:
            return "luminous.app"
        }
    }

    /// Whether to allow non-HTTPS connections (debug only).
    static var allowInsecureConnections: Bool {
        environment == .debug
    }

    // MARK: - Feature Flags

    /// Whether to enable verbose logging.
    static var isLoggingEnabled: Bool {
        environment != .production
    }

    /// Whether to show debug overlays and tools.
    static var showDebugTools: Bool {
        environment == .debug
    }

    /// Whether offline mode is enabled.
    static var isOfflineModeEnabled: Bool {
        true // Always enabled
    }

    // MARK: - App Info

    /// App version string from Info.plist.
    static var appVersion: String {
        Bundle.main.infoDictionary?["CFBundleShortVersionString"] as? String ?? "1.0.0"
    }

    /// Build number from Info.plist.
    static var buildNumber: String {
        Bundle.main.infoDictionary?["CFBundleVersion"] as? String ?? "1"
    }

    /// Full version string (e.g., "1.0.0 (123)").
    static var fullVersion: String {
        "\(appVersion) (\(buildNumber))"
    }

    /// Bundle identifier.
    static var bundleIdentifier: String {
        Bundle.main.bundleIdentifier ?? "com.luminous.app"
    }

    // MARK: - Cache Configuration

    /// Maximum age for cached data in seconds.
    static var cacheMaxAge: TimeInterval {
        switch environment {
        case .debug: return 60 // 1 minute for testing
        default: return 3600 // 1 hour
        }
    }

    /// Maximum size for local cache in bytes.
    static var cacheMaxSize: Int {
        50 * 1024 * 1024 // 50 MB
    }

    // MARK: - Push Notifications

    /// Whether push notifications are enabled for this environment.
    static var isPushEnabled: Bool {
        environment != .debug
    }
}
