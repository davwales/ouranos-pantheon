using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableModels;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.UpdateModel;

public sealed class UpdateModelHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateModelHandler _handler;
    private readonly ILogger<UpdateModelHandler> _logger = Substitute.For<
        ILogger<UpdateModelHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public UpdateModelHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new UpdateModelHandler(_logger, _dbContext);
    }

    private async Task SeedAvailableModel(string modelIdentifier)
    {
        var availableModel = AvailableModel.Create(
            new Id<AvailableModel>(Guid.NewGuid().ToString()),
            modelIdentifier,
            "test-org"
        );
        await _dbContext.SeedData(availableModel);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateModelAndReturnId()
    {
        // Arrange
        var modelIdentifier = _fixture.Create<string>();
        var existingModel = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            modelIdentifier,
            _fixture.Create<string>()
        );
        await SeedAvailableModel(modelIdentifier);
        await _dbContext.SeedData(existingModel);

        var newName = _fixture.Create<string>();
        var command = new UpdateModelInput(
            existingModel.Id,
            newName,
            modelIdentifier,
            _fixture.Create<string>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UpdateModelResponse>();
        result.ModelId.ShouldBe(existingModel.Id);

        var updatedModel = await _dbContext.ModelConfigs.FindAsync(existingModel.Id);
        updatedModel.ShouldNotBeNull();
        updatedModel.Name.ShouldBe(newName);
    }

    [Fact]
    public async Task Handle_WhenIsDefaultTrue_ShouldClearOtherDefaults()
    {
        // Arrange
        var modelIdentifier = _fixture.Create<string>();
        await SeedAvailableModel(modelIdentifier);

        var otherDefault = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            modelIdentifier,
            _fixture.Create<string>(),
            isDefault: true
        );
        var targetModel = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            modelIdentifier,
            _fixture.Create<string>(),
            isDefault: false
        );
        await _dbContext.SeedData(otherDefault, targetModel);

        var command = new UpdateModelInput(
            targetModel.Id,
            _fixture.Create<string>(),
            modelIdentifier,
            _fixture.Create<string>(),
            IsDefault: true
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var refreshedOther = await _dbContext.ModelConfigs.FindAsync(otherDefault.Id);
        refreshedOther.ShouldNotBeNull();
        refreshedOther.IsDefault.ShouldBeFalse();

        var refreshedTarget = await _dbContext.ModelConfigs.FindAsync(targetModel.Id);
        refreshedTarget.ShouldNotBeNull();
        refreshedTarget.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenModelNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateModelInput(
            new Id<ModelConfig>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenContextWindowProvided_ShouldUpdateContextWindow()
    {
        // Arrange
        var modelIdentifier = _fixture.Create<string>();
        await SeedAvailableModel(modelIdentifier);

        var existingModel = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            modelIdentifier,
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(existingModel);

        var command = new UpdateModelInput(
            existingModel.Id,
            _fixture.Create<string>(),
            modelIdentifier,
            _fixture.Create<string>(),
            ContextWindow: 32768
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedModel = await _dbContext.ModelConfigs.FindAsync(existingModel.Id);
        updatedModel.ShouldNotBeNull();
        updatedModel.ContextWindow.ShouldBe(32768);
    }

    [Fact]
    public async Task Handle_WhenContextWindowClearedToNull_ShouldClearContextWindow()
    {
        // Arrange
        var modelIdentifier = _fixture.Create<string>();
        await SeedAvailableModel(modelIdentifier);

        var existingModel = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            modelIdentifier,
            _fixture.Create<string>(),
            contextWindow: 128000
        );
        await _dbContext.SeedData(existingModel);

        var command = new UpdateModelInput(
            existingModel.Id,
            _fixture.Create<string>(),
            modelIdentifier,
            _fixture.Create<string>(),
            ContextWindow: null
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedModel = await _dbContext.ModelConfigs.FindAsync(existingModel.Id);
        updatedModel.ShouldNotBeNull();
        updatedModel.ContextWindow.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new UpdateModelInput(
            new Id<ModelConfig>(_fixture.Create<string>()),
            _fixture.Create<string>(),
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
    public async Task Handle_WhenModelIdentifierMatchesAvailableModel_ShouldMarkAvailable()
    {
        // Arrange
        var modelIdentifier = "available-model";
        await SeedAvailableModel(modelIdentifier);

        var existingModel = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            "old-unavailable-identifier",
            _fixture.Create<string>()
        );
        existingModel.MarkUnavailable();
        await _dbContext.SeedData(existingModel);

        var command = new UpdateModelInput(
            existingModel.Id,
            _fixture.Create<string>(),
            modelIdentifier,
            _fixture.Create<string>()
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedModel = await _dbContext.ModelConfigs.FindAsync(existingModel.Id);
        updatedModel.ShouldNotBeNull();
        updatedModel.IsUnavailable.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenModelIdentifierDoesNotMatchAvailableModel_ShouldMarkUnavailable()
    {
        // Arrange
        await SeedAvailableModel("some-other-model");

        var existingModel = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(existingModel);

        var command = new UpdateModelInput(
            existingModel.Id,
            _fixture.Create<string>(),
            "nonexistent-model",
            _fixture.Create<string>()
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedModel = await _dbContext.ModelConfigs.FindAsync(existingModel.Id);
        updatedModel.ShouldNotBeNull();
        updatedModel.IsUnavailable.ShouldBeTrue();
    }
}
