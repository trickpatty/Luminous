using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cosmos DB repository for OTP Token entities.
/// </summary>
public sealed class OtpTokenRepository : CosmosRepositoryBase<OtpToken>, IOtpTokenRepository
{
    public OtpTokenRepository(CosmosDbContext context, ILogger<OtpTokenRepository> logger)
        : base(context, logger, ContainerNames.OtpTokens)
    {
    }

    protected override PartitionKey GetPartitionKey(OtpToken entity) => new(entity.Email.ToLowerInvariant());

    protected override string GetPartitionKeyPath() => "/email";

    public async Task<IReadOnlyList<OtpToken>> GetActiveByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var now = DateTime.UtcNow.ToString("o");

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.email = @email AND c.isUsed = false AND c.expiresAt > @now ORDER BY c.createdAt DESC")
            .WithParameter("@email", normalizedEmail)
            .WithParameter("@now", now);

        return await QueryAsync(query, normalizedEmail, cancellationToken);
    }

    public async Task<OtpToken?> GetLatestActiveByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var now = DateTime.UtcNow.ToString("o");

        var query = new QueryDefinition(
            "SELECT TOP 1 * FROM c WHERE c.email = @email AND c.isUsed = false AND c.expiresAt > @now ORDER BY c.createdAt DESC")
            .WithParameter("@email", normalizedEmail)
            .WithParameter("@now", now);

        var results = await QueryAsync(query, normalizedEmail, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task InvalidateAllForEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await GetActiveByEmailAsync(email, cancellationToken);

        foreach (var token in activeTokens)
        {
            token.MarkAsUsed();
            await UpdateAsync(token, cancellationToken);
        }

        Logger.LogInformation("Invalidated {Count} OTP tokens for email {Email}", activeTokens.Count, email);
    }

    public async Task DeleteExpiredAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync(cancellationToken);
        var olderThanStr = olderThan.ToString("o");

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.expiresAt < @olderThan")
            .WithParameter("@olderThan", olderThanStr);

        var expiredTokens = await QueryAsync(query, cancellationToken: cancellationToken);

        foreach (var token in expiredTokens)
        {
            await DeleteAsync(token, cancellationToken);
        }

        Logger.LogInformation("Deleted {Count} expired OTP tokens older than {OlderThan}", expiredTokens.Count, olderThan);
    }

    public async Task<int> CountRecentRequestsAsync(
        string email,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var since = DateTime.UtcNow.Subtract(window).ToString("o");

        var query = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.email = @email AND c.createdAt > @since")
            .WithParameter("@email", normalizedEmail)
            .WithParameter("@since", since);

        var container = await GetContainerAsync(cancellationToken);
        var queryOptions = new QueryRequestOptions { PartitionKey = new PartitionKey(normalizedEmail) };
        var iterator = container.GetItemQueryIterator<int>(query, requestOptions: queryOptions);
        var response = await iterator.ReadNextAsync(cancellationToken);

        return response.FirstOrDefault();
    }
}
