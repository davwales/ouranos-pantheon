using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.Postgres;
using Ouranos.Pantheon.Hermes.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Hermes.Service.Domain.Assistants;
using Ouranos.Pantheon.Hermes.Service.Infra.Postgres.Common;

namespace Ouranos.Pantheon.Hermes.Service.Infra.Postgres;

public static class PostgresModule
{
    public static IServiceCollection AddPostgresModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddCorePostgresModule<HermesDbContext>(
                configuration,
                typeof(PostgresModule).Assembly
            )
            .AddRepository<HermesDbContext, Assistant>()
            .AddTransient<IHermesUnitOfWork, HermesUnitOfWork>();
    }

    public static async Task<IServiceProvider> ApplyPostgresMigrations(
        this IServiceProvider provider
    )
    {
        return await provider.ApplyCorePostgresMigrations<HermesDbContext>();
    }
}