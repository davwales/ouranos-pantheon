using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Application.Commands.Markets.CreateMarket;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Tests.Commands.Markets.CreateMarket;

public sealed class CreateMarketHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateMarketHandler _handler;
    private readonly ILogger<CreateMarketHandler> _logger = Substitute.For<ILogger<CreateMarketHandler>>();
    private readonly IRepository<Market> _marketRepository = Substitute.For<IRepository<Market>>();

    public CreateMarketHandlerTests()
    {
        _handler = new CreateMarketHandler(_logger, _marketRepository);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateMarketAndReturnId()
    {
        // Arrange
        var command = _fixture.Create<CreateMarketInput>();
        var expectedId = new Id<Market>(_fixture.Create<string>());

        _marketRepository.CreateId().Returns(expectedId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Market>>();
        result.Id.ShouldBe(expectedId);

        await _marketRepository.Received(1).Create(
            Arg.Is<Market>(m =>
                m.Id == expectedId &&
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
        var command = _fixture.Create<CreateMarketInput>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var create = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await create.ShouldThrowAsync<OperationCanceledException>();
    }
}