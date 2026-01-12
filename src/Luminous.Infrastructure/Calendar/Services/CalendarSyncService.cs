using System.Diagnostics;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Calendar.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Luminous.Infrastructure.Calendar.Services;

/// <summary>
/// High-level service for managing calendar synchronization.
/// </summary>
public sealed class CalendarSyncService : ICalendarSyncService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<ICalendarProvider> _providers;
    private readonly CalendarSettings _settings;
    private readonly ILogger<CalendarSyncService> _logger;

    public CalendarSyncService(
        IUnitOfWork unitOfWork,
        IEnumerable<ICalendarProvider> providers,
        IOptions<CalendarSettings> settings,
        ILogger<CalendarSyncService> logger)
    {
        _unitOfWork = unitOfWork;
        _providers = providers;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<CalendarSyncSummary> SyncAsync(
        CalendarConnection connection,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "Starting calendar sync for connection {ConnectionId} ({Provider})",
            connection.Id, connection.Provider);

        try
        {
            // Refresh tokens if needed
            await RefreshTokensIfNeededAsync(connection, cancellationToken);

            var provider = GetProvider(connection.Provider);

            // Fetch events from the external calendar
            CalendarSyncResult result;
            if (connection.Provider == CalendarProvider.IcsUrl)
            {
                if (string.IsNullOrEmpty(connection.IcsUrl))
                    throw new InvalidOperationException("ICS URL is required for ICS subscriptions");

                result = await provider.FetchIcsEventsAsync(connection.IcsUrl, connection.ETag);
            }
            else
            {
                if (connection.Tokens is null)
                    throw new InvalidOperationException("OAuth tokens are required");
                if (string.IsNullOrEmpty(connection.ExternalCalendarId))
                    throw new InvalidOperationException("External calendar ID is required");

                var startDate = DateTime.UtcNow.AddDays(-connection.SyncSettings.SyncPastDays);
                var endDate = DateTime.UtcNow.AddDays(connection.SyncSettings.SyncFutureDays);

                result = await provider.FetchEventsAsync(
                    connection.Tokens,
                    connection.ExternalCalendarId,
                    startDate,
                    endDate,
                    connection.SyncToken);
            }

            // Process the sync result
            var (added, updated, deleted) = await ProcessSyncResultAsync(
                connection, result, cancellationToken);

            // Update connection state
            connection.RecordSuccessfulSync(result.SyncToken, result.ETag);
            await _unitOfWork.CalendarConnections.UpdateAsync(connection, cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "Calendar sync completed for {ConnectionId}: {Added} added, {Updated} updated, {Deleted} deleted in {Duration}ms",
                connection.Id, added, updated, deleted, stopwatch.ElapsedMilliseconds);

            return new CalendarSyncSummary
            {
                ConnectionId = connection.Id,
                FamilyId = connection.FamilyId,
                Success = true,
                EventsAdded = added,
                EventsUpdated = updated,
                EventsDeleted = deleted,
                Duration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var isAuthError = IsAuthenticationError(ex);

            _logger.LogError(ex,
                "Calendar sync failed for {ConnectionId}: {Error}",
                connection.Id, ex.Message);

            connection.RecordSyncFailure(ex.Message, isAuthError);
            await _unitOfWork.CalendarConnections.UpdateAsync(connection, cancellationToken);

            return new CalendarSyncSummary
            {
                ConnectionId = connection.Id,
                FamilyId = connection.FamilyId,
                Success = false,
                ErrorMessage = ex.Message,
                IsAuthError = isAuthError,
                Duration = stopwatch.Elapsed
            };
        }
    }

    public async Task<IReadOnlyList<CalendarSyncSummary>> SyncDueConnectionsAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var dueConnections = await _unitOfWork.CalendarConnections
            .GetDueSyncAsync(DateTime.UtcNow, limit, cancellationToken);

        _logger.LogInformation("Found {Count} connections due for sync", dueConnections.Count);

        var results = new List<CalendarSyncSummary>();

        foreach (var connection in dueConnections)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var result = await SyncAsync(connection, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    public Task<string> GetAuthorizationUrlAsync(
        CalendarProvider provider,
        string connectionId,
        string familyId,
        string redirectUri)
    {
        var calendarProvider = GetProvider(provider);
        var state = $"{connectionId}:{familyId}";
        return calendarProvider.GetAuthorizationUrlAsync(state, redirectUri);
    }

    public async Task CompleteOAuthAsync(
        string connectionId,
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        var connection = await _unitOfWork.CalendarConnections
            .GetByIdAsync(connectionId, cancellationToken: cancellationToken);

        if (connection is null)
            throw new InvalidOperationException($"Calendar connection {connectionId} not found");

        var provider = GetProvider(connection.Provider);
        var tokens = await provider.ExchangeCodeAsync(code, redirectUri);

        // Get available calendars and select the primary one
        var calendars = await provider.GetCalendarsAsync(tokens);
        var primaryCalendar = calendars.FirstOrDefault(c => c.IsPrimary)
                              ?? calendars.FirstOrDefault();

        if (primaryCalendar is null)
            throw new InvalidOperationException("No calendars available in the account");

        connection.CompleteOAuth(
            tokens,
            primaryCalendar.Id,
            null, // External account ID will be fetched separately if needed
            primaryCalendar.Name);

        if (!string.IsNullOrEmpty(primaryCalendar.Color))
            connection.Color = primaryCalendar.Color;

        await _unitOfWork.CalendarConnections.UpdateAsync(connection, cancellationToken);

        _logger.LogInformation(
            "OAuth completed for connection {ConnectionId}, calendar: {CalendarName}",
            connectionId, primaryCalendar.Name);
    }

    public async Task<bool> RefreshTokensIfNeededAsync(
        CalendarConnection connection,
        CancellationToken cancellationToken = default)
    {
        if (!connection.RequiresOAuth || connection.Tokens is null)
            return false;

        // Refresh if token expires in less than 5 minutes
        if (connection.Tokens.ExpiresAt > DateTime.UtcNow.AddMinutes(5))
            return false;

        if (!connection.Tokens.CanRefresh)
        {
            _logger.LogWarning(
                "Cannot refresh tokens for {ConnectionId}: no refresh token",
                connection.Id);
            return false;
        }

        try
        {
            var provider = GetProvider(connection.Provider);
            var newTokens = await provider.RefreshTokensAsync(connection.Tokens);
            connection.UpdateTokens(newTokens);
            await _unitOfWork.CalendarConnections.UpdateAsync(connection, cancellationToken);

            _logger.LogInformation("Tokens refreshed for connection {ConnectionId}", connection.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh tokens for {ConnectionId}", connection.Id);
            throw;
        }
    }

    public async Task<IReadOnlyList<ExternalCalendarInfo>> GetAvailableCalendarsAsync(
        CalendarConnection connection,
        CancellationToken cancellationToken = default)
    {
        if (connection.Tokens is null)
            throw new InvalidOperationException("OAuth tokens are required");

        await RefreshTokensIfNeededAsync(connection, cancellationToken);

        var provider = GetProvider(connection.Provider);
        return await provider.GetCalendarsAsync(connection.Tokens);
    }

    private ICalendarProvider GetProvider(CalendarProvider providerType)
    {
        var provider = _providers.FirstOrDefault(p => p.ProviderType == providerType);
        if (provider is null)
            throw new NotSupportedException($"Calendar provider {providerType} is not configured");
        return provider;
    }

    private async Task<(int Added, int Updated, int Deleted)> ProcessSyncResultAsync(
        CalendarConnection connection,
        CalendarSyncResult result,
        CancellationToken cancellationToken)
    {
        var added = 0;
        var updated = 0;
        var deleted = 0;

        // Process deleted events
        foreach (var deletedId in result.DeletedEventIds)
        {
            var existingEvent = await _unitOfWork.Events
                .GetByExternalEventIdAsync(connection.FamilyId, deletedId, cancellationToken);

            if (existingEvent is not null)
            {
                await _unitOfWork.Events.DeleteAsync(existingEvent, cancellationToken);
                deleted++;
            }
        }

        // Process new/updated events
        foreach (var externalEvent in result.Events)
        {
            // Skip cancelled events
            if (externalEvent.IsCancelled)
            {
                var cancelledEvent = await _unitOfWork.Events
                    .GetByExternalEventIdAsync(connection.FamilyId, externalEvent.ExternalId, cancellationToken);

                if (cancelledEvent is not null)
                {
                    await _unitOfWork.Events.DeleteAsync(cancelledEvent, cancellationToken);
                    deleted++;
                }
                continue;
            }

            var existingEvent = await _unitOfWork.Events
                .GetByExternalEventIdAsync(connection.FamilyId, externalEvent.ExternalId, cancellationToken);

            if (existingEvent is null)
            {
                // Create new event
                var newEvent = CreateEventFromExternal(connection, externalEvent);
                await _unitOfWork.Events.AddAsync(newEvent, cancellationToken);
                added++;
            }
            else
            {
                // Update existing event
                UpdateEventFromExternal(existingEvent, connection, externalEvent);
                await _unitOfWork.Events.UpdateAsync(existingEvent, cancellationToken);
                updated++;
            }
        }

        return (added, updated, deleted);
    }

    private static Event CreateEventFromExternal(
        CalendarConnection connection,
        ExternalCalendarEvent externalEvent)
    {
        Event domainEvent;

        if (externalEvent.IsAllDay)
        {
            // For all-day events, store dates only (no timezone conversion needed)
            // The dates from the external calendar represent the calendar dates directly
            var startDate = DateOnly.FromDateTime(externalEvent.StartTime);
            var endDate = DateOnly.FromDateTime(externalEvent.EndTime);

            // Ensure endDate is exclusive (at least startDate + 1 day) per iCal spec.
            // Some ICS sources may not include DTEND or may set it equal to DTSTART.
            if (endDate <= startDate)
            {
                endDate = startDate.AddDays(1);
            }

            domainEvent = Event.CreateAllDay(
                connection.FamilyId,
                externalEvent.Title,
                startDate,
                endDate,
                "system");
        }
        else
        {
            // For timed events, store the UTC times
            domainEvent = Event.CreateTimed(
                connection.FamilyId,
                externalEvent.Title,
                externalEvent.StartTime.ToUniversalTime(),
                externalEvent.EndTime.ToUniversalTime(),
                "system");
        }

        domainEvent.Description = externalEvent.Description;
        domainEvent.LocationText = externalEvent.Location;
        domainEvent.Source = connection.Provider;
        domainEvent.ExternalCalendarId = connection.ExternalCalendarId ?? connection.Id;
        domainEvent.ExternalEventId = externalEvent.ExternalId;
        domainEvent.Color = externalEvent.Color ?? connection.Color;
        domainEvent.Assignees = connection.AssignedMemberIds.ToList();

        if (externalEvent.Reminders.Count > 0)
            domainEvent.Reminders = externalEvent.Reminders;

        // Parse recurrence rule if present
        if (!string.IsNullOrEmpty(externalEvent.RecurrenceRule))
        {
            domainEvent.Recurrence = ParseRecurrenceRule(externalEvent.RecurrenceRule);
        }

        return domainEvent;
    }

    private static void UpdateEventFromExternal(
        Event domainEvent,
        CalendarConnection connection,
        ExternalCalendarEvent externalEvent)
    {
        domainEvent.Title = externalEvent.Title;
        domainEvent.Description = externalEvent.Description;
        domainEvent.IsAllDay = externalEvent.IsAllDay;
        domainEvent.LocationText = externalEvent.Location;
        domainEvent.Color = externalEvent.Color ?? connection.Color;

        if (externalEvent.IsAllDay)
        {
            // For all-day events, store dates only
            var startDate = DateOnly.FromDateTime(externalEvent.StartTime);
            var endDate = DateOnly.FromDateTime(externalEvent.EndTime);

            // Ensure endDate is exclusive (at least startDate + 1 day) per iCal spec.
            // Some ICS sources may not include DTEND or may set it equal to DTSTART.
            if (endDate <= startDate)
            {
                endDate = startDate.AddDays(1);
            }

            domainEvent.StartDate = startDate;
            domainEvent.EndDate = endDate;
            domainEvent.StartTime = null;
            domainEvent.EndTime = null;
        }
        else
        {
            // For timed events, store UTC times
            domainEvent.StartTime = externalEvent.StartTime.ToUniversalTime();
            domainEvent.EndTime = externalEvent.EndTime.ToUniversalTime();
            domainEvent.StartDate = null;
            domainEvent.EndDate = null;
        }

        if (externalEvent.Reminders.Count > 0)
            domainEvent.Reminders = externalEvent.Reminders;

        // Update recurrence if changed
        if (!string.IsNullOrEmpty(externalEvent.RecurrenceRule))
        {
            domainEvent.Recurrence = ParseRecurrenceRule(externalEvent.RecurrenceRule);
        }
        else
        {
            domainEvent.Recurrence = null;
        }

        domainEvent.ModifiedAt = DateTime.UtcNow;
        domainEvent.ModifiedBy = "sync";
    }

    private static Domain.ValueObjects.RecurrenceRule? ParseRecurrenceRule(string rrule)
    {
        // Simple RRULE parser - handles basic patterns
        if (!rrule.StartsWith("RRULE:"))
            return null;

        var rule = rrule[6..]; // Remove "RRULE:" prefix
        var parts = rule.Split(';')
            .Select(p => p.Split('='))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0], p => p[1]);

        var pattern = parts.GetValueOrDefault("FREQ") switch
        {
            "DAILY" => RecurrencePattern.Daily,
            "WEEKLY" => RecurrencePattern.Weekly,
            "MONTHLY" => RecurrencePattern.Monthly,
            "YEARLY" => RecurrencePattern.Yearly,
            _ => RecurrencePattern.None
        };

        if (pattern == RecurrencePattern.None)
            return null;

        var recurrence = new Domain.ValueObjects.RecurrenceRule
        {
            Pattern = pattern,
            Interval = parts.TryGetValue("INTERVAL", out var interval)
                ? int.Parse(interval)
                : 1
        };

        // Parse BYDAY for weekly recurrence
        if (parts.TryGetValue("BYDAY", out var byDay))
        {
            recurrence.DaysOfWeek = byDay.Split(',')
                .Select(d => d switch
                {
                    "SU" => 0,
                    "MO" => 1,
                    "TU" => 2,
                    "WE" => 3,
                    "TH" => 4,
                    "FR" => 5,
                    "SA" => 6,
                    _ => -1
                })
                .Where(d => d >= 0)
                .ToList();
        }

        // Parse UNTIL
        if (parts.TryGetValue("UNTIL", out var until))
        {
            if (DateOnly.TryParseExact(until, "yyyyMMdd", out var endDate))
                recurrence.EndDate = endDate;
        }

        // Parse COUNT
        if (parts.TryGetValue("COUNT", out var count))
        {
            recurrence.MaxOccurrences = int.Parse(count);
        }

        return recurrence;
    }

    private static bool IsAuthenticationError(Exception ex)
    {
        var message = ex.Message.ToLowerInvariant();
        return message.Contains("unauthorized") ||
               message.Contains("401") ||
               message.Contains("invalid_grant") ||
               message.Contains("token") && message.Contains("expired");
    }
}
