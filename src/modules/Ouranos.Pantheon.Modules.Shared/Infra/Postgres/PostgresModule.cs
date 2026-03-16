using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres;

public static class PostgresModule
{
    public static IServiceCollection AddCorePostgresModule<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationAssembly = null
    ) where TContext : OuranosDbContext
    {
        migrationAssembly ??= Assembly.GetCallingAssembly();

        return services
            .Configure<PostgresOptions>(configuration.GetSection(PostgresOptions.SectionName))
            .AddDbContextFactory<TContext>((sp, options) =>
                {
                    var postgresOptions = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
                    options.EnableSensitiveDataLogging(postgresOptions.EnableSensitiveDataLogging);
                    options.UseLazyLoadingProxies();

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
}