//
//  TasksViewModel.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI
import Combine

/// ViewModel for the Tasks feature.
@MainActor
final class TasksViewModel: ObservableObject {
    // MARK: - Published Properties

    @Published var tasks: [TaskItem] = []
    @Published var isLoading: Bool = false
    @Published var error: Error?

    // MARK: - Private Properties

    private var cancellables = Set<AnyCancellable>()

    // MARK: - Initialization

    init() {
        Task {
            await loadTasks()
        }
    }

    // MARK: - Public Methods

    /// Get tasks filtered by the selected filter.
    func filteredTasks(for filter: TaskFilter) -> [TaskItem] {
        switch filter {
        case .all:
            return tasks.filter { !$0.isCompleted }
        case .today:
            let calendar = Calendar.current
            return tasks.filter { task in
                guard let dueDate = task.dueDate, !task.isCompleted else { return false }
                return calendar.isDateInToday(dueDate)
            }
        case .overdue:
            return tasks.filter { $0.isOverdue }
        case .completed:
            return tasks.filter { $0.isCompleted }
        }
    }

    /// Toggle the completion status of a task.
    func toggleTaskCompletion(_ task: TaskItem) {
        guard let index = tasks.firstIndex(where: { $0.id == task.id }) else { return }

        let updatedTask = TaskItem(
            id: task.id,
            title: task.title,
            description: task.description,
            isCompleted: !task.isCompleted,
            dueDate: task.dueDate,
            assigneeId: task.assigneeId,
            assigneeName: task.assigneeName,
            assigneeColor: task.assigneeColor
        )

        tasks[index] = updatedTask

        // TODO: Sync with API
    }

    /// Delete tasks at the specified indices.
    func deleteTasks(at indexSet: IndexSet, filter: TaskFilter) {
        let filtered = filteredTasks(for: filter)
        let idsToDelete = indexSet.map { filtered[$0].id }

        tasks.removeAll { idsToDelete.contains($0.id) }

        // TODO: Sync with API
    }

    /// Refresh tasks from the server.
    func refresh() async {
        await loadTasks()
    }

    // MARK: - Private Methods

    private func loadTasks() async {
        isLoading = true
        error = nil

        do {
            tasks = try await fetchTasksFromAPI()
        } catch {
            self.error = error
        }

        isLoading = false
    }

    private func fetchTasksFromAPI() async throws -> [TaskItem] {
        // TODO: Implement with actual API call
        return []
    }
}
