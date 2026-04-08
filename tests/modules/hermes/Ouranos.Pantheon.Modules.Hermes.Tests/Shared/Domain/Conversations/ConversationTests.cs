using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Conversations;

public sealed class ConversationTests
{
    private readonly IFixture _fixture = new Fixture();

    public ConversationTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Conversation>(Guid.NewGuid().ToString());
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var name = _fixture.Create<string>();

        // Act
        var conversation = Conversation.Create(id, personaId, modelConfigId, [], [], name);

        // Assert
        conversation.Id.ShouldBe(id);
        conversation.PersonaId.ShouldBe(personaId);
        conversation.ModelConfigId.ShouldBe(modelConfigId);
        conversation.Name.ShouldBe(name);
        conversation.IsPublic.ShouldBeTrue();
        conversation.Messages.ShouldBeEmpty();
        conversation.Traits.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenNameNullOrWhitespace_ShouldDefaultToNewConversation(string? name)
    {
        // Arrange
        var id = new Id<Conversation>(Guid.NewGuid().ToString());
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());

        // Act
        var conversation = Conversation.Create(id, personaId, modelConfigId, [], [], name);

        // Assert
        conversation.Name.ShouldBe("New Conversation");
    }

    [Fact]
    public void Create_WhenNameHasWhitespace_ShouldTrimName()
    {
        // Arrange
        var id = new Id<Conversation>(Guid.NewGuid().ToString());
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());

        // Act
        var conversation = Conversation.Create(id, personaId, modelConfigId, [], [], "  hello  ");

        // Assert
        conversation.Name.ShouldBe("hello");
    }

    [Fact]
    public void Create_WhenPersonaIdMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        var id = new Id<Conversation>(Guid.NewGuid().ToString());
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var mismatchedPersona = Persona.Create(
            new Id<Persona>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var create = () => Conversation.Create(id, personaId, modelConfigId, [], [], persona: mismatchedPersona);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenModelConfigIdMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        var id = new Id<Conversation>(Guid.NewGuid().ToString());
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var mismatchedModel = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var create = () => Conversation.Create(id, personaId, modelConfigId, [], [], modelConfig: mismatchedModel);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenValidPersonaProvided_ShouldExposePersonaNavigation()
    {
        // Arrange
        var id = new Id<Conversation>(Guid.NewGuid().ToString());
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var persona = Persona.Create(personaId, _fixture.Create<string>(), _fixture.Create<string>());
        var modelConfig = ModelConfig.Create(
            modelConfigId,
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var conversation = Conversation.Create(
            id,
            personaId,
            modelConfigId,
            [],
            [],
            persona: persona,
            modelConfig: modelConfig
        );

        // Assert
        conversation.Persona.ShouldBe(persona);
        conversation.ModelConfig.ShouldBe(modelConfig);
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var id = new Id<Conversation>(Guid.NewGuid().ToString());
        var conversation = Conversation.Create(
            id,
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            []
        );

        var newName = _fixture.Create<string>();
        var newPersonaId = new Id<Persona>(Guid.NewGuid().ToString());
        var newModelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var trait = Trait.Create(
            new Id<Trait>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
        var message = Message.Create(
            new Id<Message>(Guid.NewGuid().ToString()),
            id,
            _fixture.Create<string>(),
            Role.User,
            0
        );

        // Act
        conversation.Update(newName, newPersonaId, newModelConfigId, [message], [trait], false);

        // Assert
        conversation.Name.ShouldBe(newName);
        conversation.PersonaId.ShouldBe(newPersonaId);
        conversation.ModelConfigId.ShouldBe(newModelConfigId);
        conversation.Messages.Count.ShouldBe(1);
        conversation.Traits.Count.ShouldBe(1);
        conversation.IsPublic.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidName_ShouldThrowArgumentException(string? newName)
    {
        // Arrange
        var id = new Id<Conversation>(Guid.NewGuid().ToString());
        var conversation = Conversation.Create(
            id,
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            []
        );

        // Act
        var update = () => conversation.Update(
            newName!,
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            true
        );

        // Assert
        update.ShouldThrow<ArgumentException>();
    }
}
