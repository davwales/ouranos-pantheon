using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using TickerQ.EntityFrameworkCore.DbContextFactory;

namespace Ouranos.Pantheon.Modules.Shared;

public class TickerQDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TickerQDbContext>
{
    public TickerQDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var postgresOptions = configuration.GetSection(PostgresOptions.SectionName).Get<PostgresOptions>()
                              ?? throw new InvalidOperationException("Postgres options not found.");

        var optionsBuilder = new DbContextOptionsBuilder<TickerQDbContext>();
        optionsBuilder
            .UseNpgsql(
                postgresOptions.GetConnectionString(),
                o => o.MigrationsAssembly(typeof(TickerQDesignTimeDbContextFactory).Assembly.FullName)
            )
            .UseSnakeCaseNamingConvention();

        return new TickerQDbContext(optionsBuilder.Options);
    }
}
