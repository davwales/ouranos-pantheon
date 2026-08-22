using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;
using Trait = Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits.Trait;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Traits.GetTrait;

public sealed class GetTraitHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetTraitHandler _handler;
    private readonly ILogger<GetTraitHandler> _logger = Substitute.For<ILogger<GetTraitHandler>>();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();

    public GetTraitHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetTraitHandler(_logger, _dbContext, _flagsmith);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(false));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnTraitResponse()
    {
        // Arrange
        var trait = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            true
        );
        await _dbContext.SeedData(trait);

        var query = new GetTraitInput(trait.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<GetTraitResponse>();
        result.Id.ShouldBe(trait.Id);
        result.Name.ShouldBe(trait.Name);
        result.Content.ShouldBe(trait.Content);
        result.IsPublic.ShouldBe(trait.IsPublic);
    }

    [Fact]
    public async Task Handle_WhenTraitNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetTraitInput(new Id<Trait>(_fixture.Create<string>()));

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotPublicAndPublicModeEnabled_ShouldThrowNotFoundException()
    {
        // Arrange
        var trait = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            false
        );
        await _dbContext.SeedData(trait);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(true));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));

        var query = new GetTraitInput(trait.Id);

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotPublicAndPublicModeDisabled_ShouldReturnTrait()
    {
        // Arrange
        var trait = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            false
        );
        await _dbContext.SeedData(trait);

        var query = new GetTraitInput(trait.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Id.ShouldBe(trait.Id);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetTraitInput(new Id<Trait>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
