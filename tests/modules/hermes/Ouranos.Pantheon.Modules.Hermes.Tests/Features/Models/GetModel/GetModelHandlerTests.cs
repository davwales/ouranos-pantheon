using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.GetModel;

public sealed class GetModelHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetModelHandler _handler;
    private readonly ILogger<GetModelHandler> _logger = Substitute.For<ILogger<GetModelHandler>>();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();

    public GetModelHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetModelHandler(_logger, _dbContext, _flagsmith);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(false));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnModelResponse()
    {
        // Arrange
        var model = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isPublic: true
        );
        await _dbContext.SeedData(model);

        var query = new GetModelInput(model.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<GetModelResponse>();
        result.Id.ShouldBe(model.Id);
        result.Name.ShouldBe(model.Name);
        result.ModelIdentifier.ShouldBe(model.ModelIdentifier);
        result.SystemPrompt.ShouldBe(model.SystemPrompt);
        result.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenModelNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetModelInput(new Id<ModelConfig>(_fixture.Create<string>()));

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotPublicAndPublicModeEnabled_ShouldThrowNotFoundException()
    {
        // Arrange
        var model = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isPublic: false
        );
        await _dbContext.SeedData(model);

        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(true));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));

        var query = new GetModelInput(model.Id);

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotPublicAndPublicModeDisabled_ShouldReturnModel()
    {
        // Arrange
        var model = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isPublic: false
        );
        await _dbContext.SeedData(model);

        var query = new GetModelInput(model.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Id.ShouldBe(model.Id);
    }

    [Fact]
    public async Task Handle_WhenContextWindowSet_ShouldIncludeContextWindowInResponse()
    {
        // Arrange
        var model = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            contextWindow: 128000,
            isPublic: true
        );
        await _dbContext.SeedData(model);

        var query = new GetModelInput(model.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ContextWindow.ShouldBe(128000);
    }

    [Fact]
    public async Task Handle_WhenContextWindowNotSet_ShouldReturnNullContextWindow()
    {
        // Arrange
        var model = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isPublic: true
        );
        await _dbContext.SeedData(model);

        var query = new GetModelInput(model.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ContextWindow.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetModelInput(new Id<ModelConfig>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
