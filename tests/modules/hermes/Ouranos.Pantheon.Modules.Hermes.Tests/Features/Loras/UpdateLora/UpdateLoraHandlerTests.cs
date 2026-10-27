using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.UpdateLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.UpdateLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.UpdateLora;

public sealed class UpdateLoraHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateLoraHandler _handler;
    private readonly ILogger<UpdateLoraHandler> _logger = Substitute.For<
        ILogger<UpdateLoraHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public UpdateLoraHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new UpdateLoraHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateLoraAndReturnId()
    {
        // Arrange
        var existingLora = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        await _dbContext.SeedData(existingLora);

        var newName = _fixture.Create<string>();
        var newIdentifier = _fixture.Create<string>();
        var command = new UpdateLoraInput(existingLora.Id, newName, newIdentifier, 0.5f, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UpdateLoraResponse>();
        result.LoraId.ShouldBe(existingLora.Id);

        var updatedLora = await _dbContext.Loras.FindAsync(existingLora.Id);
        updatedLora.ShouldNotBeNull();
        updatedLora.Name.ShouldBe(newName);
        updatedLora.Identifier.ShouldBe(newIdentifier);
        updatedLora.Weight.ShouldBe(0.5f);
        updatedLora.IsPublic.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenLoraNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateLoraInput(
            new Id<Lora>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new UpdateLoraInput(
            new Id<Lora>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenAnotherConfigHasTheIdentifier_ShouldThrowConflictException()
    {
        // Arrange
        var lora = Lora.Create(new Id<Lora>(Guid.NewGuid().ToString()), "Detail", "add-detail");
        var other = Lora.Create(new Id<Lora>(Guid.NewGuid().ToString()), "Pixel", "pixel-art");
        await _dbContext.SeedData(lora, other);

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(() =>
            _handler.Handle(
                new UpdateLoraInput(lora.Id, "Detail", "pixel-art"),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Handle_WhenTriggerPhraseIsGiven_ShouldSaveIt()
    {
        // Arrange
        var lora = Lora.Create(new Id<Lora>(Guid.NewGuid().ToString()), "Detail", "add-detail");
        await _dbContext.SeedData(lora);

        // Act
        await _handler.Handle(
            new UpdateLoraInput(lora.Id, "Detail", "add-detail", TriggerPhrase: "detailed"),
            CancellationToken.None
        );

        // Assert
        var updated = await _dbContext.Loras.FindAsync(lora.Id);
        updated.ShouldNotBeNull().TriggerPhrase.ShouldBe("detailed");
    }
}
