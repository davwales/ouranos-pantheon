using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.CreateMarket;

public sealed class CreateMarketHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateMarketHandler _handler;
    private readonly ILogger<CreateMarketHandler> _logger = Substitute.For<ILogger<CreateMarketHandler>>();
    private readonly PlutusDbContext _dbContext;

    public CreateMarketHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new CreateMarketHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateMarketAndReturnId()
    {
        // Arrange
        var command = _fixture.Create<CreateMarketInput>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Market>>();
        result.Id.ShouldNotBe(default);

        var market = await _dbContext.Markets.FindAsync(result.Id);
        market.ShouldNotBeNull();
        market.Name.ShouldBe(command.Name);
        market.Taxes.ShouldBe(command.Taxes);
        market.IsForecastingEnabled.ShouldBe(command.IsForecastingEnabled);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = _fixture.Create<CreateMarketInput>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var create = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await create.ShouldThrowAsync<OperationCanceledException>();
    }
}
