using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.DeleteMarket;

public sealed class DeleteMarketHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly DeleteMarketHandler _handler;
    private readonly ILogger<DeleteMarketHandler> _logger = Substitute.For<
        ILogger<DeleteMarketHandler>
    >();
    private readonly PlutusDbContext _dbContext;
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    public DeleteMarketHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new DeleteMarketHandler(_logger, _dbContext, _cache);
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
    public async Task Handle_WhenHappyPath_ShouldEvictCachedMarket()
    {
        // Arrange
        var existingMarket = _fixture.Create<Market>();
        await _dbContext.SeedData(existingMarket);
        _cache.Set(existingMarket.Id, existingMarket);

        var command = new DeleteMarketInput(existingMarket.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cache.TryGetValue(existingMarket.Id, out _).ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenMarketHasSymbols_ShouldThrowConflictExceptionAndKeepMarket()
    {
        // Arrange
        var existingMarket = _fixture.Create<Market>();
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "1234",
            null,
            "Test Symbol",
            existingMarket.Id,
            new AdditionalFields()
        );
        await _dbContext.SeedData(existingMarket);
        await _dbContext.SeedData(symbol);

        var command = new DeleteMarketInput(existingMarket.Id);

        // Act
        var delete = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await delete.ShouldThrowAsync<ConflictException>();
        (await _dbContext.Markets.FindAsync(existingMarket.Id)).ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WhenMemoryCacheIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new DeleteMarketHandler(_logger, _dbContext, null!)
        );
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
