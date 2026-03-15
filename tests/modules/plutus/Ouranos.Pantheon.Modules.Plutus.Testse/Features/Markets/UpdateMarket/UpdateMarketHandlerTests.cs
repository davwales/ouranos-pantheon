using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.UpdateMarket;

public sealed class UpdateMarketHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateMarketHandler _handler;
    private readonly ILogger<UpdateMarketHandler> _logger = Substitute.For<ILogger<UpdateMarketHandler>>();
    private readonly PlutusDbContext _dbContext = Substitute.For<PlutusDbContext>(
        Substitute.For<DbContextOptions<PlutusDbContext>>()
    );

    public UpdateMarketHandlerTests()
    {
        _handler = new UpdateMarketHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateMarketAndReturnId()
    {
        // Arrange
        var command = _fixture.Create<UpdateMarketInput>();
        var existingMarket = Market.Create(
            command.MarketId,
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var DbSetMock = Substitute.For<DbSet<Market>>();
        _dbContext.Markets.Returns(DbSetMock);

        DbSetMock.FirstOrDefaultAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Market, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(existingMarket);

        _dbContext.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Market>>();
        result.Id.ShouldBe(command.MarketId);

        DbSetMock.Received(1).Update(Arg.Is<Market>(m =>
            m.Id == command.MarketId &&
            m.Name == command.Name &&
            m.Taxes == command.Taxes
        ));

        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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
