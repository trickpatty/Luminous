//
//  HomeView.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// Home dashboard view displaying family overview and quick actions.
struct HomeView: View {
    // MARK: - State

    @StateObject private var viewModel = HomeViewModel()
    @EnvironmentObject private var appState: AppState

    // MARK: - Body

    var body: some View {
        NavigationStack {
            ScrollView {
                VStack(spacing: DesignTokens.Spacing._6) {
                    // Today's Schedule Summary
                    TodayScheduleCard(events: viewModel.todayEvents)

                    // Quick Actions
                    QuickActionsSection()

                    // Tasks Due Today
                    TasksSummaryCard(tasks: viewModel.tasksDueToday)

                    // Family Activity
                    FamilyActivityCard(activities: viewModel.recentActivities)
                }
                .padding(.horizontal, DesignTokens.Spacing._4)
                .padding(.vertical, DesignTokens.Spacing._4)
            }
            .background(appState.canvasColor.ignoresSafeArea())
            .navigationTitle("Home")
            .toolbar {
                ToolbarItem(placement: .topBarTrailing) {
                    Button {
                        viewModel.refresh()
                    } label: {
                        Image(systemName: "arrow.clockwise")
                    }
                }
            }
            .refreshable {
                await viewModel.refreshAsync()
            }
        }
    }
}

// MARK: - Today Schedule Card

private struct TodayScheduleCard: View {
    let events: [CalendarEventSummary]

    var body: some View {
        LuminousCard {
            VStack(alignment: .leading, spacing: DesignTokens.Spacing._3) {
                HStack {
                    Text("Today's Schedule")
                        .font(.system(size: DesignTokens.FontSize.title_md, weight: .semibold))
                        .foregroundStyle(DesignTokens.Colors.text_primary)

                    Spacer()

                    NavigationLink {
                        CalendarView()
                    } label: {
                        Text("See All")
                            .font(.system(size: DesignTokens.FontSize.body_sm))
                            .foregroundStyle(DesignTokens.Colors.accent_600)
                    }
                }

                if events.isEmpty {
                    EmptyStateView(
                        icon: "calendar.badge.checkmark",
                        title: "All clear!",
                        subtitle: "No events scheduled for today"
                    )
                    .padding(.vertical, DesignTokens.Spacing._4)
                } else {
                    ForEach(events) { event in
                        EventRow(event: event)
                    }
                }
            }
        }
    }
}

// MARK: - Quick Actions Section

private struct QuickActionsSection: View {
    var body: some View {
        VStack(alignment: .leading, spacing: DesignTokens.Spacing._3) {
            Text("Quick Actions")
                .font(.system(size: DesignTokens.FontSize.title_sm, weight: .medium))
                .foregroundStyle(DesignTokens.Colors.text_secondary)
                .padding(.leading, DesignTokens.Spacing._1)

            HStack(spacing: DesignTokens.Spacing._3) {
                QuickActionButton(icon: "plus.circle.fill", title: "Add Event", color: DesignTokens.Colors.accent_500) {
                    // TODO: Navigate to add event
                }

                QuickActionButton(icon: "checkmark.circle.fill", title: "Add Task", color: DesignTokens.Colors.status_success) {
                    // TODO: Navigate to add task
                }

                QuickActionButton(icon: "list.bullet", title: "Grocery List", color: DesignTokens.Colors.member_amber) {
                    // TODO: Navigate to grocery list
                }

                QuickActionButton(icon: "person.badge.plus", title: "Invite", color: DesignTokens.Colors.member_violet) {
                    // TODO: Navigate to invite
                }
            }
        }
    }
}

// MARK: - Quick Action Button

private struct QuickActionButton: View {
    let icon: String
    let title: String
    let color: Color
    let action: () -> Void

    var body: some View {
        Button(action: action) {
            VStack(spacing: DesignTokens.Spacing._2) {
                Image(systemName: icon)
                    .font(.system(size: 24))
                    .foregroundStyle(color)

                Text(title)
                    .font(.system(size: DesignTokens.FontSize.caption, weight: .medium))
                    .foregroundStyle(DesignTokens.Colors.text_secondary)
                    .lineLimit(1)
            }
            .frame(maxWidth: .infinity)
            .padding(.vertical, DesignTokens.Spacing._3)
            .background(DesignTokens.Colors.surface_primary)
            .clipShape(RoundedRectangle(cornerRadius: DesignTokens.Radius.lg))
            .shadow(color: .black.opacity(0.04), radius: 2, y: 1)
        }
        .buttonStyle(.plain)
    }
}

