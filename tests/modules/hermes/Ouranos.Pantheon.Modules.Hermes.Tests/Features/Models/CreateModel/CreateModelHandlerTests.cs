using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.CreateModel;

public sealed class CreateModelHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateModelHandler _handler;
    private readonly ILogger<CreateModelHandler> _logger = Substitute.For<
        ILogger<CreateModelHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public CreateModelHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new CreateModelHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateModelAndReturnId()
    {
        // Arrange
        var command = new CreateModelInput(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CreateModelResponse>();
        result.ModelId.ShouldNotBe(default);

        var savedModel = await _dbContext.ModelConfigs.FindAsync(result.ModelId);
        savedModel.ShouldNotBeNull();
        savedModel.Name.ShouldBe(command.Name);
    }

    [Fact]
    public async Task Handle_WhenIsDefaultTrue_ShouldClearOtherDefaults()
    {
        // Arrange
        var existingDefault = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            isDefault: true
        );
        await _dbContext.SeedData(existingDefault);

        var command = new CreateModelInput(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            IsDefault: true
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var oldModel = await _dbContext.ModelConfigs.FindAsync(existingDefault.Id);
        oldModel.ShouldNotBeNull();
        oldModel.IsDefault.ShouldBeFalse();

        var newModel = await _dbContext.ModelConfigs.FindAsync(result.ModelId);
        newModel.ShouldNotBeNull();
        newModel.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenContextWindowProvided_ShouldSaveContextWindow()
    {
        // Arrange
        var command = new CreateModelInput(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            ContextWindow: 128_000
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedModel = await _dbContext.ModelConfigs.FindAsync(result.ModelId);
        savedModel.ShouldNotBeNull();
        savedModel.ContextWindow.ShouldBe(128_000);
    }

    [Fact]
    public async Task Handle_WhenContextWindowNotProvided_ShouldLeaveContextWindowNull()
    {
        // Arrange
        var command = new CreateModelInput(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedModel = await _dbContext.ModelConfigs.FindAsync(result.ModelId);
        savedModel.ShouldNotBeNull();
        savedModel.ContextWindow.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreateModelInput(
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
}
