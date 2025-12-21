using System.Diagnostics;
using Luminous.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for performance monitoring.
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _timer = new Stopwatch();
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId ?? "Anonymous";
            var familyId = _currentUserService.FamilyId ?? "None";

            _logger.LogWarning(
                "Luminous Long Running Request: {Name} ({ElapsedMilliseconds}ms) UserId: {UserId} FamilyId: {FamilyId} {@Request}",
                requestName, elapsedMilliseconds, userId, familyId, request);
        }

        return response;
    }
}
