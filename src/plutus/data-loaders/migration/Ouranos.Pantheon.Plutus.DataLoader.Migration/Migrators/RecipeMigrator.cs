using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.DataLoader.Migration.Migrators;

public class RecipeMigrator
{
    private readonly ILogger<RecipeMigrator> _logger;
    private readonly IRepository<Market> _marketRepository;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IRepository<Recipe> _recipeRepository;

    public RecipeMigrator(
        ILogger<RecipeMigrator> logger,
        IMongoDatabaseManager mongoDatabaseManager,
        IRepository<Market> marketRepository,
        IRepository<Recipe> recipeRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(mongoDatabaseManager);
        Guard.Against.Null(marketRepository);
        Guard.Against.Null(recipeRepository);

        _logger = logger;
        _mongoDatabase = mongoDatabaseManager.GetDatabase<Migration>();
        _marketRepository = marketRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task MigrateAsync(
        IReadOnlyDictionary<Id<Market>, Id<Market>> marketIdMap,
        IReadOnlyDictionary<Id<Symbol>, Id<Symbol>> symbolIdMap,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Starting recipe migration...");

        var legacyRecipeCollection = _mongoDatabase.GetCollection<Recipe>("recipes");
        var allLegacyRecipes = await legacyRecipeCollection.Find(Builders<Recipe>.Filter.Empty)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {count} legacy recipes to migrate.", allLegacyRecipes.Count);

        if (allLegacyRecipes.Count != 0)
        {
            var markets = await _marketRepository.ReadAll(cancellationToken);
            var marketDictionary = markets.ToDictionary(m => m.Id, m => m);

            foreach (var legacyRecipe in allLegacyRecipes)
            {
                var marketId = marketIdMap[legacyRecipe.MarketId];

                var inputs = legacyRecipe.Inputs.Select(i =>
                {
                    if (!symbolIdMap.TryGetValue(i.SymbolId, out var newSymbolId))
                    {
                        throw new InvalidOperationException($"Could not find mapping for legacy symbol '{i.SymbolId}' in recipe '{legacyRecipe.Name}' inputs.");
                    }
                    return i with { SymbolId = newSymbolId };
                }).ToList();

                var outputs = legacyRecipe.Outputs.Select(o =>
                {
                    if (!symbolIdMap.TryGetValue(o.SymbolId, out var newSymbolId))
                    {
                        throw new InvalidOperationException($"Could not find mapping for legacy symbol '{o.SymbolId}' in recipe '{legacyRecipe.Name}' outputs.");
                    }
                    return o with { SymbolId = newSymbolId };
                }).ToList();

                var recipe = Recipe.Create(
                    new Id<Recipe>(Guid.NewGuid().ToString()),
                    marketDictionary[marketId],
                    legacyRecipe.Name,
                    legacyRecipe.Cost,
                    inputs,
                    outputs
                );

                await _recipeRepository.Create(recipe, cancellationToken);
            }

            await _recipeRepository.SaveChanges(cancellationToken);
        }

        _logger.LogInformation("Completed recipe migration.");
    }
}
