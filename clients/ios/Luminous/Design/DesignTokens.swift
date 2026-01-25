//
// DesignTokens.swift
// Luminous Design System
//
// Generated from design-tokens/tokens.json
// Do not edit directly - regenerate using Style Dictionary
//

import SwiftUI

// MARK: - Color Hex Extension

public extension Color {
    init(hex: String) {
        let hex = hex.trimmingCharacters(in: CharacterSet.alphanumerics.inverted)
        var int: UInt64 = 0
        Scanner(string: hex).scanHexInt64(&int)
        let a, r, g, b: UInt64
        switch hex.count {
        case 3: // RGB (12-bit)
            (a, r, g, b) = (255, (int >> 8) * 17, (int >> 4 & 0xF) * 17, (int & 0xF) * 17)
        case 6: // RGB (24-bit)
            (a, r, g, b) = (255, int >> 16, int >> 8 & 0xFF, int & 0xFF)
        case 8: // ARGB (32-bit)
            (a, r, g, b) = (int >> 24, int >> 16 & 0xFF, int >> 8 & 0xFF, int & 0xFF)
        default:
            (a, r, g, b) = (255, 0, 0, 0)
        }
        self.init(
            .sRGB,
            red: Double(r) / 255,
            green: Double(g) / 255,
            blue: Double(b) / 255,
            opacity: Double(a) / 255
        )
    }
}

// MARK: - Design Tokens

public enum DesignTokens {

    // MARK: - Colors

    public enum Colors {
        // Canvas (time-adaptive backgrounds)
        public static let canvas_dawn = Color(hex: "FFFCF7")
        public static let canvas_morning = Color(hex: "FEFDFB")
        public static let canvas_afternoon = Color(hex: "FDFCFA")
        public static let canvas_evening = Color(hex: "FDF9F3")
        public static let canvas_night = Color(hex: "FAF7F2")

        // Surface
        public static let surface_primary = Color(hex: "FFFFFF")
        public static let surface_secondary = Color(hex: "FAFAFA")
        public static let surface_interactive = Color(hex: "F9FAFB")
        public static let surface_pressed = Color(hex: "F3F4F6")

        // Text
        public static let text_primary = Color(hex: "111827")
        public static let text_secondary = Color(hex: "6B7280")
        public static let text_tertiary = Color(hex: "9CA3AF")
        public static let text_inverse = Color(hex: "FFFFFF")

        // Border
        public static let border_default = Color(hex: "E5E7EB")
        public static let border_light = Color(hex: "F3F4F6")
        public static let border_strong = Color(hex: "D1D5DB")

        // Accent
        public static let accent_50 = Color(hex: "F0F9FF")
        public static let accent_100 = Color(hex: "E0F2FE")
        public static let accent_200 = Color(hex: "BAE6FD")
        public static let accent_300 = Color(hex: "7DD3FC")
        public static let accent_400 = Color(hex: "38BDF8")
        public static let accent_500 = Color(hex: "0EA5E9")
        public static let accent_600 = Color(hex: "0284C7")
        public static let accent_700 = Color(hex: "0369A1")
        public static let accent_800 = Color(hex: "075985")
        public static let accent_900 = Color(hex: "0C4A6E")

        // Status
        public static let status_success = Color(hex: "16A34A")
        public static let status_success_light = Color(hex: "DCFCE7")
        public static let status_success_dark = Color(hex: "166534")
        public static let status_warning = Color(hex: "D97706")
        public static let status_warning_light = Color(hex: "FEF3C7")
        public static let status_warning_dark = Color(hex: "B45309")
        public static let status_danger = Color(hex: "DC2626")
        public static let status_danger_light = Color(hex: "FEE2E2")
        public static let status_danger_dark = Color(hex: "B91C1C")
        public static let status_info = Color(hex: "0284C7")
        public static let status_info_light = Color(hex: "E0F2FE")
        public static let status_info_dark = Color(hex: "075985")

        // Member colors (for family member avatars/badges)
        public static let member_sky = Color(hex: "0EA5E9")
        public static let member_emerald = Color(hex: "10B981")
        public static let member_amber = Color(hex: "F59E0B")
        public static let member_orange = Color(hex: "F97316")
        public static let member_rose = Color(hex: "F43F5E")
        public static let member_violet = Color(hex: "8B5CF6")
        public static let member_pink = Color(hex: "EC4899")
        public static let member_teal = Color(hex: "14B8A6")

