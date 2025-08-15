using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Queries.Recipes.GetRecipeTrades;
using Ouranos.Pantheon.Plutus.Service.Application.Queries.Recipes.GetRecipeTrades.Models;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Application.Tests.Queries.Recipes.GetRecipeTrades;

public sealed class GetRecipeTradesHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetRecipeTradesHandler _handler;
    private readonly ILogger<GetRecipeTradesHandler> _logger = Substitute.For<ILogger<GetRecipeTradesHandler>>();
    private readonly IQueryExecutor _queryExecutor = Substitute.For<IQueryExecutor>();
    private readonly IPlutusUnitOfWork _unitOfWork = Substitute.For<IPlutusUnitOfWork>();

    public GetRecipeTradesHandlerTests()
    {
        _handler = new GetRecipeTradesHandler(
            _logger,
            _unitOfWork,
            _queryExecutor
        );
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnRecipeTrades()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var recipe = _fixture.Build<Recipe>().With(x => x.MarketId, marketId).Create();
        var trade = _fixture.Create<Trade>();
        var query = new GetRecipeTradesInput(marketId, 3600);

        _unitOfWork.Recipes
            .ReadAll(Arg.Any<Expression<Func<Recipe, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([recipe]);

        _unitOfWork.Trades.AsQueryable(Arg.Any<CancellationToken>())
            .Returns(
                new List<Trade>
                {
                    trade
                }.AsQueryable()
            );

        _queryExecutor
            .ToList(Arg.Any<IQueryable<SymbolPrice>>(), Arg.Any<CancellationToken>())
            .Returns(
                recipe.Inputs.Select(i => _fixture.Build<SymbolPrice>().With(x => x.Id, i.SymbolId).Create())
                    .Union(
                        recipe.Outputs.Select(o => _fixture.Build<SymbolPrice>().With(x => x.Id, o.SymbolId).Create())
                    )
                    .ToList()
            );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<WrapperResponse<IQueryable<GetRecipeTradesResponse>>>();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(1);

        await _unitOfWork.Recipes.Received(1).ReadAll(
            Arg.Any<Expression<Func<Recipe, bool>>>(),
            Arg.Any<CancellationToken>()
        );

        _unitOfWork.Trades.Received(1).AsQueryable(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoRecipesFound_ShouldReturnEmptyList()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var query = new GetRecipeTradesInput(marketId, 3600);

        _unitOfWork.Recipes
            .ReadAll(Arg.Any<Expression<Func<Recipe, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<WrapperResponse<IQueryable<GetRecipeTradesResponse>>>();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var query = new GetRecipeTradesInput(marketId, 3600);
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}