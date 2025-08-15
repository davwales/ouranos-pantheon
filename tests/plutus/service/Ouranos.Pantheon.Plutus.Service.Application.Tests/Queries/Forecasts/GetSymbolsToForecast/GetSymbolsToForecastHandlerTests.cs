using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Queries.Forecasts.GetSymbolsToForecast;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Tests.Queries.Forecasts.GetSymbolsToForecast;

public sealed class GetSymbolsToForecastHandlerTests
{
    private readonly GetSymbolsToForecastHandler _handler;

    private readonly ILogger<GetSymbolsToForecastHandler> _logger =
        Substitute.For<ILogger<GetSymbolsToForecastHandler>>();

    private readonly IQueryExecutor _queryExecutor = Substitute.For<IQueryExecutor>();
    private readonly IPlutusUnitOfWork _unitOfWork = Substitute.For<IPlutusUnitOfWork>();

    public GetSymbolsToForecastHandlerTests()
    {
        _handler = new GetSymbolsToForecastHandler(
            _logger,
            _queryExecutor,
            _unitOfWork
        );
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnSymbols()
    {
        // Arrange
        var marketIds = new Fixture().CreateMany<Id<Market>>(3).ToList();
        var symbols = new Fixture().Build<Symbol>()
            .With(s => s.MarketId, () => marketIds[0])
            .CreateMany(5)
            .ToList();

        var query = new GetSymbolsToForecastInput();

        _unitOfWork.Markets
            .AsQueryable(Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Market>().AsQueryable());

        _queryExecutor
            .ToList(Arg.Any<IQueryable<Id<Market>>>(), Arg.Any<CancellationToken>())
            .Returns(marketIds);

        _unitOfWork.Symbols
            .ReadAll(Arg.Any<Expression<Func<Symbol, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(symbols);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<WrapperResponse<List<Symbol>>>();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBe(symbols);

        _unitOfWork.Markets.Received(1).AsQueryable(Arg.Any<CancellationToken>());

        await _queryExecutor.Received(1).ToList(
            Arg.Any<IQueryable<Id<Market>>>(),
            Arg.Any<CancellationToken>()
        );

        await _unitOfWork.Symbols.Received(1).ReadAll(
            Arg.Any<Expression<Func<Symbol, bool>>>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_WhenNoMarketsFound_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetSymbolsToForecastInput();

        _unitOfWork.Markets
            .AsQueryable(Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Market>().AsQueryable());

        _queryExecutor
            .ToList(Arg.Any<IQueryable<Id<Market>>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<WrapperResponse<List<Symbol>>>();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetSymbolsToForecastInput();
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}