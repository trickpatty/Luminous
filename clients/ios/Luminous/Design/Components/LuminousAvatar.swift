//
//  LuminousAvatar.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// An avatar component for displaying family member profiles.
struct LuminousAvatar: View {
    // MARK: - Properties

    let member: FamilyMemberSummary?
    let size: AvatarSize
    let showBorder: Bool

    // MARK: - Initialization

    init(
        member: FamilyMemberSummary?,
        size: AvatarSize = .medium,
        showBorder: Bool = false
    ) {
        self.member = member
        self.size = size
        self.showBorder = showBorder
    }

    // MARK: - Body

    var body: some View {
        Group {
            if let imageURL = member?.avatarURL {
                AsyncImage(url: imageURL) { phase in
                    switch phase {
                    case .empty:
                        initialsView
                    case .success(let image):
                        image
                            .resizable()
                            .aspectRatio(contentMode: .fill)
                    case .failure:
                        initialsView
                    @unknown default:
                        initialsView
                    }
                }
            } else {
                initialsView
            }
        }
        .frame(width: size.dimension, height: size.dimension)
        .clipShape(Circle())
        .overlay {
            if showBorder {
                Circle()
                    .stroke(DesignTokens.Colors.surface_primary, lineWidth: 2)
            }
        }
    }

    // MARK: - Private Views

    private var initialsView: some View {
        ZStack {
            Circle()
                .fill(member?.color ?? DesignTokens.Colors.text_tertiary)

            Text(member?.initials ?? "?")
                .font(.system(size: size.fontSize, weight: .semibold))
                .foregroundStyle(.white)
        }
    }

    // MARK: - Avatar Size

    enum AvatarSize {
        case tiny      // 24pt
        case small     // 32pt
        case medium    // 40pt
        case large     // 56pt
        case xlarge    // 72pt

        var dimension: CGFloat {
            switch self {
            case .tiny: return 24
            case .small: return 32
            case .medium: return 40
            case .large: return 56
            case .xlarge: return 72
            }
        }

        var fontSize: CGFloat {
            switch self {
            case .tiny: return 10
            case .small: return 12
            case .medium: return 14
            case .large: return 20
            case .xlarge: return 28
            }
        }
    }
}

// MARK: - Avatar Group

/// Displays a stack of overlapping avatars.
struct LuminousAvatarGroup: View {
    let members: [FamilyMemberSummary]
    let size: LuminousAvatar.AvatarSize
    let maxDisplay: Int

    init(
        members: [FamilyMemberSummary],
        size: LuminousAvatar.AvatarSize = .small,
        maxDisplay: Int = 4
    ) {
        self.members = members
        self.size = size
        self.maxDisplay = maxDisplay
    }

    var body: some View {
        HStack(spacing: -size.dimension * 0.3) {
            ForEach(Array(members.prefix(maxDisplay).enumerated()), id: \.element.id) { index, member in
                LuminousAvatar(member: member, size: size, showBorder: true)
                    .zIndex(Double(maxDisplay - index))
            }

            if members.count > maxDisplay {
                ZStack {
                    Circle()
                        .fill(DesignTokens.Colors.surface_secondary)

                    Text("+\(members.count - maxDisplay)")
                        .font(.system(size: size.fontSize, weight: .medium))
                        .foregroundStyle(DesignTokens.Colors.text_secondary)
                }
                .frame(width: size.dimension, height: size.dimension)
                .overlay {
                    Circle()
                        .stroke(DesignTokens.Colors.surface_primary, lineWidth: 2)
                }
            }
        }
    }
}

// MARK: - Preview

#Preview {
    VStack(spacing: 24) {
        // Individual avatars
        HStack(spacing: 16) {
            LuminousAvatar(
                member: FamilyMemberSummary(
                    id: "1",
                    displayName: "John Doe",
                    avatarURL: nil,
                    color: DesignTokens.Colors.member_sky
                ),
                size: .tiny
            )
            LuminousAvatar(
                member: FamilyMemberSummary(
                    id: "1",
                    displayName: "John Doe",
                    avatarURL: nil,
                    color: DesignTokens.Colors.member_sky
                ),
                size: .small
            )
            LuminousAvatar(
                member: FamilyMemberSummary(
                    id: "1",
                    displayName: "John Doe",
                    avatarURL: nil,
                    color: DesignTokens.Colors.member_sky
                ),
                size: .medium
            )
            LuminousAvatar(
                member: FamilyMemberSummary(
                    id: "1",
                    displayName: "John Doe",
                    avatarURL: nil,
                    color: DesignTokens.Colors.member_sky
                ),
                size: .large
            )
        }

        // Avatar group
        LuminousAvatarGroup(
            members: [
                FamilyMemberSummary(id: "1", displayName: "John", avatarURL: nil, color: DesignTokens.Colors.member_sky),
                FamilyMemberSummary(id: "2", displayName: "Jane", avatarURL: nil, color: DesignTokens.Colors.member_rose),
                FamilyMemberSummary(id: "3", displayName: "Bob", avatarURL: nil, color: DesignTokens.Colors.member_emerald),
                FamilyMemberSummary(id: "4", displayName: "Alice", avatarURL: nil, color: DesignTokens.Colors.member_amber),
                FamilyMemberSummary(id: "5", displayName: "Charlie", avatarURL: nil, color: DesignTokens.Colors.member_violet),
            ]
        )
    }
    .padding()
}
