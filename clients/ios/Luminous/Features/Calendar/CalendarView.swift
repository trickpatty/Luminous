//
//  CalendarView.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import SwiftUI

/// Calendar feature showing family schedule with multiple view modes.
struct CalendarView: View {
    // MARK: - State

    @StateObject private var viewModel = CalendarViewModel()
    @EnvironmentObject private var appState: AppState

    @State private var selectedViewMode: CalendarViewMode = .day
    @State private var selectedDate: Date = Date()
    @State private var showingEventCreation = false

    // MARK: - Body

    var body: some View {
        NavigationStack {
            VStack(spacing: 0) {
                // View mode picker
                Picker("View", selection: $selectedViewMode) {
                    ForEach(CalendarViewMode.allCases) { mode in
                        Text(mode.title).tag(mode)
                    }
                }
                .pickerStyle(.segmented)
                .padding(.horizontal, DesignTokens.Spacing._4)
                .padding(.vertical, DesignTokens.Spacing._2)

                // Calendar content based on selected mode
                Group {
                    switch selectedViewMode {
                    case .day:
                        DayCalendarView(
                            selectedDate: $selectedDate,
                            events: viewModel.eventsForDate(selectedDate)
                        )
                    case .week:
                        WeekCalendarView(
                            selectedDate: $selectedDate,
                            events: viewModel.eventsForWeek(containing: selectedDate)
                        )
                    case .month:
                        MonthCalendarView(
                            selectedDate: $selectedDate,
                            events: viewModel.allEvents
                        )
                    case .agenda:
                        AgendaCalendarView(
                            events: viewModel.upcomingEvents
                        )
                    }
                }
            }
            .background(appState.canvasColor.ignoresSafeArea())
            .navigationTitle("Calendar")
            .toolbar {
                ToolbarItem(placement: .topBarLeading) {
                    Button("Today") {
                        selectedDate = Date()
                    }
                }

                ToolbarItem(placement: .topBarTrailing) {
                    Button {
                        showingEventCreation = true
                    } label: {
                        Image(systemName: "plus")
                    }
                }
            }
            .sheet(isPresented: $showingEventCreation) {
                EventCreationView()
            }
        }
    }
}

// MARK: - Calendar View Mode

enum CalendarViewMode: String, CaseIterable, Identifiable {
    case day
    case week
    case month
    case agenda

    var id: String { rawValue }

    var title: String {
        switch self {
        case .day: return "Day"
        case .week: return "Week"
        case .month: return "Month"
        case .agenda: return "Agenda"
        }
    }
}

// MARK: - Placeholder Sub-Views

/// Day view showing hourly timeline.
struct DayCalendarView: View {
    @Binding var selectedDate: Date
    let events: [CalendarEventSummary]

    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 0) {
                // Date navigation
                HStack {
                    Button {
                        selectedDate = Calendar.current.date(byAdding: .day, value: -1, to: selectedDate) ?? selectedDate
                    } label: {
                        Image(systemName: "chevron.left")
                    }

                    Spacer()

                    Text(selectedDate, format: .dateTime.weekday(.wide).month().day())
                        .font(.system(size: DesignTokens.FontSize.title_md, weight: .semibold))

                    Spacer()

                    Button {
                        selectedDate = Calendar.current.date(byAdding: .day, value: 1, to: selectedDate) ?? selectedDate
                    } label: {
                        Image(systemName: "chevron.right")
                    }
                }
                .padding(.horizontal, DesignTokens.Spacing._4)
                .padding(.vertical, DesignTokens.Spacing._3)

                Divider()

                // Hourly timeline
                ForEach(0..<24, id: \.self) { hour in
                    HourRow(hour: hour, events: events.filter { event in
                        Calendar.current.component(.hour, from: event.startTime) == hour
                    })
                }
            }
        }
    }
}

/// Single hour row in the day view.
private struct HourRow: View {
    let hour: Int
    let events: [CalendarEventSummary]

    var body: some View {
        HStack(alignment: .top, spacing: DesignTokens.Spacing._3) {
            Text(String(format: "%02d:00", hour))
                .font(.system(size: DesignTokens.FontSize.caption, design: .monospaced))
                .foregroundStyle(DesignTokens.Colors.text_tertiary)
                .frame(width: 50, alignment: .trailing)

            VStack(alignment: .leading, spacing: DesignTokens.Spacing._1) {
                ForEach(events) { event in
                    EventChip(event: event)
                }
            }

            Spacer()
        }
        .padding(.horizontal, DesignTokens.Spacing._4)
        .frame(minHeight: 60)
        .overlay(alignment: .bottom) {
            Divider()
        }
    }
}

/// Week view showing 7-day overview.
struct WeekCalendarView: View {
    @Binding var selectedDate: Date
    let events: [CalendarEventSummary]

    var body: some View {
        Text("Week view - Coming soon")
            .foregroundStyle(DesignTokens.Colors.text_secondary)
            .frame(maxWidth: .infinity, maxHeight: .infinity)
    }
}

/// Month view with calendar grid.
struct MonthCalendarView: View {
    @Binding var selectedDate: Date
    let events: [CalendarEventSummary]

    var body: some View {
        Text("Month view - Coming soon")
            .foregroundStyle(DesignTokens.Colors.text_secondary)
            .frame(maxWidth: .infinity, maxHeight: .infinity)
    }
}

/// Agenda view showing chronological event list.
struct AgendaCalendarView: View {
    let events: [CalendarEventSummary]

    var body: some View {
        if events.isEmpty {
            EmptyStateView(
                icon: "calendar",
                title: "No upcoming events",
                subtitle: "Your schedule looks clear"
            )
        } else {
            List(events) { event in
                EventRow(event: event)
            }
            .listStyle(.plain)
        }
    }

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

                    Text(event.timeString)
                        .font(.system(size: DesignTokens.FontSize.body_sm))
                        .foregroundStyle(DesignTokens.Colors.text_secondary)
                }
            }
            .padding(.vertical, DesignTokens.Spacing._2)
        }
    }
}

/// Compact event display chip.
private struct EventChip: View {
    let event: CalendarEventSummary

    var body: some View {
        HStack(spacing: DesignTokens.Spacing._2) {
            Circle()
                .fill(event.memberColor)
                .frame(width: 8, height: 8)

            Text(event.title)
                .font(.system(size: DesignTokens.FontSize.body_sm))
                .lineLimit(1)
        }
        .padding(.horizontal, DesignTokens.Spacing._2)
        .padding(.vertical, DesignTokens.Spacing._1)
        .background(event.memberColor.opacity(0.1))
        .clipShape(RoundedRectangle(cornerRadius: DesignTokens.Radius.sm))
    }
}

/// Placeholder for event creation.
struct EventCreationView: View {
    @Environment(\.dismiss) private var dismiss

    var body: some View {
        NavigationStack {
            Text("Event creation form - Coming soon")
                .navigationTitle("New Event")
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

// MARK: - Preview

#Preview {
    CalendarView()
        .environmentObject(AppState())
        .environmentObject(AuthViewModel())
}
