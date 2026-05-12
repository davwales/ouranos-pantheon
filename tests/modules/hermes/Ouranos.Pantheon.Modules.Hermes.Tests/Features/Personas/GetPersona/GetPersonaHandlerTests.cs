using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Personas.GetPersona;

public sealed class GetPersonaHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetPersonaHandler _handler;
    private readonly ILogger<GetPersonaHandler> _logger = Substitute.For<
        ILogger<GetPersonaHandler>
    >();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();

    public GetPersonaHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetPersonaHandler(_logger, _dbContext, _flagsmith);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(false));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnPersonaResponse()
    {
        // Arrange
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isPublic: true
        );
        await _dbContext.SeedData(persona);

        var query = new GetPersonaInput(persona.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<GetPersonaResponse>();
        result.Id.ShouldBe(persona.Id);
        result.Name.ShouldBe(persona.Name);
        result.Description.ShouldBe(persona.Description);
        result.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenPersonaNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetPersonaInput(new Id<Persona>(_fixture.Create<string>()));

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotPublicAndPublicModeEnabled_ShouldThrowNotFoundException()
    {
        // Arrange
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isPublic: false
        );
        await _dbContext.SeedData(persona);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(true));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));

        var query = new GetPersonaInput(persona.Id);

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotPublicAndPublicModeDisabled_ShouldReturnPersona()
    {
        // Arrange
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isPublic: false
        );
        await _dbContext.SeedData(persona);

        var query = new GetPersonaInput(persona.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Id.ShouldBe(persona.Id);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetPersonaInput(new Id<Persona>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenPersonaHasAllFields_ShouldReturnResponseWithAllProperties()
    {
        // Arrange
        var persona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            personality: "Friendly",
            scenario: "In a chat",
            isDefault: true,
            isPublic: false
        );
        await _dbContext.SeedData(persona);

        var query = new GetPersonaInput(persona.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Personality.ShouldBe("Friendly");
        result.Scenario.ShouldBe("In a chat");
        result.IsDefault.ShouldBeTrue();
        result.IsPublic.ShouldBeFalse();
    }
}
