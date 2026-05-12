using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.CreateSymbolGroup;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.CreateSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.SymbolGroups.CreateSymbolGroup;

public sealed class CreateSymbolGroupHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateSymbolGroupHandler _handler;
    private readonly ILogger<CreateSymbolGroupHandler> _logger = Substitute.For<
        ILogger<CreateSymbolGroupHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public CreateSymbolGroupHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new CreateSymbolGroupHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateSymbolGroupAndReturnId()
    {
        // Arrange
        var command = new CreateSymbolGroupInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<SymbolGroup>>();
        result.Id.ShouldNotBe(default);

        var group = await _dbContext.SymbolGroups.FindAsync(result.Id);
        group.ShouldNotBeNull();
        group.Name.ShouldBe(command.Name);
        group.MarketId.ShouldBe(command.MarketId);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreateSymbolGroupInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var create = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await create.ShouldThrowAsync<OperationCanceledException>();
    }
}
