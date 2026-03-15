using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.CreateMarket;

public sealed class CreateMarketHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateMarketHandler _handler;
    private readonly ILogger<CreateMarketHandler> _logger = Substitute.For<ILogger<CreateMarketHandler>>();
    private readonly PlutusDbContext _dbContext = Substitute.For<PlutusDbContext>(
        Substitute.For<DbContextOptions<PlutusDbContext>>()
    );

    public CreateMarketHandlerTests()
    {
        _handler = new CreateMarketHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateMarketAndReturnId()
    {
        // Arrange
        var command = _fixture.Create<CreateMarketInput>();
        var expectedId = new Id<Market>(_fixture.Create<string>());

        var DbSetMock = Substitute.For<DbSet<Market>>();
        _dbContext.Markets.Returns(DbSetMock);

        var entryMock = Substitute.For<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Market>>();
        DbSetMock.AddAsync(Arg.Any<Market>(), Arg.Any<CancellationToken>())
            .Returns(_ => new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Market>>(entryMock));

        _dbContext.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Market>>();
        result.Id.ShouldNotBe(default);

        await DbSetMock.Received(1).AddAsync(
            Arg.Is<Market>(m =>
                m.Name == command.Name &&
                m.Taxes == command.Taxes &&
                m.IsForecastingEnabled == command.IsForecastingEnabled
            ),
            Arg.Any<CancellationToken>()
        );

        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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
