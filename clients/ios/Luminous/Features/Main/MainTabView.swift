//
//  MainTabView.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// Main tab navigation for authenticated users.
///
/// Provides access to the 5 core sections:
/// - Home (Dashboard)
/// - Calendar
/// - Tasks
/// - Family
/// - Settings
struct MainTabView: View {
    // MARK: - State

    @State private var selectedTab: Tab = .home

    // MARK: - Body

    var body: some View {
        TabView(selection: $selectedTab) {
            HomeView()
                .tabItem {
                    Label(Tab.home.title, systemImage: Tab.home.icon)
                }
                .tag(Tab.home)

            CalendarView()
                .tabItem {
                    Label(Tab.calendar.title, systemImage: Tab.calendar.icon)
                }
                .tag(Tab.calendar)

            TasksView()
                .tabItem {
                    Label(Tab.tasks.title, systemImage: Tab.tasks.icon)
                }
                .tag(Tab.tasks)

            FamilyView()
                .tabItem {
                    Label(Tab.family.title, systemImage: Tab.family.icon)
                }
                .tag(Tab.family)

            SettingsView()
                .tabItem {
                    Label(Tab.settings.title, systemImage: Tab.settings.icon)
                }
                .tag(Tab.settings)
        }
        .tint(DesignTokens.Colors.accent_600)
    }
}

// MARK: - Tab Enum

extension MainTabView {
    /// Represents the available tabs in the main navigation.
    enum Tab: String, CaseIterable, Identifiable {
        case home
        case calendar
        case tasks
        case family
        case settings

        var id: String { rawValue }

        var title: String {
            switch self {
            case .home: return "Home"
            case .calendar: return "Calendar"
            case .tasks: return "Tasks"
            case .family: return "Family"
            case .settings: return "Settings"
            }
        }

        var icon: String {
            switch self {
            case .home: return "house.fill"
            case .calendar: return "calendar"
            case .tasks: return "checkmark.circle.fill"
            case .family: return "person.2.fill"
            case .settings: return "gearshape.fill"
            }
        }
    }
}

// MARK: - Preview

#Preview {
    MainTabView()
        .environmentObject(AppState())
        .environmentObject(AuthViewModel())
}
