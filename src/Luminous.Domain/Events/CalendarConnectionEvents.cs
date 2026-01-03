using Luminous.Domain.Common;
using Luminous.Domain.Enums;

namespace Luminous.Domain.Events;

/// <summary>
/// Event raised when a calendar connection is created.
/// </summary>
public sealed class CalendarConnectionCreatedEvent : DomainEvent
{
    public string ConnectionId { get; }
    public string FamilyId { get; }
    public CalendarProvider Provider { get; }
    public string Name { get; }

    public CalendarConnectionCreatedEvent(
        string connectionId,
        string familyId,
        CalendarProvider provider,
        string name)
    {
        ConnectionId = connectionId;
        FamilyId = familyId;
        Provider = provider;
        Name = name;
    }
}

/// <summary>
/// Event raised when a calendar connection is activated (OAuth completed).
/// </summary>
public sealed class CalendarConnectionActivatedEvent : DomainEvent
{
    public string ConnectionId { get; }
    public string FamilyId { get; }
    public CalendarProvider Provider { get; }
    public string ExternalCalendarId { get; }

    public CalendarConnectionActivatedEvent(
        string connectionId,
        string familyId,
        CalendarProvider provider,
        string externalCalendarId)
    {
        ConnectionId = connectionId;
        FamilyId = familyId;
        Provider = provider;
        ExternalCalendarId = externalCalendarId;
    }
}

/// <summary>
/// Event raised when a calendar sync completes successfully.
/// </summary>
public sealed class CalendarSyncCompletedEvent : DomainEvent
{
    public string ConnectionId { get; }
    public string FamilyId { get; }
    public int EventsAdded { get; }
    public int EventsUpdated { get; }
    public int EventsDeleted { get; }

    public CalendarSyncCompletedEvent(
        string connectionId,
        string familyId,
        int eventsAdded,
        int eventsUpdated,
        int eventsDeleted)
    {
        ConnectionId = connectionId;
        FamilyId = familyId;
        EventsAdded = eventsAdded;
        EventsUpdated = eventsUpdated;
        EventsDeleted = eventsDeleted;
    }
}

/// <summary>
/// Event raised when a calendar sync fails.
/// </summary>
public sealed class CalendarSyncFailedEvent : DomainEvent
{
    public string ConnectionId { get; }
    public string FamilyId { get; }
    public string Error { get; }
    public bool IsAuthError { get; }

    public CalendarSyncFailedEvent(
        string connectionId,
        string familyId,
        string error,
        bool isAuthError)
    {
        ConnectionId = connectionId;
        FamilyId = familyId;
        Error = error;
        IsAuthError = isAuthError;
    }
}

/// <summary>
/// Event raised when a calendar connection is disconnected.
/// </summary>
public sealed class CalendarConnectionDisconnectedEvent : DomainEvent
{
    public string ConnectionId { get; }
    public string FamilyId { get; }
    public CalendarProvider Provider { get; }

    public CalendarConnectionDisconnectedEvent(
        string connectionId,
        string familyId,
        CalendarProvider provider)
    {
        ConnectionId = connectionId;
        FamilyId = familyId;
        Provider = provider;
    }
}
