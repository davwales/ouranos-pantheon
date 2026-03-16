using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Postgres;

public abstract class OuranosDesignTimeDbContextFactory<TContext>(params string[] configFiles)
    : IDesignTimeDbContextFactory<TContext>
    where TContext : OuranosDbContext
{
    private readonly string[] _configFiles = configFiles.Length == 0
        ? ["appsettings.json", "appsettings.Development.json"]
        : configFiles;

    public TContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder();
        foreach (var file in _configFiles)
        {
            configurationBuilder.AddJsonFile(file);
        }

        var configuration = configurationBuilder.Build();

        var postgresOptions = configuration.GetSection(PostgresOptions.SectionName).Get<PostgresOptions>()
                              ?? throw new InvalidOperationException("Postgres options not found.");

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.UseNpgsql(postgresOptions.GetConnectionString());
        optionsBuilder.UseSnakeCaseNamingConvention();

        return CreateDbContext(optionsBuilder);
    }

    protected abstract TContext CreateDbContext(DbContextOptionsBuilder<TContext> optionsBuilder);
}