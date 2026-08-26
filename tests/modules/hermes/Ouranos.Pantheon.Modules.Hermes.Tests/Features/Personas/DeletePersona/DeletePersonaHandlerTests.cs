using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Personas.DeletePersona;

public sealed class DeletePersonaHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly DeletePersonaHandler _handler;
    private readonly ILogger<DeletePersonaHandler> _logger = Substitute.For<
        ILogger<DeletePersonaHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public DeletePersonaHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new DeletePersonaHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldDeletePersonaAndReturnId()
    {
        // Arrange
        var existingPersona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(existingPersona);

        var command = new DeletePersonaInput(existingPersona.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<DeletePersonaResponse>();
        result.PersonaId.ShouldBe(existingPersona.Id);

        var deletedPersona = await _dbContext.Personas.FindAsync(existingPersona.Id);
        deletedPersona.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenPersonaNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeletePersonaInput(new Id<Persona>(_fixture.Create<string>()));

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeletePersonaInput(new Id<Persona>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
