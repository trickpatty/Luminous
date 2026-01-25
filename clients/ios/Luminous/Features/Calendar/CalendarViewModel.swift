//
//  CalendarViewModel.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI
import Combine

/// ViewModel for the Calendar feature.
@MainActor
final class CalendarViewModel: ObservableObject {
    // MARK: - Published Properties

    @Published var allEvents: [CalendarEventSummary] = []
    @Published var isLoading: Bool = false
    @Published var error: Error?

    // MARK: - Dependencies

    private let eventService: EventServiceProtocol

    // MARK: - Private Properties

    private var cancellables = Set<AnyCancellable>()

    // MARK: - Computed Properties

    /// Events sorted by date, upcoming first.
    var upcomingEvents: [CalendarEventSummary] {
        allEvents
            .filter { $0.startTime >= Date() }
            .sorted { $0.startTime < $1.startTime }
    }

    // MARK: - Initialization

    init(eventService: EventServiceProtocol = EventService.shared) {
        self.eventService = eventService

        Task {
            await loadEvents()
        }
    }

    // MARK: - Public Methods

    /// Get events for a specific date.
    func eventsForDate(_ date: Date) -> [CalendarEventSummary] {
        let calendar = Calendar.current
        return allEvents.filter { event in
            calendar.isDate(event.startTime, inSameDayAs: date)
        }.sorted { $0.startTime < $1.startTime }
    }

    /// Get events for the week containing the given date.
    func eventsForWeek(containing date: Date) -> [CalendarEventSummary] {
        let calendar = Calendar.current
        guard let weekInterval = calendar.dateInterval(of: .weekOfYear, for: date) else {
            return []
        }

        return allEvents.filter { event in
            event.startTime >= weekInterval.start && event.startTime < weekInterval.end
        }.sorted { $0.startTime < $1.startTime }
    }

    /// Refresh events from the server.
    func refresh() async {
        await loadEvents()
    }

    // MARK: - Private Methods

    private func loadEvents() async {
        isLoading = true
        error = nil

        do {
            // Load events for the next 90 days
            let endDate = Calendar.current.date(byAdding: .day, value: 90, to: Date()) ?? Date()
            allEvents = try await loadEventsFromAPI(from: Date(), to: endDate)
        } catch {
            self.error = error
        }

        isLoading = false
    }

    private func loadEventsFromAPI(from startDate: Date, to endDate: Date) async throws -> [CalendarEventSummary] {
        // TODO: Implement with actual API call
        return []
    }
}