        // Night display (dark mode for display app)
        public static let night_display_canvas = Color(hex: "18181B")
        public static let night_display_surface_primary = Color(hex: "27272A")
        public static let night_display_surface_secondary = Color(hex: "1F1F23")
        public static let night_display_text_primary = Color(hex: "FAFAFA")
        public static let night_display_text_secondary = Color(hex: "A1A1AA")
        public static let night_display_border = Color(hex: "3F3F46")

        // Array of member colors for assignment
        public static let memberColors: [Color] = [
            member_sky,
            member_emerald,
            member_amber,
            member_orange,
            member_rose,
            member_violet,
            member_pink,
            member_teal
        ]
    }

    // MARK: - Spacing

    public enum Spacing {
        public static let _0: CGFloat = 0
        public static let _1: CGFloat = 4
        public static let _2: CGFloat = 8
        public static let _3: CGFloat = 12
        public static let _4: CGFloat = 16
        public static let _5: CGFloat = 20
        public static let _6: CGFloat = 24
        public static let _8: CGFloat = 32
        public static let _10: CGFloat = 40
        public static let _12: CGFloat = 48
        public static let _16: CGFloat = 64
        public static let _20: CGFloat = 80
    }

    // MARK: - Border Radius

    public enum Radius {
        public static let sm: CGFloat = 6
        public static let md: CGFloat = 8
        public static let lg: CGFloat = 12
        public static let xl: CGFloat = 16
        public static let xxl: CGFloat = 24
        public static let full: CGFloat = 9999
    }

    // MARK: - Touch Targets

    public enum Touch {
        public static let min: CGFloat = 44
        public static let md: CGFloat = 48
        public static let lg: CGFloat = 56
        public static let xl: CGFloat = 64
    }

    // MARK: - Font Sizes

    public enum FontSize {
        // Display sizes (large headers)
        public static let display_xl: CGFloat = 72
        public static let display_lg: CGFloat = 56
        public static let display_md: CGFloat = 40
        public static let display_sm: CGFloat = 32

        // Title sizes
        public static let title_lg: CGFloat = 24
        public static let title_md: CGFloat = 20
        public static let title_sm: CGFloat = 18

        // Body sizes
        public static let body_lg: CGFloat = 18
        public static let body_md: CGFloat = 16
        public static let body_sm: CGFloat = 14

        // Small sizes
        public static let caption: CGFloat = 12
        public static let overline: CGFloat = 11
    }

    // MARK: - Font Weights

    public enum FontWeight {
        public static let regular = Font.Weight.regular
        public static let medium = Font.Weight.medium
        public static let semibold = Font.Weight.semibold
        public static let bold = Font.Weight.bold
    }

    // MARK: - Animation Durations

    public enum Duration {
        public static let instant: Double = 0
        public static let quick: Double = 0.1
        public static let standard: Double = 0.2
        public static let moderate: Double = 0.3
        public static let slow: Double = 0.4
        public static let deliberate: Double = 0.6
    }

    // MARK: - Shadows

    public enum Shadow {
        public static func xs(_ color: Color = .black.opacity(0.04)) -> some View {
            return EmptyView()
                .shadow(color: color, radius: 1, x: 0, y: 1)
        }

        public static func sm(_ color: Color = .black.opacity(0.06)) -> some View {
            return EmptyView()
                .shadow(color: color, radius: 3, x: 0, y: 1)
        }

        public static func md(_ color: Color = .black.opacity(0.05)) -> some View {
            return EmptyView()
                .shadow(color: color, radius: 6, x: 0, y: 4)
        }

        public static func lg(_ color: Color = .black.opacity(0.06)) -> some View {
            return EmptyView()
                .shadow(color: color, radius: 15, x: 0, y: 10)
        }
    }
}

// MARK: - View Extensions

public extension View {
    /// Apply a standard card shadow
    func cardShadow() -> some View {
        self.shadow(color: .black.opacity(0.04), radius: 2, x: 0, y: 1)
            .shadow(color: .black.opacity(0.06), radius: 3, x: 0, y: 1)
    }

    /// Apply a large shadow for modals/dialogs
    func modalShadow() -> some View {
        self.shadow(color: .black.opacity(0.08), radius: 25, x: 0, y: 20)
            .shadow(color: .black.opacity(0.04), radius: 10, x: 0, y: 8)
    }
}
