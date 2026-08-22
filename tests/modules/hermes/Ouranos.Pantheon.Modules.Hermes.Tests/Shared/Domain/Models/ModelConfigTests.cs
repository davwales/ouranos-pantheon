using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Models;

public sealed class ModelConfigTests
{
    private readonly IFixture _fixture = new Fixture();

    public ModelConfigTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var name = _fixture.Create<string>();
        var modelIdentifier = _fixture.Create<string>();
        var systemPrompt = _fixture.Create<string>();
        var temperature = _fixture.Create<float?>();
        var maxTokens = _fixture.Create<int?>();
        var repeatPenalty = _fixture.Create<float?>();
        var isDefault = _fixture.Create<bool>();
        var isPublic = _fixture.Create<bool>();

        // Act
        var model = ModelConfig.Create(
            id,
            name,
            modelIdentifier,
            systemPrompt,
            temperature,
            maxTokens,
            repeatPenalty,
            null,
            isDefault,
            isPublic
        );

        // Assert
        model.Id.ShouldBe(id);
        model.Name.ShouldBe(name);
        model.ModelIdentifier.ShouldBe(modelIdentifier);
        model.SystemPrompt.ShouldBe(systemPrompt);
        model.Temperature.ShouldBe(temperature);
        model.MaxTokens.ShouldBe(maxTokens);
        model.RepeatPenalty.ShouldBe(repeatPenalty);
        model.IsDefault.ShouldBe(isDefault);
        model.IsPublic.ShouldBe(isPublic);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var id = new Id<ModelConfig>(Guid.NewGuid().ToString());

        // Act
        var create = () =>
            ModelConfig.Create(id, name!, _fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidModelIdentifier_ShouldThrowArgumentException(
        string? modelIdentifier
    )
    {
        // Arrange
        var id = new Id<ModelConfig>(Guid.NewGuid().ToString());

        // Act
        var create = () =>
            ModelConfig.Create(
                id,
                _fixture.Create<string>(),
                modelIdentifier!,
                _fixture.Create<string>()
            );

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidSystemPrompt_ShouldThrowArgumentException(string? systemPrompt)
    {
        // Arrange
        var id = new Id<ModelConfig>(Guid.NewGuid().ToString());

        // Act
        var create = () =>
            ModelConfig.Create(
                id,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                systemPrompt!
            );

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var id = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var model = ModelConfig.Create(
            id,
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var newName = _fixture.Create<string>();
        var newModelIdentifier = _fixture.Create<string>();
        var newSystemPrompt = _fixture.Create<string>();

        // Act
        model.Update(
            newName,
            newModelIdentifier,
            newSystemPrompt,
            0.5f,
            1024,
            1.1f,
            null,
            true,
            false
        );

        // Assert
        model.Name.ShouldBe(newName);
        model.ModelIdentifier.ShouldBe(newModelIdentifier);
        model.SystemPrompt.ShouldBe(newSystemPrompt);
        model.Temperature.ShouldBe(0.5f);
        model.MaxTokens.ShouldBe(1024);
        model.RepeatPenalty.ShouldBe(1.1f);
        model.IsDefault.ShouldBeTrue();
        model.IsPublic.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidName_ShouldThrowArgumentException(string? newName)
    {
        // Arrange
        var model = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var update = () =>
            model.Update(newName!, _fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidModelIdentifier_ShouldThrowArgumentException(
        string? newModelIdentifier
    )
    {
        // Arrange
        var model = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var update = () =>
            model.Update(_fixture.Create<string>(), newModelIdentifier!, _fixture.Create<string>());

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidSystemPrompt_ShouldThrowArgumentException(string? newSystemPrompt)
    {
        // Arrange
        var model = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var update = () =>
            model.Update(_fixture.Create<string>(), _fixture.Create<string>(), newSystemPrompt!);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenContextWindowProvided_ShouldSetContextWindow()
    {
        // Arrange
        var id = new Id<ModelConfig>(Guid.NewGuid().ToString());
        const int contextWindow = 128000;

        // Act
        var model = ModelConfig.Create(
            id,
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            contextWindow: contextWindow
        );

        // Assert
        model.ContextWindow.ShouldBe(contextWindow);
    }

    [Fact]
    public void Create_WhenContextWindowNotProvided_ShouldLeaveContextWindowNull()
    {
        // Arrange
        var id = new Id<ModelConfig>(Guid.NewGuid().ToString());

        // Act
        var model = ModelConfig.Create(
            id,
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Assert
        model.ContextWindow.ShouldBeNull();
    }

    [Fact]
    public void Update_WhenContextWindowProvided_ShouldUpdateContextWindow()
    {
        // Arrange
        var model = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        const int contextWindow = 32768;

        // Act
        model.Update(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            contextWindow: contextWindow
        );

        // Assert
        model.ContextWindow.ShouldBe(contextWindow);
    }

    [Fact]
    public void Update_WhenContextWindowSetToNull_ShouldClearContextWindow()
    {
        // Arrange
        var model = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            contextWindow: 128000
        );

        // Act
        model.Update(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            contextWindow: null
        );

        // Assert
        model.ContextWindow.ShouldBeNull();
    }
}
