using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Recipes.GetRecipeTrades.Models;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Recipes.GetRecipeTrades;

public sealed class GetRecipeTradesHandler
    : QueryHandler<GetRecipeTradesInput, WrapperResponse<IQueryable<GetRecipeTradesResponse>>>
{
    private readonly ILogger<GetRecipeTradesHandler> _logger;
    private readonly IQueryExecutor _queryExecutor;
    private readonly IRepository<Recipe> _recipeRepository;
    private readonly IRepository<Trade> _tradeRepository;

    public GetRecipeTradesHandler(
        ILogger<GetRecipeTradesHandler> logger,
        IRepository<Recipe> recipeRepository,
        IRepository<Trade> tradeRepository,
        IQueryExecutor queryExecutor
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(recipeRepository);
        Guard.Against.Null(tradeRepository);
        Guard.Against.Null(queryExecutor);

        _logger = logger;
        _recipeRepository = recipeRepository;
        _tradeRepository = tradeRepository;
        _queryExecutor = queryExecutor;
    }

    public override async Task<WrapperResponse<IQueryable<GetRecipeTradesResponse>>> Handle(
        GetRecipeTradesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get recipe trades query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var recipes = await _recipeRepository.ReadAll(
            r => r.MarketId == query.MarketId,
            cancellationToken
        );

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

        var prices = await GetSymbolPrices(
            symbolIds,
            query.Seconds,
            cancellationToken
        );

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
        double? seconds,
        CancellationToken cancellationToken
    )
    {
        DateTimeOffset? since = seconds.HasValue
            ? DateTimeOffset.UtcNow - TimeSpan.FromSeconds(seconds.Value)
            : null;

        var priceQuery = _tradeRepository
            .AsQueryable(cancellationToken)
            .Where(x =>
                (since == null || x.CreatedAt >= since) &&
                symbolIds.Contains(x.SymbolId)
            )
            .OrderByDescending(x => x.CreatedAt)
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

        var prices = await _queryExecutor.ToList(
            priceQuery,
            cancellationToken
        );

        return prices.ToDictionary(
            x => x.Id,
            x => new IntermediatePrice(
                x.AveragePrice,
                x.LatestPrice
            )
        );
    }
}