using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Personas.UpdatePersona;

public sealed class UpdatePersonaHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdatePersonaHandler _handler;
    private readonly ILogger<UpdatePersonaHandler> _logger = Substitute.For<
        ILogger<UpdatePersonaHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public UpdatePersonaHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new UpdatePersonaHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdatePersonaAndReturnId()
    {
        // Arrange
        var existingPersona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(existingPersona);

        var newName = _fixture.Create<string>();
        var command = new UpdatePersonaInput(
            existingPersona.Id,
            newName,
            _fixture.Create<string>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UpdatePersonaResponse>();
        result.PersonaId.ShouldBe(existingPersona.Id);

        var updatedPersona = await _dbContext.Personas.FindAsync(existingPersona.Id);
        updatedPersona.ShouldNotBeNull();
        updatedPersona.Name.ShouldBe(newName);
    }

    [Fact]
    public async Task Handle_WhenIsDefaultTrue_ShouldClearOtherDefaults()
    {
        // Arrange
        var otherDefault = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isDefault: true
        );
        var targetPersona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isDefault: false
        );
        await _dbContext.SeedData(otherDefault, targetPersona);

        var command = new UpdatePersonaInput(
            targetPersona.Id,
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            IsDefault: true
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var refreshedOther = await _dbContext.Personas.FindAsync(otherDefault.Id);
        refreshedOther.ShouldNotBeNull();
        refreshedOther.IsDefault.ShouldBeFalse();

        var refreshedTarget = await _dbContext.Personas.FindAsync(targetPersona.Id);
        refreshedTarget.ShouldNotBeNull();
        refreshedTarget.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenPersonaNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdatePersonaInput(
            new Id<Persona>(_fixture.Create<string>()),
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
        var command = new UpdatePersonaInput(
            new Id<Persona>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
