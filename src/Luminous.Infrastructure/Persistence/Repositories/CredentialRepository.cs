using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cosmos DB repository for Credential entities (WebAuthn).
/// </summary>
public sealed class CredentialRepository : CosmosRepositoryBase<Credential>, ICredentialRepository
{
    public CredentialRepository(CosmosDbContext context, ILogger<CredentialRepository> logger)
        : base(context, logger, ContainerNames.Credentials)
    {
    }

    protected override PartitionKey GetPartitionKey(Credential entity) => new(entity.UserId);

    protected override string GetPartitionKeyPath() => "/userId";

    public async Task<IReadOnlyList<Credential>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId = @userId AND c.isActive = true ORDER BY c.registeredAt DESC")
            .WithParameter("@userId", userId);

        return await QueryAsync(query, userId, cancellationToken);
    }

    public async Task<Credential?> GetByCredentialIdAsync(
        byte[] credentialId,
        CancellationToken cancellationToken = default)
    {
        var base64CredentialId = Convert.ToBase64String(credentialId);

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.credentialIdBase64 = @credentialId")
            .WithParameter("@credentialId", base64CredentialId);

        var results = await QueryAsync(query, cancellationToken: cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<Credential?> GetByUserHandleAsync(
        byte[] userHandle,
        CancellationToken cancellationToken = default)
    {
        var base64UserHandle = Convert.ToBase64String(userHandle);

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userHandleBase64 = @userHandle")
            .WithParameter("@userHandle", base64UserHandle);

        var results = await QueryAsync(query, cancellationToken: cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<bool> CredentialIdExistsAsync(
        byte[] credentialId,
        CancellationToken cancellationToken = default)
    {
        var base64CredentialId = Convert.ToBase64String(credentialId);

        var query = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.credentialIdBase64 = @credentialId")
            .WithParameter("@credentialId", base64CredentialId);

        var container = await GetContainerAsync(cancellationToken);
        var iterator = container.GetItemQueryIterator<int>(query);
        var response = await iterator.ReadNextAsync(cancellationToken);

        return response.FirstOrDefault() > 0;
    }
}
