//
//  FamilyView.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// Family feature showing household members and management.
struct FamilyView: View {
    // MARK: - State

    @StateObject private var viewModel = FamilyViewModel()
    @EnvironmentObject private var appState: AppState

    @State private var showingInviteSheet = false
    @State private var selectedMember: FamilyMember?

    // MARK: - Body

    var body: some View {
        NavigationStack {
            ScrollView {
                VStack(spacing: DesignTokens.Spacing._6) {
                    // Family Members Section
                    FamilyMembersSection(
                        members: viewModel.members,
                        onMemberTap: { member in
                            selectedMember = member
                        }
                    )

                    // Pending Invitations Section
                    if !viewModel.pendingInvitations.isEmpty {
                        PendingInvitationsSection(
                            invitations: viewModel.pendingInvitations,
                            onRevoke: { invitation in
                                viewModel.revokeInvitation(invitation)
                            }
                        )
                    }

                    // Linked Devices Section
                    LinkedDevicesSection(devices: viewModel.devices)
                }
                .padding(.horizontal, DesignTokens.Spacing._4)
                .padding(.vertical, DesignTokens.Spacing._4)
            }
            .background(appState.canvasColor.ignoresSafeArea())
            .navigationTitle(viewModel.familyName)
            .toolbar {
                ToolbarItem(placement: .topBarTrailing) {
                    Button {
                        showingInviteSheet = true
                    } label: {
                        Image(systemName: "person.badge.plus")
                    }
                }
            }
            .sheet(isPresented: $showingInviteSheet) {
                InviteMemberView()
            }
            .sheet(item: $selectedMember) { member in
                MemberDetailView(member: member)
            }
            .refreshable {
                await viewModel.refresh()
            }
        }
    }
}

// MARK: - Family Members Section

private struct FamilyMembersSection: View {
    let members: [FamilyMember]
    let onMemberTap: (FamilyMember) -> Void

    var body: some View {
        LuminousCard {
            VStack(alignment: .leading, spacing: DesignTokens.Spacing._4) {
                Text("Family Members")
                    .font(.system(size: DesignTokens.FontSize.title_md, weight: .semibold))
                    .foregroundStyle(DesignTokens.Colors.text_primary)

                if members.isEmpty {
                    EmptyStateView(
                        icon: "person.2",
                        title: "No members yet",
                        subtitle: "Invite family members to get started"
                    )
                } else {
                    ForEach(members) { member in
                        MemberRow(member: member)
                            .onTapGesture {
                                onMemberTap(member)
                            }
                    }
                }
            }
        }
    }
}

// MARK: - Member Row

private struct MemberRow: View {
    let member: FamilyMember

    var body: some View {
        HStack(spacing: DesignTokens.Spacing._3) {
            LuminousAvatar(
                member: FamilyMemberSummary(
                    id: member.id,
                    displayName: member.displayName,
                    avatarURL: member.avatarURL,
                    color: member.color
                ),
                size: .medium
            )

            VStack(alignment: .leading, spacing: 2) {
                Text(member.displayName)
                    .font(.system(size: DesignTokens.FontSize.body_md, weight: .medium))
                    .foregroundStyle(DesignTokens.Colors.text_primary)

                Text(member.role.displayName)
                    .font(.system(size: DesignTokens.FontSize.body_sm))
                    .foregroundStyle(DesignTokens.Colors.text_secondary)
            }

            Spacer()

            Image(systemName: "chevron.right")
                .font(.system(size: 14))
                .foregroundStyle(DesignTokens.Colors.text_tertiary)
        }
        .padding(.vertical, DesignTokens.Spacing._2)
    }
}

// MARK: - Pending Invitations Section

private struct PendingInvitationsSection: View {
    let invitations: [FamilyInvitation]
    let onRevoke: (FamilyInvitation) -> Void

    var body: some View {
        LuminousCard {
            VStack(alignment: .leading, spacing: DesignTokens.Spacing._4) {
                Text("Pending Invitations")
                    .font(.system(size: DesignTokens.FontSize.title_md, weight: .semibold))
                    .foregroundStyle(DesignTokens.Colors.text_primary)

                ForEach(invitations) { invitation in
                    HStack {
                        VStack(alignment: .leading, spacing: 2) {
                            Text(invitation.email)
                                .font(.system(size: DesignTokens.FontSize.body_md))
                                .foregroundStyle(DesignTokens.Colors.text_primary)

                            Text("Expires \(invitation.expiresAt, format: .relative(presentation: .named))")
                                .font(.system(size: DesignTokens.FontSize.caption))
                                .foregroundStyle(DesignTokens.Colors.text_tertiary)
                        }

                        Spacer()

                        Button("Revoke") {
                            onRevoke(invitation)
                        }
                        .font(.system(size: DesignTokens.FontSize.body_sm, weight: .medium))
                        .foregroundStyle(DesignTokens.Colors.status_danger)
                    }
                    .padding(.vertical, DesignTokens.Spacing._1)
                }
            }
        }
    }
}

