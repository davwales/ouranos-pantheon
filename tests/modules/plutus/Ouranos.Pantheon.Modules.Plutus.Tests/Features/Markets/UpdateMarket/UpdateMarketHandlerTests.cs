using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.UpdateMarket;

public sealed class UpdateMarketHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateMarketHandler _handler;
    private readonly ILogger<UpdateMarketHandler> _logger = Substitute.For<
        ILogger<UpdateMarketHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public UpdateMarketHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new UpdateMarketHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateMarketAndReturnId()
    {
        // Arrange
        var existingMarket = _fixture.Create<Market>();
        await _dbContext.SeedData(existingMarket);

        var command = new UpdateMarketInput(
            existingMarket.Id,
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Market>>();
        result.Id.ShouldBe(command.MarketId);

        var updatedMarket = await _dbContext.Markets.FindAsync(command.MarketId);
        updatedMarket.ShouldNotBeNull();
        updatedMarket.Name.ShouldBe(command.Name);
        updatedMarket.Taxes.ShouldBe(command.Taxes);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = _fixture.Create<UpdateMarketInput>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var update = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await update.ShouldThrowAsync<OperationCanceledException>();
    }
}
