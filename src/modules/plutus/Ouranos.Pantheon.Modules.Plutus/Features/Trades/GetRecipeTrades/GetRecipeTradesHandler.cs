using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades;

public sealed class GetRecipeTradesHandler
    : QueryHandler<GetRecipeTradesInput, WrapperResponse<IQueryable<GetRecipeTradesResponse>>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetRecipeTradesHandler> _logger;

    public GetRecipeTradesHandler(
        ILogger<GetRecipeTradesHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<WrapperResponse<IQueryable<GetRecipeTradesResponse>>> Handle(
        GetRecipeTradesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get recipe trades query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var recipes = await _dbContext.Recipes
            .Where(r => r.MarketId == query.MarketId)
            .ToListAsync(cancellationToken);

        if (recipes.Count == 0)
        {
            _logger.LogDebug("No recipes found for market '{marketId}'.", query.MarketId);
            return new WrapperResponse<IQueryable<GetRecipeTradesResponse>>(
                new List<GetRecipeTradesResponse>().AsQueryable()
            );
        }

        var symbolIds = recipes
            .SelectMany(r => r.Inputs.Select(x => x.SymbolId))
            .Union(recipes.SelectMany(r => r.Outputs.Select(x => x.SymbolId)))
            .Distinct()
            .ToList();

        DateTimeOffset? since = query.Seconds.HasValue
            ? DateTimeOffset.UtcNow - TimeSpan.FromSeconds(query.Seconds.Value)
            : null;

        var prices = await GetSymbolPrices(symbolIds, since, cancellationToken);

        var validRecipes = recipes
            .Where(r =>
                r.Inputs.All(x => prices.ContainsKey(x.SymbolId)) &&
                r.Outputs.All(x => prices.ContainsKey(x.SymbolId))
            )
            .ToList();

        var response = validRecipes
            .Select(r => new
            {
                r.Id,
                r.Name,
                LatestBuyPrice = r.Inputs.Sum(i => prices[i.SymbolId].LatestPrice * i.Quantity) + r.Cost,
                LatestSellPrice = r.Outputs.Sum(i => prices[i.SymbolId].LatestPrice * i.Quantity),
                AverageBuyPrice = r.Inputs.Sum(i => prices[i.SymbolId].AveragePrice * i.Quantity) + r.Cost,
                AverageSellPrice = r.Outputs.Sum(i => prices[i.SymbolId].AveragePrice * i.Quantity)
            }
            )
            .Union(
                recipes.Except(validRecipes).Select(r => new
                {
                    r.Id,
                    r.Name,
                    LatestBuyPrice = (decimal)0,
                    LatestSellPrice = (decimal)0,
                    AverageBuyPrice = (decimal)0,
                    AverageSellPrice = (decimal)0
                }
                )
            )
            .Select(x => new GetRecipeTradesResponse(
                    x.Id,
                    x.Name,
                    x.LatestBuyPrice,
                    x.LatestSellPrice,
                    x.LatestSellPrice - x.LatestBuyPrice,
                    x.AverageBuyPrice,
                    x.AverageSellPrice,
                    x.AverageSellPrice - x.AverageBuyPrice
                )
            )
            .AsQueryable();

        _logger.LogDebug("Successfully handled get recipe trades query.");
        return new WrapperResponse<IQueryable<GetRecipeTradesResponse>>(response);
    }

    private async Task<Dictionary<Id<Symbol>, IntermediatePrice>> GetSymbolPrices(
        IReadOnlyCollection<Id<Symbol>> symbolIds,
        DateTimeOffset? since,
        CancellationToken cancellationToken
    )
    {
        var priceQuery = _dbContext.Trades
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
            .Select(x => new SymbolPrice(
                    x.SymbolId,
                    x.TotalSpent / x.Volume,
                    x.LatestPrice
                )
            );

        var prices = await priceQuery.ToListAsync(cancellationToken);

        return prices.ToDictionary(
            x => x.Id,
            x => new IntermediatePrice(
                x.AveragePrice,
                x.LatestPrice
            )
        );
    }

    private record SymbolPrice(
        Id<Symbol> Id,
        decimal AveragePrice,
        decimal LatestPrice
    );

    private record IntermediatePrice(
        decimal AveragePrice,
        decimal LatestPrice
    );
}
