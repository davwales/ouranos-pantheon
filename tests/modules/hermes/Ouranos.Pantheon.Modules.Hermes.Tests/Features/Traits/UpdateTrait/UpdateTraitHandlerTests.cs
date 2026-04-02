using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Traits.UpdateTrait;

public sealed class UpdateTraitHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateTraitHandler _handler;
    private readonly ILogger<UpdateTraitHandler> _logger = Substitute.For<ILogger<UpdateTraitHandler>>();
    private readonly HermesDbContext _dbContext;

    public UpdateTraitHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new UpdateTraitHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateTraitAndReturnId()
    {
        // Arrange
        var existingTrait = Trait.Create(new Id<Trait>(Guid.NewGuid().ToString()), _fixture.Create<string>(), _fixture.Create<string>());
        await _dbContext.SeedData(existingTrait);

        var newName = _fixture.Create<string>();
        var newContent = _fixture.Create<string>();
        var command = new UpdateTraitInput(existingTrait.Id, newName, newContent, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UpdateTraitResponse>();
        result.TraitId.ShouldBe(existingTrait.Id);

        var updatedTrait = await _dbContext.Traits.FindAsync(existingTrait.Id);
        updatedTrait.ShouldNotBeNull();
        updatedTrait.Name.ShouldBe(newName);
        updatedTrait.Content.ShouldBe(newContent);
    }

    [Fact]
    public async Task Handle_WhenTraitNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateTraitInput(new Id<Trait>(_fixture.Create<string>()), _fixture.Create<string>(), _fixture.Create<string>());

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new UpdateTraitInput(new Id<Trait>(_fixture.Create<string>()), _fixture.Create<string>(), _fixture.Create<string>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
