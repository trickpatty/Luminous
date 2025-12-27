using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cosmos DB repository for Refresh Token entities.
/// </summary>
public sealed class RefreshTokenRepository : CosmosRepositoryBase<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(CosmosDbContext context, ILogger<RefreshTokenRepository> logger)
        : base(context, logger, ContainerNames.RefreshTokens)
    {
    }

    protected override PartitionKey GetPartitionKey(RefreshToken entity) => new(entity.UserId);

    protected override string GetPartitionKeyPath() => "/userId";

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.ToString("o");

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId = @userId AND c.isRevoked = false AND c.expiresAt > @now ORDER BY c.createdAt DESC")
            .WithParameter("@userId", userId)
            .WithParameter("@now", now);

        return await QueryAsync(query, userId, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.tokenHash = @tokenHash")
            .WithParameter("@tokenHash", tokenHash);

        var results = await QueryAsync(query, cancellationToken: cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task RevokeAllForUserAsync(
        string userId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await GetActiveByUserIdAsync(userId, cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(reason);
            await UpdateAsync(token, cancellationToken);
        }

        Logger.LogInformation("Revoked {Count} refresh tokens for user {UserId} with reason: {Reason}",
            activeTokens.Count, userId, reason);
    }

    public async Task RevokeAllExceptAsync(
        string userId,
        string exceptTokenId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await GetActiveByUserIdAsync(userId, cancellationToken);

        var tokensToRevoke = activeTokens.Where(t => t.Id != exceptTokenId).ToList();
        foreach (var token in tokensToRevoke)
        {
            token.Revoke(reason);
            await UpdateAsync(token, cancellationToken);
        }

        Logger.LogInformation("Revoked {Count} refresh tokens for user {UserId} except {ExceptTokenId}",
            tokensToRevoke.Count, userId, exceptTokenId);
    }

    public async Task RevokeAllForDeviceAsync(
        string deviceId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.ToString("o");

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.deviceId = @deviceId AND c.isRevoked = false AND c.expiresAt > @now")
            .WithParameter("@deviceId", deviceId)
            .WithParameter("@now", now);

        var activeTokens = await QueryAsync(query, cancellationToken: cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(reason);
            await UpdateAsync(token, cancellationToken);
        }

        Logger.LogInformation("Revoked {Count} refresh tokens for device {DeviceId} with reason: {Reason}",
            activeTokens.Count, deviceId, reason);
    }

    public async Task DeleteExpiredAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default)
    {
        var olderThanStr = olderThan.ToString("o");

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.expiresAt < @olderThan AND (c.isRevoked = true OR c.expiresAt < @now)")
            .WithParameter("@olderThan", olderThanStr)
            .WithParameter("@now", DateTime.UtcNow.ToString("o"));

        var expiredTokens = await QueryAsync(query, cancellationToken: cancellationToken);

        foreach (var token in expiredTokens)
        {
            await DeleteAsync(token, cancellationToken);
        }

        Logger.LogInformation("Deleted {Count} expired/revoked refresh tokens older than {OlderThan}",
            expiredTokens.Count, olderThan);
    }

    public async Task<int> CountActiveForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.ToString("o");

        var query = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.userId = @userId AND c.isRevoked = false AND c.expiresAt > @now")
            .WithParameter("@userId", userId)
            .WithParameter("@now", now);

        var container = await GetContainerAsync(cancellationToken);
        var queryOptions = new QueryRequestOptions { PartitionKey = new PartitionKey(userId) };
        var iterator = container.GetItemQueryIterator<int>(query, requestOptions: queryOptions);
        var response = await iterator.ReadNextAsync(cancellationToken);

        return response.FirstOrDefault();
    }
}
