using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Traits.DeleteTrait;

public sealed class DeleteTraitHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly DeleteTraitHandler _handler;
    private readonly ILogger<DeleteTraitHandler> _logger = Substitute.For<ILogger<DeleteTraitHandler>>();
    private readonly HermesDbContext _dbContext;

    public DeleteTraitHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new DeleteTraitHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldDeleteTraitAndReturnId()
    {
        // Arrange
        var existingTrait = Trait.Create(new Id<Trait>(Guid.NewGuid().ToString()), _fixture.Create<string>(), _fixture.Create<string>());
        await _dbContext.SeedData(existingTrait);

        var command = new DeleteTraitInput(existingTrait.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<DeleteTraitResponse>();
        result.TraitId.ShouldBe(existingTrait.Id);

        var deletedTrait = await _dbContext.Traits.FindAsync(existingTrait.Id);
        deletedTrait.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenTraitNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteTraitInput(new Id<Trait>(_fixture.Create<string>()));

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeleteTraitInput(new Id<Trait>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
