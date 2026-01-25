//
//  AuthService.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import Foundation
import AuthenticationServices

/// Service handling authentication operations including passkeys and OTP.
final class AuthService: AuthServiceProtocol {
    // MARK: - Singleton

    static let shared = AuthService()

    // MARK: - Dependencies

    private let api: APIService
    private let keychain: KeychainService

    // MARK: - Initialization

    private init(api: APIService = .shared, keychain: KeychainService = .shared) {
        self.api = api
        self.keychain = keychain
    }

    // MARK: - Passkey Authentication

    /// Authenticate using a passkey.
    func authenticateWithPasskey() async throws -> AuthResult {
        // 1. Start authentication challenge
        let challengeResponse: PasskeyAuthChallengeResponse = try await api.post("/api/auth/passkey/authenticate/start")

        // 2. Perform WebAuthn authentication
        let credential = try await performPasskeyAuthentication(challenge: challengeResponse.challenge)

        // 3. Complete authentication with server
        let completeRequest = PasskeyAuthCompleteRequest(
            credentialId: credential.credentialID.base64URLEncodedString(),
            authenticatorData: credential.rawAuthenticatorData.base64URLEncodedString(),
            clientDataJSON: credential.rawClientDataJSON.base64URLEncodedString(),
            signature: credential.signature.base64URLEncodedString(),
            userHandle: credential.userID.base64URLEncodedString(),
            sessionId: challengeResponse.sessionId
        )

        let authResponse: AuthResponse = try await api.post("/api/auth/passkey/authenticate/complete", body: completeRequest)

        return AuthResult(
            accessToken: authResponse.accessToken,
            refreshToken: authResponse.refreshToken,
            user: authResponse.user.toAuthenticatedUser()
        )
    }

    /// Register a new passkey for the user.
    func registerWithPasskey(email: String) async throws -> AuthResult {
        // 1. Start registration challenge
        let challengeRequest = PasskeyRegisterStartRequest(email: email)
        let challengeResponse: PasskeyRegisterChallengeResponse = try await api.post("/api/auth/passkey/register/start", body: challengeRequest)

        // 2. Perform WebAuthn registration
        let credential = try await performPasskeyRegistration(
            challenge: challengeResponse.challenge,
            userID: challengeResponse.userId,
            userName: email
        )

        // 3. Complete registration with server
        let completeRequest = PasskeyRegisterCompleteRequest(
            credentialId: credential.credentialID.base64URLEncodedString(),
            attestationObject: credential.rawAttestationObject?.base64URLEncodedString() ?? "",
            clientDataJSON: credential.rawClientDataJSON.base64URLEncodedString(),
            sessionId: challengeResponse.sessionId
        )

        let authResponse: AuthResponse = try await api.post("/api/auth/passkey/register/complete", body: completeRequest)

        return AuthResult(
            accessToken: authResponse.accessToken,
            refreshToken: authResponse.refreshToken,
            user: authResponse.user.toAuthenticatedUser()
        )
    }

    // MARK: - OTP Authentication

    /// Request an OTP code to be sent to the email.
    func requestOTP(email: String) async throws {
        let request = OTPRequestBody(email: email)
        let _: EmptyResponse = try await api.post("/api/auth/otp/request", body: request)
    }

    /// Verify the OTP code and authenticate.
    func verifyOTP(email: String, code: String) async throws -> AuthResult {
        let request = OTPVerifyBody(email: email, code: code)
        let response: AuthResponse = try await api.post("/api/auth/otp/verify", body: request)

        return AuthResult(
            accessToken: response.accessToken,
            refreshToken: response.refreshToken,
            user: response.user.toAuthenticatedUser()
        )
    }

    // MARK: - Session Management

    /// Sign out the current user.
    func signOut() async throws {
        // Notify server to invalidate the refresh token
        try? await api.post("/api/auth/logout") as EmptyResponse
    }

    /// Refresh the access token using a refresh token.
    func refreshToken(_ token: String) async throws -> TokenPair {
        let request = RefreshTokenBody(refreshToken: token)
        let response: TokenResponse = try await api.post("/api/auth/refresh", body: request)

        return TokenPair(
            accessToken: response.accessToken,
            refreshToken: response.refreshToken
        )
    }

    /// Validate an access token and return user info.
    func validateToken(_ token: String) async throws -> AuthenticatedUser {
        api.setAccessToken(token)
        let response: UserResponse = try await api.get("/api/auth/me")
        return response.toAuthenticatedUser()
    }

    // MARK: - Private WebAuthn Methods

    private func performPasskeyAuthentication(challenge: String) async throws -> ASAuthorizationPlatformPublicKeyCredentialAssertion {
        let challengeData = Data(base64URLEncoded: challenge)!

        let provider = ASAuthorizationPlatformPublicKeyCredentialProvider(relyingPartyIdentifier: AppConfiguration.webAuthnRelyingPartyID)
        let request = provider.createCredentialAssertionRequest(challenge: challengeData)

        return try await withCheckedThrowingContinuation { continuation in
            let controller = ASAuthorizationController(authorizationRequests: [request])
            let delegate = PasskeyDelegate(continuation: continuation)
            controller.delegate = delegate
            controller.presentationContextProvider = delegate
            controller.performRequests()

            // Keep delegate alive
            objc_setAssociatedObject(controller, "delegate", delegate, .OBJC_ASSOCIATION_RETAIN)
        }
    }

