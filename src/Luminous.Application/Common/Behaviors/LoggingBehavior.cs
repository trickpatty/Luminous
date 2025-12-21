using Luminous.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for logging requests.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId ?? "Anonymous";
        var familyId = _currentUserService.FamilyId ?? "None";

        _logger.LogInformation(
            "Luminous Request: {Name} UserId: {UserId} FamilyId: {FamilyId} {@Request}",
            requestName, userId, familyId, request);

        var response = await next();

        _logger.LogInformation(
            "Luminous Response: {Name} UserId: {UserId}",
            requestName, userId);

        return response;
    }
}
