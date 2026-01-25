//
//  FamilyViewModel.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI
import Combine

/// ViewModel for the Family feature.
@MainActor
final class FamilyViewModel: ObservableObject {
    // MARK: - Published Properties

    @Published var familyName: String = "My Family"
    @Published var members: [FamilyMember] = []
    @Published var pendingInvitations: [FamilyInvitation] = []
    @Published var devices: [LinkedDevice] = []
    @Published var isLoading: Bool = false
    @Published var error: Error?

    // MARK: - Private Properties

    private var cancellables = Set<AnyCancellable>()

    // MARK: - Initialization

    init() {
        Task {
            await loadFamilyData()
        }
    }

    // MARK: - Public Methods

    /// Refresh all family data.
    func refresh() async {
        await loadFamilyData()
    }

    /// Revoke a pending invitation.
    func revokeInvitation(_ invitation: FamilyInvitation) {
        pendingInvitations.removeAll { $0.id == invitation.id }

        // TODO: Sync with API
    }

    // MARK: - Private Methods

    private func loadFamilyData() async {
        isLoading = true
        error = nil

        do {
            async let familyInfo = fetchFamilyInfo()
            async let membersData = fetchMembers()
            async let invitationsData = fetchInvitations()
            async let devicesData = fetchDevices()

            let (family, loadedMembers, loadedInvitations, loadedDevices) = try await (
                familyInfo,
                membersData,
                invitationsData,
                devicesData
            )

            familyName = family.name
            members = loadedMembers
            pendingInvitations = loadedInvitations
            devices = loadedDevices
        } catch {
            self.error = error
        }

        isLoading = false
    }

    private func fetchFamilyInfo() async throws -> (name: String, id: String) {
        // TODO: Implement with actual API call
        return (name: "My Family", id: "family-1")
    }

    private func fetchMembers() async throws -> [FamilyMember] {
        // TODO: Implement with actual API call
        return []
    }

    private func fetchInvitations() async throws -> [FamilyInvitation] {
        // TODO: Implement with actual API call
        return []
    }

    private func fetchDevices() async throws -> [LinkedDevice] {
        // TODO: Implement with actual API call
        return []
    }
}
