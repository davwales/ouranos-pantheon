using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Plutus.DataLoader.Migration.Migrators;

namespace Ouranos.Pantheon.Plutus.DataLoader.Migration;

public sealed class Migration : IMigration
{
    private readonly ILogger<Migration> _logger;
    private readonly MarketMigrator _marketMigrator;
    private readonly SymbolMigrator _symbolMigrator;
    private readonly TradeMigrator _tradeMigrator;
    private readonly RecipeMigrator _recipeMigrator;

    public Migration(
        ILogger<Migration> logger,
        MarketMigrator marketMigrator,
        SymbolMigrator symbolMigrator,
        TradeMigrator tradeMigrator,
        RecipeMigrator recipeMigrator
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(marketMigrator);
        Guard.Against.Null(symbolMigrator);
        Guard.Against.Null(tradeMigrator);
        Guard.Against.Null(recipeMigrator);

        _logger = logger;
        _marketMigrator = marketMigrator;
        _symbolMigrator = symbolMigrator;
        _tradeMigrator = tradeMigrator;
        _recipeMigrator = recipeMigrator;
    }

    public async Task Migrate(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting data migration...");
        cancellationToken.ThrowIfCancellationRequested();

        var marketIdMap = await _marketMigrator.Migrate(cancellationToken);
        var symbolIdMap = await _symbolMigrator.MigrateAsync(marketIdMap, cancellationToken);
        await _recipeMigrator.MigrateAsync(marketIdMap, symbolIdMap, cancellationToken);
        await _tradeMigrator.MigrateAsync(symbolIdMap, cancellationToken);

        _logger.LogInformation("Completed data migration.");
    }
}
