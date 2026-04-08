using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Trait = Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits.Trait;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Traits.GetAllTraits;

public sealed class GetAllTraitsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllTraitsHandler _handler;
    private readonly ILogger<GetAllTraitsHandler> _logger = Substitute.For<ILogger<GetAllTraitsHandler>>();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();

    public GetAllTraitsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetAllTraitsHandler(_logger, _dbContext, _flagsmith);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(false));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    [Fact]
    public async Task Handle_WhenTraitsExist_ShouldReturnAllTraits()
    {
        // Arrange
        var trait1 = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var trait2 = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            false
        );
        await _dbContext.SeedData(trait1, trait2);

        var query = new GetAllTraitsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<List<GetAllTraitsResponse>>();
        result.Count.ShouldBe(2);
        var item = result[0];
        item.Id.ShouldNotBe(default);
        item.Name.ShouldNotBeNull();
        item.Content.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenNoTraits_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllTraitsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPublicModeEnabled_ShouldOnlyReturnPublicTraits()
    {
        // Arrange
        var publicTrait = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var privateTrait = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            false
        );
        await _dbContext.SeedData(publicTrait, privateTrait);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(true));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));

        var query = new GetAllTraitsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(publicTrait.Id);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllTraitsInput();
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenTraitsExist_ShouldReturnResponseWithAllProperties()
    {
        // Arrange
        await _dbContext.SeedData(
            Trait.Create(
                new Id<Trait>(Guid.NewGuid().ToString()),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                isPublic: false
            )
        );

        var query = new GetAllTraitsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Count.ShouldBe(1);
        result[0].IsPublic.ShouldBeFalse();
    }
}
