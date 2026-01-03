using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence;

/// <summary>
/// Unit of work implementation for Cosmos DB.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CosmosDbContext _context;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<UnitOfWork> _logger;

    private IFamilyRepository? _families;
    private IUserRepository? _users;
    private IDeviceRepository? _devices;
    private IEventRepository? _events;
    private IChoreRepository? _chores;
    private ICredentialRepository? _credentials;
    private IInvitationRepository? _invitations;
    private IOtpTokenRepository? _otpTokens;
    private IRefreshTokenRepository? _refreshTokens;
    private ICalendarConnectionRepository? _calendarConnections;

    public UnitOfWork(
        CosmosDbContext context,
        ILoggerFactory loggerFactory)
    {
        _context = context;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<UnitOfWork>();
    }

    public IFamilyRepository Families => _families ??=
        new FamilyRepository(_context, _loggerFactory.CreateLogger<FamilyRepository>());

    public IUserRepository Users => _users ??=
        new UserRepository(_context, _loggerFactory.CreateLogger<UserRepository>());

    public IDeviceRepository Devices => _devices ??=
        new DeviceRepository(_context, _loggerFactory.CreateLogger<DeviceRepository>());

    public IEventRepository Events => _events ??=
        new EventRepository(_context, _loggerFactory.CreateLogger<EventRepository>());

    public IChoreRepository Chores => _chores ??=
        new ChoreRepository(_context, _loggerFactory.CreateLogger<ChoreRepository>());

    public ICredentialRepository Credentials => _credentials ??=
        new CredentialRepository(_context, _loggerFactory.CreateLogger<CredentialRepository>());

    public IInvitationRepository Invitations => _invitations ??=
        new InvitationRepository(_context, _loggerFactory.CreateLogger<InvitationRepository>());

    public IOtpTokenRepository OtpTokens => _otpTokens ??=
        new OtpTokenRepository(_context, _loggerFactory.CreateLogger<OtpTokenRepository>());

    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??=
        new RefreshTokenRepository(_context, _loggerFactory.CreateLogger<RefreshTokenRepository>());

    public ICalendarConnectionRepository CalendarConnections => _calendarConnections ??=
        new CalendarConnectionRepository(_context, _loggerFactory.CreateLogger<CalendarConnectionRepository>());

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Cosmos DB saves immediately on each operation
        // This is a no-op but maintains the pattern for compatibility
        return Task.FromResult(0);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Cosmos DB transactional batches are scoped per partition
        // Full cross-partition transactions require Saga pattern
        _logger.LogDebug("BeginTransaction called - Cosmos DB uses partition-scoped transactions");
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("CommitTransaction called");
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("RollbackTransaction called - manual cleanup may be required");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Resources are managed by the DI container
    }
}
