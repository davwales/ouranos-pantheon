using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.DeleteMarket;

public sealed class DeleteMarketHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly DeleteMarketHandler _handler;
    private readonly ILogger<DeleteMarketHandler> _logger = Substitute.For<ILogger<DeleteMarketHandler>>();
    private readonly PlutusDbContext _dbContext;

    public DeleteMarketHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new DeleteMarketHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldDeleteMarketAndReturnId()
    {
        // Arrange
        var existingMarket = _fixture.Create<Market>();
        await _dbContext.SeedData(existingMarket);

        var command = new DeleteMarketInput(existingMarket.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Market>>();
        result.Id.ShouldBe(existingMarket.Id);

        var deletedMarket = await _dbContext.Markets.FindAsync(existingMarket.Id);
        deletedMarket.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenMarketNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteMarketInput(new Id<Market>(_fixture.Create<string>()));

        // Act
        var delete = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await delete.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeleteMarketInput(new Id<Market>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var delete = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await delete.ShouldThrowAsync<OperationCanceledException>();
    }
}
