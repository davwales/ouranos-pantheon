using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Core.Infra.Postgres.Common;

namespace Ouranos.Pantheon.Core.Infra.Postgres;

public static class PostgresModule
{
    public static IServiceCollection AddCorePostgresModule<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationAssembly = null
    ) where TContext : OuranosDbContext
    {
        migrationAssembly ??= Assembly.GetCallingAssembly();

        services.TryAddScoped<IQueryExecutor, QueryExecutor>();

        return services
            .Configure<PostgresOptions>(configuration.GetSection(PostgresOptions.SectionName))
            .AddDbContextFactory<TContext>((sp, options) =>
                {
                    var postgresOptions = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;

                    options
                        .UseNpgsql(
                            postgresOptions.GetConnectionString(),
                            npgsqlOptions =>
                            {
                                npgsqlOptions.MigrationsAssembly(migrationAssembly.GetName().Name);
                                npgsqlOptions.CommandTimeout(postgresOptions.CommandTimeout);

                                npgsqlOptions.EnableRetryOnFailure(
                                    postgresOptions.MaxRetries,
                                    TimeSpan.FromSeconds(postgresOptions.MaxRetryDelaySeconds),
                                    null
                                );
                            }
                        )
                        .UseSnakeCaseNamingConvention();
                }
            );
    }

    public static async Task<IServiceProvider> ApplyCorePostgresMigrations<TContext>(
        this IServiceProvider provider
    ) where TContext : OuranosDbContext
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.MigrateAsync();
        return provider;
    }

    public static IServiceCollection AddRepository<TContext, TEntity>(
        this IServiceCollection services
    ) where TContext : OuranosDbContext where TEntity : BaseEntity<Id<TEntity>>
    {
        return services.AddScoped<IRepository<TEntity>, Repository<TContext, TEntity>>();
    }
}