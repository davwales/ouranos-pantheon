using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Signals.GetSignalRankings;

public sealed class GetSignalRankingsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogger<GetSignalRankingsHandler> _logger = Substitute.For<ILogger<GetSignalRankingsHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetSignalRankingsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
    }

    private GetSignalRankingsHandler BuildHandler(IEnumerable<ISignalComputer>? computers = null) =>
        new(_logger, _dbContext, Options.Create(new QueryOptions()), computers ?? []);

    [Fact]
    public async Task Handle_WhenNoSignals_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var handler = BuildHandler();
        var query = new GetSignalRankingsInput(
            new Id<Market>(_fixture.Create<string>()),
            Take: 10
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var handler = BuildHandler();
        var query = new GetSignalRankingsInput(
            new Id<Market>(_fixture.Create<string>()),
            Take: 10
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