    private func performPasskeyRegistration(challenge: String, userID: String, userName: String) async throws -> ASAuthorizationPlatformPublicKeyCredentialRegistration {
        let challengeData = Data(base64URLEncoded: challenge)!
        let userIDData = Data(base64URLEncoded: userID)!

        let provider = ASAuthorizationPlatformPublicKeyCredentialProvider(relyingPartyIdentifier: AppConfiguration.webAuthnRelyingPartyID)
        let request = provider.createCredentialRegistrationRequest(
            challenge: challengeData,
            name: userName,
            userID: userIDData
        )

        return try await withCheckedThrowingContinuation { continuation in
            let controller = ASAuthorizationController(authorizationRequests: [request])
            let delegate = PasskeyRegistrationDelegate(continuation: continuation)
            controller.delegate = delegate
            controller.presentationContextProvider = delegate
            controller.performRequests()

            // Keep delegate alive
            objc_setAssociatedObject(controller, "delegate", delegate, .OBJC_ASSOCIATION_RETAIN)
        }
    }
}

// MARK: - Passkey Delegates

private class PasskeyDelegate: NSObject, ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding {
    private let continuation: CheckedContinuation<ASAuthorizationPlatformPublicKeyCredentialAssertion, Error>

    init(continuation: CheckedContinuation<ASAuthorizationPlatformPublicKeyCredentialAssertion, Error>) {
        self.continuation = continuation
    }

    func authorizationController(controller: ASAuthorizationController, didCompleteWithAuthorization authorization: ASAuthorization) {
        if let credential = authorization.credential as? ASAuthorizationPlatformPublicKeyCredentialAssertion {
            continuation.resume(returning: credential)
        } else {
            continuation.resume(throwing: AuthError.invalidCredentialType)
        }
    }

    func authorizationController(controller: ASAuthorizationController, didCompleteWithError error: Error) {
        continuation.resume(throwing: error)
    }

    func presentationAnchor(for controller: ASAuthorizationController) -> ASPresentationAnchor {
        ASPresentationAnchor()
    }
}

private class PasskeyRegistrationDelegate: NSObject, ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding {
    private let continuation: CheckedContinuation<ASAuthorizationPlatformPublicKeyCredentialRegistration, Error>

    init(continuation: CheckedContinuation<ASAuthorizationPlatformPublicKeyCredentialRegistration, Error>) {
        self.continuation = continuation
    }

    func authorizationController(controller: ASAuthorizationController, didCompleteWithAuthorization authorization: ASAuthorization) {
        if let credential = authorization.credential as? ASAuthorizationPlatformPublicKeyCredentialRegistration {
            continuation.resume(returning: credential)
        } else {
            continuation.resume(throwing: AuthError.invalidCredentialType)
        }
    }

    func authorizationController(controller: ASAuthorizationController, didCompleteWithError error: Error) {
        continuation.resume(throwing: error)
    }

    func presentationAnchor(for controller: ASAuthorizationController) -> ASPresentationAnchor {
        ASPresentationAnchor()
    }
}

// MARK: - Auth Error

enum AuthError: LocalizedError {
    case invalidCredentialType
    case challengeFailed
    case registrationFailed

    var errorDescription: String? {
        switch self {
        case .invalidCredentialType:
            return "Invalid credential type received"
        case .challengeFailed:
            return "Authentication challenge failed"
        case .registrationFailed:
            return "Registration failed"
        }
    }
}

// MARK: - Request/Response Types

private struct PasskeyAuthChallengeResponse: Decodable {
    let challenge: String
    let sessionId: String
}

private struct PasskeyAuthCompleteRequest: Encodable {
    let credentialId: String
    let authenticatorData: String
    let clientDataJSON: String
    let signature: String
    let userHandle: String
    let sessionId: String
}

private struct PasskeyRegisterStartRequest: Encodable {
    let email: String
}

private struct PasskeyRegisterChallengeResponse: Decodable {
    let challenge: String
    let userId: String
    let sessionId: String
}

private struct PasskeyRegisterCompleteRequest: Encodable {
    let credentialId: String
    let attestationObject: String
    let clientDataJSON: String
    let sessionId: String
}

private struct OTPRequestBody: Encodable {
    let email: String
}

private struct OTPVerifyBody: Encodable {
    let email: String
    let code: String
}

private struct RefreshTokenBody: Encodable {
    let refreshToken: String
}

private struct AuthResponse: Decodable {
    let accessToken: String
    let refreshToken: String
    let user: UserResponse
}

private struct TokenResponse: Decodable {
    let accessToken: String
    let refreshToken: String
}

private struct UserResponse: Decodable {
    let id: String
    let email: String
    let displayName: String
    let familyId: String
    let role: String

    func toAuthenticatedUser() -> AuthenticatedUser {
        AuthenticatedUser(
            id: id,
            email: email,
            displayName: displayName,
            familyId: familyId,
            role: role
        )
    }
}

// MARK: - Data Extensions

extension Data {
    init?(base64URLEncoded string: String) {
        var base64 = string
            .replacingOccurrences(of: "-", with: "+")
            .replacingOccurrences(of: "_", with: "/")

        while base64.count % 4 != 0 {
            base64.append("=")
        }

        self.init(base64Encoded: base64)
    }

    func base64URLEncodedString() -> String {
        base64EncodedString()
            .replacingOccurrences(of: "+", with: "-")
            .replacingOccurrences(of: "/", with: "_")
            .replacingOccurrences(of: "=", with: "")
    }
}