// MARK: - Tasks Summary Card

private struct TasksSummaryCard: View {
    let tasks: [TaskSummary]

    var body: some View {
        LuminousCard {
            VStack(alignment: .leading, spacing: DesignTokens.Spacing._3) {
                HStack {
                    Text("Tasks Due Today")
                        .font(.system(size: DesignTokens.FontSize.title_md, weight: .semibold))
                        .foregroundStyle(DesignTokens.Colors.text_primary)

                    Spacer()

                    if !tasks.isEmpty {
                        Text("\(tasks.filter { $0.isCompleted }.count)/\(tasks.count)")
                            .font(.system(size: DesignTokens.FontSize.body_sm, weight: .medium))
                            .foregroundStyle(DesignTokens.Colors.text_secondary)
                    }
                }

                if tasks.isEmpty {
                    EmptyStateView(
                        icon: "checkmark.circle",
                        title: "All done!",
                        subtitle: "No tasks due today"
                    )
                    .padding(.vertical, DesignTokens.Spacing._4)
                } else {
                    ForEach(tasks) { task in
                        TaskRow(task: task)
                    }
                }
            }
        }
    }
}

// MARK: - Family Activity Card

private struct FamilyActivityCard: View {
    let activities: [FamilyActivity]

    var body: some View {
        LuminousCard {
            VStack(alignment: .leading, spacing: DesignTokens.Spacing._3) {
                Text("Recent Activity")
                    .font(.system(size: DesignTokens.FontSize.title_md, weight: .semibold))
                    .foregroundStyle(DesignTokens.Colors.text_primary)

                if activities.isEmpty {
                    EmptyStateView(
                        icon: "person.2",
                        title: "No recent activity",
                        subtitle: "Family activity will appear here"
                    )
                    .padding(.vertical, DesignTokens.Spacing._4)
                } else {
                    ForEach(activities) { activity in
                        ActivityRow(activity: activity)
                    }
                }
            }
        }
    }
}

// MARK: - Row Components

private struct EventRow: View {
    let event: CalendarEventSummary

    var body: some View {
        HStack(spacing: DesignTokens.Spacing._3) {
            RoundedRectangle(cornerRadius: 2)
                .fill(event.memberColor)
                .frame(width: 4)

            VStack(alignment: .leading, spacing: 2) {
                Text(event.title)
                    .font(.system(size: DesignTokens.FontSize.body_md, weight: .medium))
                    .foregroundStyle(DesignTokens.Colors.text_primary)

                Text(event.timeString)
                    .font(.system(size: DesignTokens.FontSize.body_sm))
                    .foregroundStyle(DesignTokens.Colors.text_secondary)
            }

            Spacer()
        }
        .padding(.vertical, DesignTokens.Spacing._2)
    }
}

private struct TaskRow: View {
    let task: TaskSummary

    var body: some View {
        HStack(spacing: DesignTokens.Spacing._3) {
            Image(systemName: task.isCompleted ? "checkmark.circle.fill" : "circle")
                .foregroundStyle(task.isCompleted ? DesignTokens.Colors.status_success : DesignTokens.Colors.text_tertiary)
                .font(.system(size: 20))

            Text(task.title)
                .font(.system(size: DesignTokens.FontSize.body_md))
                .foregroundStyle(task.isCompleted ? DesignTokens.Colors.text_tertiary : DesignTokens.Colors.text_primary)
                .strikethrough(task.isCompleted)

            Spacer()

            LuminousAvatar(member: task.assignee, size: .small)
        }
        .padding(.vertical, DesignTokens.Spacing._2)
    }
}

private struct ActivityRow: View {
    let activity: FamilyActivity

    var body: some View {
        HStack(spacing: DesignTokens.Spacing._3) {
            LuminousAvatar(member: activity.member, size: .small)

            VStack(alignment: .leading, spacing: 2) {
                Text(activity.description)
                    .font(.system(size: DesignTokens.FontSize.body_sm))
                    .foregroundStyle(DesignTokens.Colors.text_primary)

                Text(activity.timeAgo)
                    .font(.system(size: DesignTokens.FontSize.caption))
                    .foregroundStyle(DesignTokens.Colors.text_tertiary)
            }

            Spacer()
        }
        .padding(.vertical, DesignTokens.Spacing._1)
    }
}

// MARK: - Preview

#Preview {
    HomeView()
        .environmentObject(AppState())
        .environmentObject(AuthViewModel())
}
