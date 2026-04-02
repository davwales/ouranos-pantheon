using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Personas.CreatePersona;

public sealed class CreatePersonaHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreatePersonaHandler _handler;
    private readonly ILogger<CreatePersonaHandler> _logger = Substitute.For<ILogger<CreatePersonaHandler>>();
    private readonly HermesDbContext _dbContext;

    public CreatePersonaHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new CreatePersonaHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreatePersonaAndReturnId()
    {
        // Arrange
        var command = new CreatePersonaInput(_fixture.Create<string>(), _fixture.Create<string>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CreatePersonaResponse>();
        result.PersonaId.ShouldNotBe(default);

        var savedPersona = await _dbContext.Personas.FindAsync(result.PersonaId);
        savedPersona.ShouldNotBeNull();
        savedPersona.Name.ShouldBe(command.Name);
    }

    [Fact]
    public async Task Handle_WhenIsDefaultTrue_ShouldClearOtherDefaults()
    {
        // Arrange
        var existingDefault = Persona.Create(new Id<Persona>(Guid.NewGuid().ToString()), _fixture.Create<string>(), _fixture.Create<string>(), isDefault: true);
        await _dbContext.SeedData(existingDefault);

        var command = new CreatePersonaInput(_fixture.Create<string>(), _fixture.Create<string>(), IsDefault: true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var oldPersona = await _dbContext.Personas.FindAsync(existingDefault.Id);
        oldPersona.ShouldNotBeNull();
        oldPersona.IsDefault.ShouldBeFalse();

        var newPersona = await _dbContext.Personas.FindAsync(result.PersonaId);
        newPersona.ShouldNotBeNull();
        newPersona.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreatePersonaInput(_fixture.Create<string>(), _fixture.Create<string>());
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
