using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Commands.Markets.UpdateMarket;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.Service.Application.Tests.Commands.Markets.UpdateMarket;

public sealed class UpdateMarketHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateMarketHandler _handler;
    private readonly ILogger<UpdateMarketHandler> _logger = Substitute.For<ILogger<UpdateMarketHandler>>();
    private readonly IRepository<Market> _marketRepository = Substitute.For<IRepository<Market>>();

    public UpdateMarketHandlerTests()
    {
        _handler = new UpdateMarketHandler(_logger, _marketRepository);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateMarketAndReturnId()
    {
        // Arrange
        var command = _fixture.Create<UpdateMarketInput>();
        var existingMarket = new Market(
            command.MarketId,
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        _marketRepository
            .Read(command.MarketId, Arg.Any<CancellationToken>())
            .Returns(existingMarket);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Market>>();
        result.Id.ShouldBe(command.MarketId);

        existingMarket.Name.ShouldBe(command.Name);
        existingMarket.Taxes.ShouldBe(command.Taxes);

        await _marketRepository.Received(1).Update(
            Arg.Is<Market>(m =>
                m.Id == command.MarketId &&
                m.Name == command.Name &&
                m.Taxes == command.Taxes
            ),
            Arg.Any<CancellationToken>()
        );
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