using Luminous.Application.Common.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Luminous.Functions.Sync.Functions;

/// <summary>
/// Timer-triggered function for syncing calendar connections.
/// </summary>
public class CalendarSyncFunction
{
    private readonly ICalendarSyncService _calendarSyncService;
    private readonly ILogger<CalendarSyncFunction> _logger;

    public CalendarSyncFunction(
        ICalendarSyncService calendarSyncService,
        ILogger<CalendarSyncFunction> logger)
    {
        _calendarSyncService = calendarSyncService;
        _logger = logger;
    }

    /// <summary>
    /// Runs every 5 minutes to sync calendar connections that are due.
    /// </summary>
    [Function(nameof(SyncDueCalendars))]
    public async Task SyncDueCalendars(
        [TimerTrigger("0 */5 * * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calendar sync job started at: {Time}", DateTime.UtcNow);

        try
        {
            var results = await _calendarSyncService.SyncDueConnectionsAsync(
                limit: 50,
                cancellationToken: cancellationToken);

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);
            var totalEvents = results.Sum(r => r.EventsAdded + r.EventsUpdated + r.EventsDeleted);

            _logger.LogInformation(
                "Calendar sync job completed. Synced: {Success}/{Total} connections, Events processed: {Events}",
                successCount,
                results.Count,
                totalEvents);

            if (failureCount > 0)
            {
                foreach (var failure in results.Where(r => !r.Success))
                {
                    _logger.LogWarning(
                        "Calendar sync failed for connection {ConnectionId}: {Error}",
                        failure.ConnectionId,
                        failure.ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Calendar sync job failed with exception");
            throw;
        }
    }
}
