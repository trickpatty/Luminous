using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence;
using Luminous.Infrastructure.Persistence.Configuration;
using Luminous.Infrastructure.Persistence.Repositories;
using Luminous.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Luminous.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Cosmos DB settings
        services.Configure<CosmosDbSettings>(configuration.GetSection(CosmosDbSettings.SectionName));

        // Register Cosmos DB context
        services.AddSingleton<CosmosDbContext>();

        // Register repositories
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IChoreRepository, ChoreRepository>();
        services.AddScoped<ICredentialRepository, CredentialRepository>();
        services.AddScoped<IOtpTokenRepository, OtpTokenRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register services
        services.AddSingleton<IDateTimeService, DateTimeService>();

        return services;
    }
}
