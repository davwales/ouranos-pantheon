using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe;

public sealed class GetRecipeHandler : IPantheonHandler<GetRecipeInput, GetRecipeResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetRecipeHandler> _logger;

    public GetRecipeHandler(
        ILogger<GetRecipeHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetRecipeResponse> Handle(
        GetRecipeInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get recipe query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var recipe = await _dbContext.Recipes
            .Include(r => r.Inputs)
            .Include(r => r.Outputs)
            .FirstOrDefaultAsync(r => r.Id == query.RecipeId, cancellationToken);

        Guard.Against.NotFound(query.RecipeId, recipe);

        var symbolIds = recipe.Inputs.Select(x => x.SymbolId)
            .Union(recipe.Outputs.Select(x => x.SymbolId))
            .Distinct()
            .ToList();

        var prices = await GetSymbolPrices(symbolIds, query.TimeFrame, cancellationToken);

        _logger.LogDebug("Successfully handled get recipe request.");
        return new GetRecipeResponse(
            recipe.Id,
            recipe.MarketId,
            recipe.Name,
            recipe.Cost,
            recipe.Inputs.Select(c => ToPricedComponent(c, prices)).ToList(),
            recipe.Outputs.Select(c => ToPricedComponent(c, prices)).ToList()
        );
    }

    private static PricedRecipeComponent ToPricedComponent(
        RecipeComponent component,
        Dictionary<Id<Symbol>, SymbolPriceDataDto> prices
    )
    {
        if (prices.TryGetValue(component.SymbolId, out var price))
        {
            return new PricedRecipeComponent(
                component.SymbolId,
                component.Name,
                component.Quantity,
                price.LatestPrice,
                price.AveragePrice,
                price.LatestPrice * component.Quantity,
                price.Volume
            );
        }

        return new PricedRecipeComponent(
            component.SymbolId,
            component.Name,
            component.Quantity,
            null,
            null,
            null,
            null
        );
    }

    private async Task<Dictionary<Id<Symbol>, SymbolPriceDataDto>> GetSymbolPrices(
        IReadOnlyCollection<Id<Symbol>> symbolIds,
        TimeFrame timeFrame,
        CancellationToken cancellationToken
    )
    {
        DateTimeOffset? since = timeFrame.ToTimeSpan() is { } span
            ? DateTimeOffset.UtcNow - span
            : null;

        var prices = await _dbContext.Trades
            .AsNoTracking()
            .Where(x =>
                (since == null || x.Timestamp >= since) &&
                symbolIds.Contains(x.SymbolId)
            )
            .OrderByDescending(x => x.Timestamp)
            .GroupBy(x => x.SymbolId)
            .Select(g => new
            {
                SymbolId = g.Key,
                TotalSpent = g.Sum(x => x.Price * x.Volume),
                Volume = g.Sum(x => x.Volume),
                LatestPrice = g.First().Price
            }
            )
            .Select(x => new { x.SymbolId, AveragePrice = x.TotalSpent / x.Volume, x.LatestPrice, x.Volume })
            .ToListAsync(cancellationToken);

        return prices.ToDictionary(
            x => x.SymbolId,
            x => new SymbolPriceDataDto(x.LatestPrice, x.AveragePrice, x.Volume)
        );
    }
}
