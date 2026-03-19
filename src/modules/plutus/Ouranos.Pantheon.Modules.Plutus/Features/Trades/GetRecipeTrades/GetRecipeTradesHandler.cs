using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades;

public sealed class GetRecipeTradesHandler
    : IPantheonHandler<GetRecipeTradesInput, PagedResponse<GetRecipeTradesResponse>>
{
    private static readonly FilterBuilder<GetRecipeTradesResponse> FilterBuilder =
        new FilterBuilder<GetRecipeTradesResponse>()
            .On(nameof(GetRecipeTradesResponse.RecipeName), x => x.RecipeName)
            .On(nameof(GetRecipeTradesResponse.LatestBuyPrice), x => x.LatestBuyPrice)
            .On(nameof(GetRecipeTradesResponse.LatestSellPrice), x => x.LatestSellPrice)
            .On(nameof(GetRecipeTradesResponse.LatestMargin), x => x.LatestMargin)
            .On(nameof(GetRecipeTradesResponse.AverageBuyPrice), x => x.AverageBuyPrice)
            .On(nameof(GetRecipeTradesResponse.AverageSellPrice), x => x.AverageSellPrice)
            .On(nameof(GetRecipeTradesResponse.AverageMargin), x => x.AverageMargin);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetRecipeTradesHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetRecipeTradesHandler(
        ILogger<GetRecipeTradesHandler> logger,
        PlutusDbContext dbContext,
        IOptions<QueryOptions> queryOptions
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(queryOptions);

        _logger = logger;
        _dbContext = dbContext;
        _queryOptions = queryOptions;
    }

    public async Task<PagedResponse<GetRecipeTradesResponse>> Handle(
        GetRecipeTradesInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get recipe trades query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(input.Take, nameof(input.Take), limits.MinPageSize, limits.MaxPageSize);

        if (input.Seconds.HasValue)
        {
            Guard.Against.NegativeOrZero(input.Seconds.Value);
        }

        var recipes = await _dbContext.Recipes
            .AsNoTracking()
            .Where(r => r.MarketId == input.MarketId)
            .ToListAsync(cancellationToken);

        if (recipes.Count == 0)
        {
            _logger.LogDebug("No recipes found for market '{marketId}'.", input.MarketId);
            return new PagedResponse<GetRecipeTradesResponse>(
                [],
                0,
                input.Skip,
                input.Take
            );
        }

        var symbolIds = recipes
            .SelectMany(r => r.Inputs.Select(x => x.SymbolId))
            .Union(recipes.SelectMany(r => r.Outputs.Select(x => x.SymbolId)))
            .Distinct()
            .ToList();

        DateTimeOffset? since = input.Seconds.HasValue
            ? DateTimeOffset.UtcNow - TimeSpan.FromSeconds(input.Seconds.Value)
            : null;

        var prices = await GetSymbolPrices(symbolIds, since, cancellationToken);

        var validRecipes = recipes
            .Where(r =>
                r.Inputs.All(x => prices.ContainsKey(x.SymbolId)) &&
                r.Outputs.All(x => prices.ContainsKey(x.SymbolId))
            )
            .ToList();

        var results = validRecipes
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
            .ToList();

        var filtered = results.AsQueryable().FilterBy(input.Filter, FilterBuilder);
        var totalCount = filtered.Count();

        var page = filtered
            .SortBy(
                input.SortField,
                input.SortDirection,
                builder => builder
                    .On(nameof(GetRecipeTradesResponse.RecipeName), x => x.RecipeName)
                    .On(nameof(GetRecipeTradesResponse.LatestBuyPrice), x => x.LatestBuyPrice)
                    .On(nameof(GetRecipeTradesResponse.LatestSellPrice), x => x.LatestSellPrice)
                    .On(nameof(GetRecipeTradesResponse.LatestMargin), x => x.LatestMargin)
                    .On(nameof(GetRecipeTradesResponse.AverageBuyPrice), x => x.AverageBuyPrice)
                    .On(nameof(GetRecipeTradesResponse.AverageSellPrice), x => x.AverageSellPrice)
                    .On(nameof(GetRecipeTradesResponse.AverageMargin), x => x.AverageMargin)
                    .Default(x => x.AverageMargin)
            )
            .Paginate(input.Skip, input.Take)
            .ToList();

        _logger.LogDebug("Successfully handled get recipe trades query.");
        return new PagedResponse<GetRecipeTradesResponse>(page, totalCount, input.Skip, input.Take);
    }

    private async Task<Dictionary<Id<Symbol>, IntermediatePrice>> GetSymbolPrices(
        IReadOnlyCollection<Id<Symbol>> symbolIds,
        DateTimeOffset? since,
        CancellationToken cancellationToken
    )
    {
        var priceQuery = _dbContext.Trades
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