// MARK: - Linked Devices Section

private struct LinkedDevicesSection: View {
    let devices: [LinkedDevice]

    var body: some View {
        LuminousCard {
            VStack(alignment: .leading, spacing: DesignTokens.Spacing._4) {
                Text("Linked Devices")
                    .font(.system(size: DesignTokens.FontSize.title_md, weight: .semibold))
                    .foregroundStyle(DesignTokens.Colors.text_primary)

                if devices.isEmpty {
                    EmptyStateView(
                        icon: "display",
                        title: "No devices linked",
                        subtitle: "Link a display to show your family hub"
                    )
                } else {
                    ForEach(devices) { device in
                        DeviceRow(device: device)
                    }
                }
            }
        }
    }
}

// MARK: - Device Row

private struct DeviceRow: View {
    let device: LinkedDevice

    var body: some View {
        HStack(spacing: DesignTokens.Spacing._3) {
            Image(systemName: device.type.icon)
                .font(.system(size: 24))
                .foregroundStyle(DesignTokens.Colors.accent_500)
                .frame(width: 40, height: 40)

            VStack(alignment: .leading, spacing: 2) {
                Text(device.name)
                    .font(.system(size: DesignTokens.FontSize.body_md, weight: .medium))
                    .foregroundStyle(DesignTokens.Colors.text_primary)

                Text("Last seen \(device.lastSeenAt, format: .relative(presentation: .named))")
                    .font(.system(size: DesignTokens.FontSize.caption))
                    .foregroundStyle(DesignTokens.Colors.text_tertiary)
            }

            Spacer()

            Circle()
                .fill(device.isOnline ? DesignTokens.Colors.status_success : DesignTokens.Colors.text_tertiary)
                .frame(width: 8, height: 8)
        }
        .padding(.vertical, DesignTokens.Spacing._1)
    }
}

// MARK: - Supporting Views

struct InviteMemberView: View {
    @Environment(\.dismiss) private var dismiss

    var body: some View {
        NavigationStack {
            Text("Invite member form - Coming soon")
                .navigationTitle("Invite Member")
                .toolbar {
                    ToolbarItem(placement: .cancellationAction) {
                        Button("Cancel") {
                            dismiss()
                        }
                    }
                }
        }
    }
}

struct MemberDetailView: View {
    let member: FamilyMember
    @Environment(\.dismiss) private var dismiss

    var body: some View {
        NavigationStack {
            Text("Member details - Coming soon")
                .navigationTitle(member.displayName)
                .toolbar {
                    ToolbarItem(placement: .confirmationAction) {
                        Button("Done") {
                            dismiss()
                        }
                    }
                }
        }
    }
}

// MARK: - Models

struct FamilyMember: Identifiable {
    let id: String
    let displayName: String
    let email: String?
    let avatarURL: URL?
    let color: Color
    let role: UserRole

    enum UserRole: String {
        case owner
        case admin
        case adult
        case teen
        case child
        case caregiver

        var displayName: String {
            switch self {
            case .owner: return "Owner"
            case .admin: return "Admin"
            case .adult: return "Adult"
            case .teen: return "Teen"
            case .child: return "Child"
            case .caregiver: return "Caregiver"
            }
        }
    }
}

struct FamilyInvitation: Identifiable {
    let id: String
    let email: String
    let role: FamilyMember.UserRole
    let expiresAt: Date
}

struct LinkedDevice: Identifiable {
    let id: String
    let name: String
    let type: DeviceType
    let lastSeenAt: Date
    let isOnline: Bool

    enum DeviceType: String {
        case display
        case mobile
        case web

        var icon: String {
            switch self {
            case .display: return "display"
            case .mobile: return "iphone"
            case .web: return "globe"
            }
        }
    }
}

// MARK: - Preview

#Preview {
    FamilyView()
        .environmentObject(AppState())
        .environmentObject(AuthViewModel())
}
