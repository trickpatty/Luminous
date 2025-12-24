using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cosmos DB repository for Invitation entities.
/// </summary>
public sealed class InvitationRepository : CosmosRepositoryBase<Invitation>, IInvitationRepository
{
    public InvitationRepository(CosmosDbContext context, ILogger<InvitationRepository> logger)
        : base(context, logger, ContainerNames.Invitations)
    {
    }

    protected override PartitionKey GetPartitionKey(Invitation entity) => new(entity.FamilyId);

    protected override string GetPartitionKeyPath() => "/familyId";

    public new async Task<Invitation?> GetByIdAsync(
        string invitationId,
        string? familyId,
        CancellationToken cancellationToken = default)
    {
        return await base.GetByIdAsync(invitationId, familyId, cancellationToken);
    }

    public async Task<Invitation?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.code = @code")
            .WithParameter("@code", code);

        var results = await QueryAsync(query, cancellationToken: cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<Invitation>> GetByFamilyIdAsync(
        string familyId,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.familyId = @familyId ORDER BY c.createdAt DESC")
            .WithParameter("@familyId", familyId);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<IReadOnlyList<Invitation>> GetByFamilyIdAndStatusAsync(
        string familyId,
        InvitationStatus status,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.familyId = @familyId AND c.status = @status ORDER BY c.createdAt DESC")
            .WithParameter("@familyId", familyId)
            .WithParameter("@status", (int)status);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<Invitation?> GetPendingByEmailAndFamilyIdAsync(
        string email,
        string familyId,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.familyId = @familyId AND c.email = @email AND c.status = @status")
            .WithParameter("@familyId", familyId)
            .WithParameter("@email", normalizedEmail)
            .WithParameter("@status", (int)InvitationStatus.Pending);

        var results = await QueryAsync(query, familyId, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<bool> HasPendingInvitationAsync(
        string email,
        string familyId,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var query = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.familyId = @familyId AND c.email = @email AND c.status = @status")
            .WithParameter("@familyId", familyId)
            .WithParameter("@email", normalizedEmail)
            .WithParameter("@status", (int)InvitationStatus.Pending);

        var container = await GetContainerAsync(cancellationToken);
        var queryOptions = new QueryRequestOptions { PartitionKey = new PartitionKey(familyId) };
        var iterator = container.GetItemQueryIterator<int>(query, requestOptions: queryOptions);
        var response = await iterator.ReadNextAsync(cancellationToken);

        return response.FirstOrDefault() > 0;
    }
}
