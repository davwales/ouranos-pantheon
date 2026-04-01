using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetAllTrades;

public sealed class GetAllTradesHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllTradesHandler _handler;
    private readonly ILogger<GetAllTradesHandler> _logger = Substitute.For<ILogger<GetAllTradesHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetAllTradesHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetAllTradesHandler(_logger, _dbContext, Options.Create(new QueryOptions()));
    }

    [Fact]
    public async Task Handle_WhenNoTrades_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllTradesInput(Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllTradesInput(Take: 10);
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
