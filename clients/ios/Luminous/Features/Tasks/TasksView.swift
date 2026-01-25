//
//  TasksView.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// Tasks feature showing family chores and to-dos.
struct TasksView: View {
    // MARK: - State

    @StateObject private var viewModel = TasksViewModel()
    @EnvironmentObject private var appState: AppState

    @State private var selectedFilter: TaskFilter = .all
    @State private var showingTaskCreation = false

    // MARK: - Body

    var body: some View {
        NavigationStack {
            VStack(spacing: 0) {
                // Filter chips
                ScrollView(.horizontal, showsIndicators: false) {
                    HStack(spacing: DesignTokens.Spacing._2) {
                        ForEach(TaskFilter.allCases) { filter in
                            FilterChip(
                                title: filter.title,
                                isSelected: selectedFilter == filter
                            ) {
                                selectedFilter = filter
                            }
                        }
                    }
                    .padding(.horizontal, DesignTokens.Spacing._4)
                    .padding(.vertical, DesignTokens.Spacing._2)
                }

                Divider()

                // Task list
                if viewModel.filteredTasks(for: selectedFilter).isEmpty {
                    EmptyStateView(
                        icon: "checkmark.circle",
                        title: selectedFilter == .completed ? "No completed tasks" : "No tasks",
                        subtitle: selectedFilter == .all ? "Create your first task to get started" : "Tasks matching this filter will appear here"
                    )
                } else {
                    List {
                        ForEach(viewModel.filteredTasks(for: selectedFilter)) { task in
                            TaskRowView(task: task) {
                                viewModel.toggleTaskCompletion(task)
                            }
                        }
                        .onDelete { indexSet in
                            viewModel.deleteTasks(at: indexSet, filter: selectedFilter)
                        }
                    }
                    .listStyle(.plain)
                }
            }
            .background(appState.canvasColor.ignoresSafeArea())
            .navigationTitle("Tasks")
            .toolbar {
                ToolbarItem(placement: .topBarTrailing) {
                    Button {
                        showingTaskCreation = true
                    } label: {
                        Image(systemName: "plus")
                    }
                }
            }
            .sheet(isPresented: $showingTaskCreation) {
                TaskCreationView()
            }
            .refreshable {
                await viewModel.refresh()
            }
        }
    }
}

// MARK: - Task Filter

enum TaskFilter: String, CaseIterable, Identifiable {
    case all
    case today
    case overdue
    case completed

    var id: String { rawValue }

    var title: String {
        switch self {
        case .all: return "All"
        case .today: return "Today"
        case .overdue: return "Overdue"
        case .completed: return "Completed"
        }
    }
}

// MARK: - Filter Chip

private struct FilterChip: View {
    let title: String
    let isSelected: Bool
    let action: () -> Void

    var body: some View {
        Button(action: action) {
            Text(title)
                .font(.system(size: DesignTokens.FontSize.body_sm, weight: .medium))
                .foregroundStyle(isSelected ? .white : DesignTokens.Colors.text_primary)
                .padding(.horizontal, DesignTokens.Spacing._4)
                .padding(.vertical, DesignTokens.Spacing._2)
                .background(isSelected ? DesignTokens.Colors.accent_600 : DesignTokens.Colors.surface_secondary)
                .clipShape(Capsule())
        }
        .buttonStyle(.plain)
    }
}

// MARK: - Task Row View

private struct TaskRowView: View {
    let task: TaskItem
    let onToggle: () -> Void

    var body: some View {
        HStack(spacing: DesignTokens.Spacing._3) {
            Button(action: onToggle) {
                Image(systemName: task.isCompleted ? "checkmark.circle.fill" : "circle")
                    .font(.system(size: 24))
                    .foregroundStyle(task.isCompleted ? DesignTokens.Colors.status_success : task.assigneeColor)
            }
            .buttonStyle(.plain)

            VStack(alignment: .leading, spacing: 2) {
                Text(task.title)
                    .font(.system(size: DesignTokens.FontSize.body_md))
                    .strikethrough(task.isCompleted)
                    .foregroundStyle(task.isCompleted ? DesignTokens.Colors.text_tertiary : DesignTokens.Colors.text_primary)

                if let dueDate = task.dueDate {
                    Text(dueDate, format: .dateTime.month().day().hour().minute())
                        .font(.system(size: DesignTokens.FontSize.caption))
                        .foregroundStyle(task.isOverdue ? DesignTokens.Colors.status_danger : DesignTokens.Colors.text_secondary)
                }
            }

            Spacer()

            if let assignee = task.assigneeName {
                LuminousAvatar(
                    member: FamilyMemberSummary(
                        id: task.assigneeId ?? "",
                        displayName: assignee,
                        avatarURL: nil,
                        color: task.assigneeColor
                    ),
                    size: .small
                )
            }
        }
        .padding(.vertical, DesignTokens.Spacing._2)
    }
}

// MARK: - Task Creation View

struct TaskCreationView: View {
    @Environment(\.dismiss) private var dismiss

    var body: some View {
        NavigationStack {
            Text("Task creation form - Coming soon")
                .navigationTitle("New Task")
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

// MARK: - Task Item Model

struct TaskItem: Identifiable {
    let id: String
    let title: String
    let description: String?
    let isCompleted: Bool
    let dueDate: Date?
    let assigneeId: String?
    let assigneeName: String?
    let assigneeColor: Color

    var isOverdue: Bool {
        guard let dueDate, !isCompleted else { return false }
        return dueDate < Date()
    }
}

// MARK: - Preview

#Preview {
    TasksView()
        .environmentObject(AppState())
        .environmentObject(AuthViewModel())
}
