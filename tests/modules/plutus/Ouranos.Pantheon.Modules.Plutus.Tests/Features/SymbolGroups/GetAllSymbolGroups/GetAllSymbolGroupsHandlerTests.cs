using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.SymbolGroups.GetAllSymbolGroups;

public sealed class GetAllSymbolGroupsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllSymbolGroupsHandler _handler;
    private readonly ILogger<GetAllSymbolGroupsHandler> _logger = Substitute.For<ILogger<GetAllSymbolGroupsHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetAllSymbolGroupsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetAllSymbolGroupsHandler(_logger, _dbContext, Options.Create(new QueryOptions()));
    }

    [Fact]
    public async Task Handle_WhenGroupsExist_ShouldReturnPagedGroups()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var groups = Enumerable.Range(0, 2).Select(_ => SymbolGroup.Create(
            new Id<SymbolGroup>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            market.Id
        )).ToArray();

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(groups);

        var query = new GetAllSymbolGroupsInput(market.Id, Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(2);
        result.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenNoGroups_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var query = new GetAllSymbolGroupsInput(
            new Id<Market>(_fixture.Create<string>()),
            Take: 10
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllSymbolGroupsInput(
            new Id<Market>(_fixture.Create<string>()),
            Take: 10
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
