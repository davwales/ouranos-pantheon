using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.Postgres;
using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

namespace Ouranos.Pantheon.Service.Hermes.Infra.Postgres;

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
            .AddRepository<HermesDbContext, Assistant>();
    }

    public static async Task<IServiceProvider> ApplyPostgresMigrations(
        this IServiceProvider provider
    )
    {
        return await provider.ApplyCorePostgresMigrations<HermesDbContext>();
    }
}