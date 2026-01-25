//
//  HomeViewModel.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI
import Combine

/// ViewModel for the Home dashboard screen.
@MainActor
final class HomeViewModel: ObservableObject {
    // MARK: - Published Properties

    @Published var todayEvents: [CalendarEventSummary] = []
    @Published var tasksDueToday: [TaskSummary] = []
    @Published var recentActivities: [FamilyActivity] = []
    @Published var isLoading: Bool = false
    @Published var error: Error?

    // MARK: - Dependencies

    private let eventService: EventServiceProtocol
    private let taskService: TaskServiceProtocol
    private let activityService: ActivityServiceProtocol

    // MARK: - Private Properties

    private var cancellables = Set<AnyCancellable>()

    // MARK: - Initialization

    init(
        eventService: EventServiceProtocol = EventService.shared,
        taskService: TaskServiceProtocol = TaskService.shared,
        activityService: ActivityServiceProtocol = ActivityService.shared
    ) {
        self.eventService = eventService
        self.taskService = taskService
        self.activityService = activityService

        // Load initial data
        Task {
            await loadData()
        }
    }

    // MARK: - Public Methods

    /// Refresh all data synchronously (for button taps).
    func refresh() {
        Task {
            await refreshAsync()
        }
    }

    /// Refresh all data asynchronously (for pull-to-refresh).
    func refreshAsync() async {
        await loadData()
    }

    // MARK: - Private Methods

    private func loadData() async {
        isLoading = true
        error = nil

        do {
            async let events = eventService.getTodayEvents()
            async let tasks = taskService.getTasksDueToday()
            async let activities = activityService.getRecentActivities(limit: 5)

            let (loadedEvents, loadedTasks, loadedActivities) = try await (events, tasks, activities)

            todayEvents = loadedEvents
            tasksDueToday = loadedTasks
            recentActivities = loadedActivities
        } catch {
            self.error = error
        }

        isLoading = false
    }
}

// MARK: - Data Models

/// Summary of a calendar event for display.
struct CalendarEventSummary: Identifiable {
    let id: String
    let title: String
    let startTime: Date
    let endTime: Date?
    let isAllDay: Bool
    let memberColor: Color
    let memberName: String?

    var timeString: String {
        if isAllDay {
            return "All day"
        }

        let formatter = DateFormatter()
        formatter.timeStyle = .short

        if let endTime {
            return "\(formatter.string(from: startTime)) - \(formatter.string(from: endTime))"
        }
        return formatter.string(from: startTime)
    }
}

/// Summary of a task for display.
struct TaskSummary: Identifiable {
    let id: String
    let title: String
    let isCompleted: Bool
    let dueDate: Date?
    let assignee: FamilyMemberSummary?
}

/// Summary of a family member for display in compact views.
struct FamilyMemberSummary: Identifiable {
    let id: String
    let displayName: String
    let avatarURL: URL?
    let color: Color

    var initials: String {
        let components = displayName.split(separator: " ")
        if components.count >= 2 {
            return "\(components[0].prefix(1))\(components[1].prefix(1))".uppercased()
        }
        return String(displayName.prefix(2)).uppercased()
    }
}

/// Recent family activity item.
struct FamilyActivity: Identifiable {
    let id: String
    let member: FamilyMemberSummary
    let description: String
    let timestamp: Date
    let type: ActivityType

    var timeAgo: String {
        let formatter = RelativeDateTimeFormatter()
        formatter.unitsStyle = .abbreviated
        return formatter.localizedString(for: timestamp, relativeTo: Date())
    }

    enum ActivityType {
        case taskCompleted
        case eventCreated
        case profileUpdated
        case memberJoined
    }
}

// MARK: - Service Protocols

/// Protocol for fetching calendar events.
protocol EventServiceProtocol {
    func getTodayEvents() async throws -> [CalendarEventSummary]
}

/// Protocol for fetching tasks.
protocol TaskServiceProtocol {
    func getTasksDueToday() async throws -> [TaskSummary]
}

/// Protocol for fetching family activity.
protocol ActivityServiceProtocol {
    func getRecentActivities(limit: Int) async throws -> [FamilyActivity]
}

// MARK: - Placeholder Service Implementations

/// Placeholder implementation of EventService.
final class EventService: EventServiceProtocol {
    static let shared = EventService()

    func getTodayEvents() async throws -> [CalendarEventSummary] {
        // TODO: Implement with actual API calls
        return []
    }
}

/// Placeholder implementation of TaskService.
final class TaskService: TaskServiceProtocol {
    static let shared = TaskService()

    func getTasksDueToday() async throws -> [TaskSummary] {
        // TODO: Implement with actual API calls
        return []
    }
}

/// Placeholder implementation of ActivityService.
final class ActivityService: ActivityServiceProtocol {
    static let shared = ActivityService()

    func getRecentActivities(limit: Int) async throws -> [FamilyActivity] {
        // TODO: Implement with actual API calls
        return []
    }
}
