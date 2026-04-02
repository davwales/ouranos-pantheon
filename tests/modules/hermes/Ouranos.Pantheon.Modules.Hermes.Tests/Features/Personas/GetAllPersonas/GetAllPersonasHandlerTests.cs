using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Personas.GetAllPersonas;

public sealed class GetAllPersonasHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllPersonasHandler _handler;
    private readonly ILogger<GetAllPersonasHandler> _logger = Substitute.For<ILogger<GetAllPersonasHandler>>();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();

    public GetAllPersonasHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetAllPersonasHandler(_logger, _dbContext, _flagsmith);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(false));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    [Fact]
    public async Task Handle_WhenPersonasExist_ShouldReturnAllPersonas()
    {
        // Arrange
        var persona1 = Persona.Create(new Id<Persona>(Guid.NewGuid().ToString()), _fixture.Create<string>(), _fixture.Create<string>(), isPublic: true);
        var persona2 = Persona.Create(new Id<Persona>(Guid.NewGuid().ToString()), _fixture.Create<string>(), _fixture.Create<string>(), isPublic: false);
        await _dbContext.SeedData(persona1, persona2);

        var query = new GetAllPersonasInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<List<GetAllPersonasResponse>>();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenNoPersonas_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllPersonasInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPublicModeEnabled_ShouldOnlyReturnPublicPersonas()
    {
        // Arrange
        var publicPersona = Persona.Create(new Id<Persona>(Guid.NewGuid().ToString()), _fixture.Create<string>(), _fixture.Create<string>(), isPublic: true);
        var privatePersona = Persona.Create(new Id<Persona>(Guid.NewGuid().ToString()), _fixture.Create<string>(), _fixture.Create<string>(), isPublic: false);
        await _dbContext.SeedData(publicPersona, privatePersona);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(true));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));

        var query = new GetAllPersonasInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(publicPersona.Id);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllPersonasInput();
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
