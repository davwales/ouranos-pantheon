using System.Reflection;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Converters;
using Wolverine.Marten;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres;

public static class PostgresModule
{
    public static IServiceCollection AddCorePostgresModule<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationAssembly = null
    )
        where TContext : OuranosDbContext
    {
        migrationAssembly ??= Assembly.GetCallingAssembly();

        return services
            .Configure<PostgresOptions>(configuration.GetSection(PostgresOptions.SectionName))
            .AddDbContextFactory<TContext>(
                (sp, options) =>
                {
                    var postgresOptions = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
                    options.EnableSensitiveDataLogging(postgresOptions.EnableSensitiveDataLogging);

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

    public static IServiceCollection AddCoreMartenModule<TMarker>(
        this IServiceCollection services,
        IConfiguration configuration,
        string schemaName,
        Action<StoreOptions>? configure = null,
        Marten.Schema.IInitialData[]? initialData = null
    )
        where TMarker : class, IDocumentStore
    {
        services.Configure<PostgresOptions>(configuration.GetSection(PostgresOptions.SectionName));

        void ConfigureStore(StoreOptions options)
        {
            var postgresOptions =
                configuration.GetSection(PostgresOptions.SectionName).Get<PostgresOptions>()
                ?? new PostgresOptions();

            options.Connection(postgresOptions.GetConnectionString());
            options.DatabaseSchemaName = schemaName;

            configure?.Invoke(options);

            options.UseSystemTextJsonForSerialization(configure: jsonOptions =>
            {
                jsonOptions.Converters.Add(new IdJsonConverterFactory());
            });
        }

        services
            .AddMartenStore<TMarker>(ConfigureStore)
            .InitializeWith(initialData ?? [])
            .IntegrateWithWolverine()
            .ApplyAllDatabaseChangesOnStartup();

        return services;
    }

    public static async Task<IServiceProvider> ApplyCorePostgresMigrations<TContext>(
        this IServiceProvider provider
    )
        where TContext : OuranosDbContext
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.MigrateAsync();
        return provider;
    }
}
