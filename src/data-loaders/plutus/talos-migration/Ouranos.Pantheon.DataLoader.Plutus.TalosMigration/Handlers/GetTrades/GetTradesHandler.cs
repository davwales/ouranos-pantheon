using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Handlers.GetTrades;

public sealed class GetTradesHandler : QueryHandler<GetTradesInput, GetTradesResponse>
{
    private readonly ILogger<GetTradesHandler> _logger;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly FindOptions<TalosTrade> _options;

    private readonly string _talosTradesCollectionName;

    public GetTradesHandler(
        ILogger<GetTradesHandler> logger,
        IMongoDatabaseManager mongoDatabaseManager,
        IConfiguration configuration
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(mongoDatabaseManager);
        Guard.Against.Null(configuration);

        _logger = logger;
        _mongoDatabase = mongoDatabaseManager.GetDatabase<TalosTrade>();

        _options = new FindOptions<TalosTrade>
        {
            BatchSize = configuration.GetValue("Ouranos:BatchSize", 1000)
        };

        _talosTradesCollectionName = configuration
            .GetSection("Ouranos:Mongo:TalosTradesCollectionName")
            .Get<string>() ?? throw new InvalidOperationException("Invalid talos-trades collection configuration.");
    }

    protected override async Task<GetTradesResponse> Handle(
        GetTradesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get trades query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var collection = _mongoDatabase.GetCollection<TalosTrade>(_talosTradesCollectionName);
        var filter = Builders<TalosTrade>.Filter.Empty;
        var cursor = await collection.FindAsync(filter, _options, cancellationToken);
        var response = new GetTradesResponse(cursor);

        _logger.LogDebug("Successfully handled get trades query.");
        return response;
    }
}