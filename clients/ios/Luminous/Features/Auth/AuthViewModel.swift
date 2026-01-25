//
//  AuthViewModel.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI
import AuthenticationServices
import Combine

/// ViewModel managing authentication state and flows.
@MainActor
final class AuthViewModel: ObservableObject {
    // MARK: - Published Properties

    @Published var isAuthenticated: Bool = false
    @Published var currentUser: AuthenticatedUser?
    @Published var isLoading: Bool = false
    @Published var error: Error?

    // MARK: - Dependencies

    private let authService: AuthServiceProtocol
    private let keychainService: KeychainServiceProtocol

    // MARK: - Private Properties

    private var cancellables = Set<AnyCancellable>()

    // MARK: - Initialization

    init(
        authService: AuthServiceProtocol = AuthService.shared,
        keychainService: KeychainServiceProtocol = KeychainService.shared
    ) {
        self.authService = authService
        self.keychainService = keychainService

        // Check for existing session on init
        Task {
            await checkExistingSession()
        }
    }

    // MARK: - Public Methods

    /// Sign in with passkey (WebAuthn).
    func signInWithPasskey() async {
        isLoading = true
        error = nil

        do {
            let result = try await authService.authenticateWithPasskey()
            await handleAuthSuccess(result)
        } catch {
            self.error = error
        }

        isLoading = false
    }

    /// Sign up with passkey (WebAuthn).
    func signUpWithPasskey(email: String) async {
        isLoading = true
        error = nil

        do {
            let result = try await authService.registerWithPasskey(email: email)
            await handleAuthSuccess(result)
        } catch {
            self.error = error
        }

        isLoading = false
    }

    /// Request OTP code for email authentication.
    func requestOTP(email: String) async -> Bool {
        isLoading = true
        error = nil

        do {
            try await authService.requestOTP(email: email)
            isLoading = false
            return true
        } catch {
            self.error = error
            isLoading = false
            return false
        }
    }

    /// Verify OTP code and authenticate.
    func verifyOTP(email: String, code: String) async {
        isLoading = true
        error = nil

        do {
            let result = try await authService.verifyOTP(email: email, code: code)
            await handleAuthSuccess(result)
        } catch {
            self.error = error
        }

        isLoading = false
    }

    /// Sign out the current user.
    func signOut() {
        Task {
            do {
                try await authService.signOut()
                await keychainService.clearTokens()
                currentUser = nil
                isAuthenticated = false
            } catch {
                self.error = error
            }
        }
    }

    /// Refresh the current access token.
    func refreshToken() async -> Bool {
        do {
            guard let refreshToken = await keychainService.getRefreshToken() else {
                return false
            }

            let newTokens = try await authService.refreshToken(refreshToken)
            await keychainService.saveTokens(
                accessToken: newTokens.accessToken,
                refreshToken: newTokens.refreshToken
            )
            return true
        } catch {
            return false
        }
    }

    // MARK: - Private Methods

    private func checkExistingSession() async {
        guard let accessToken = await keychainService.getAccessToken() else {
            isAuthenticated = false
            return
        }

        // Validate the token
        do {
            currentUser = try await authService.validateToken(accessToken)
            isAuthenticated = true
        } catch {
            // Token invalid, try to refresh
            if await refreshToken() {
                await checkExistingSession()
            } else {
                await keychainService.clearTokens()
                isAuthenticated = false
            }
        }
    }

    private func handleAuthSuccess(_ result: AuthResult) async {
        await keychainService.saveTokens(
            accessToken: result.accessToken,
            refreshToken: result.refreshToken
        )
        currentUser = result.user
        isAuthenticated = true
    }
}

// MARK: - Data Models

/// Represents an authenticated user.
struct AuthenticatedUser: Identifiable, Equatable {
    let id: String
    let email: String
    let displayName: String
    let familyId: String
    let role: String
}

/// Result of a successful authentication.
struct AuthResult {
    let accessToken: String
    let refreshToken: String
    let user: AuthenticatedUser
}

/// Token pair for authentication.
struct TokenPair {
    let accessToken: String
    let refreshToken: String
}

// MARK: - Service Protocols

/// Protocol for authentication operations.
protocol AuthServiceProtocol {
    func authenticateWithPasskey() async throws -> AuthResult
    func registerWithPasskey(email: String) async throws -> AuthResult
    func requestOTP(email: String) async throws
    func verifyOTP(email: String, code: String) async throws -> AuthResult
    func signOut() async throws
    func refreshToken(_ token: String) async throws -> TokenPair
    func validateToken(_ token: String) async throws -> AuthenticatedUser
}

/// Protocol for keychain operations.
protocol KeychainServiceProtocol {
    func saveTokens(accessToken: String, refreshToken: String) async
    func getAccessToken() async -> String?
    func getRefreshToken() async -> String?
    func clearTokens() async
}
