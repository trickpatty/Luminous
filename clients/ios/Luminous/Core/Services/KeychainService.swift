//
//  KeychainService.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import Foundation
import Security

/// Service for secure storage of sensitive data in the iOS Keychain.
actor KeychainService: KeychainServiceProtocol {
    // MARK: - Singleton

    static let shared = KeychainService()

    // MARK: - Constants

    private enum Keys {
        static let accessToken = "com.luminous.accessToken"
        static let refreshToken = "com.luminous.refreshToken"
        static let deviceId = "com.luminous.deviceId"
    }

    // MARK: - Private Properties

    private let service = AppConfiguration.bundleIdentifier

    // MARK: - Public Methods

    /// Save access and refresh tokens.
    func saveTokens(accessToken: String, refreshToken: String) {
        save(key: Keys.accessToken, value: accessToken)
        save(key: Keys.refreshToken, value: refreshToken)
    }

    /// Get the stored access token.
    func getAccessToken() -> String? {
        get(key: Keys.accessToken)
    }

    /// Get the stored refresh token.
    func getRefreshToken() -> String? {
        get(key: Keys.refreshToken)
    }

    /// Clear all stored tokens.
    func clearTokens() {
        delete(key: Keys.accessToken)
        delete(key: Keys.refreshToken)
    }

    /// Get or create a unique device identifier.
    func getOrCreateDeviceId() -> String {
        if let existingId = get(key: Keys.deviceId) {
            return existingId
        }

        let newId = UUID().uuidString
        save(key: Keys.deviceId, value: newId)
        return newId
    }

    // MARK: - Private Methods

    private func save(key: String, value: String) {
        guard let data = value.data(using: .utf8) else { return }

        // Delete any existing item first
        delete(key: key)

        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: service,
            kSecAttrAccount as String: key,
            kSecValueData as String: data,
            kSecAttrAccessible as String: kSecAttrAccessibleAfterFirstUnlockThisDeviceOnly
        ]

        let status = SecItemAdd(query as CFDictionary, nil)

        if status != errSecSuccess && AppConfiguration.isLoggingEnabled {
            print("Keychain save error for \(key): \(status)")
        }
    }

    private func get(key: String) -> String? {
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: service,
            kSecAttrAccount as String: key,
            kSecReturnData as String: true,
            kSecMatchLimit as String: kSecMatchLimitOne
        ]

        var result: AnyObject?
        let status = SecItemCopyMatching(query as CFDictionary, &result)

        guard status == errSecSuccess,
              let data = result as? Data,
              let value = String(data: data, encoding: .utf8) else {
            return nil
        }

        return value
    }

    private func delete(key: String) {
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: service,
            kSecAttrAccount as String: key
        ]

        SecItemDelete(query as CFDictionary)
    }

    private func update(key: String, value: String) {
        guard let data = value.data(using: .utf8) else { return }

        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: service,
            kSecAttrAccount as String: key
        ]

        let attributes: [String: Any] = [
            kSecValueData as String: data
        ]

        let status = SecItemUpdate(query as CFDictionary, attributes as CFDictionary)

        if status != errSecSuccess && AppConfiguration.isLoggingEnabled {
            print("Keychain update error for \(key): \(status)")
        }
    }
}
