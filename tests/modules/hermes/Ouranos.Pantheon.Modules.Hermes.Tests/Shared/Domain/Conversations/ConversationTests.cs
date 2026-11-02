using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Conversations;

public sealed class ConversationTests
{
    private readonly Fixture _fixture = new();

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
        conversation.ReasoningEffort.ShouldBeNull();
        conversation.ToolSetId.ShouldBeNull();
    }

    [Fact]
    public void Create_WhenToolSetIdProvided_ShouldSetIt()
    {
        // Arrange
        var toolSetId = DatabaseExtensions.CreateId<ToolSet>();

        // Act
        var conversation = Conversation.Create(
            DatabaseExtensions.CreateId<Conversation>(),
            DatabaseExtensions.CreateId<Persona>(),
            DatabaseExtensions.CreateId<ModelConfig>(),
            [],
            [],
            toolSetId: toolSetId
        );

        // Assert
        conversation.ToolSetId.ShouldBe(toolSetId);
    }

    [Fact]
    public void Create_WhenReasoningEffortProvided_ShouldSetIt()
    {
        // Arrange
        var id = new Id<Conversation>(Guid.NewGuid().ToString());

        // Act
        var conversation = Conversation.Create(
            id,
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            reasoningEffort: ReasoningEffort.Low
        );

        // Assert
        conversation.ReasoningEffort.ShouldBe(ReasoningEffort.Low);
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
        var create = () =>
            Conversation.Create(id, personaId, modelConfigId, [], [], persona: mismatchedPersona);

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
        var create = () =>
            Conversation.Create(id, personaId, modelConfigId, [], [], modelConfig: mismatchedModel);

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
        var persona = Persona.Create(
            personaId,
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );
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
        conversation.Update(
            newName,
            newPersonaId,
            newModelConfigId,
            [message],
            [trait],
            false,
            null,
            ReasoningEffort.High
        );

        // Assert
        conversation.Name.ShouldBe(newName);
        conversation.PersonaId.ShouldBe(newPersonaId);
        conversation.ModelConfigId.ShouldBe(newModelConfigId);
        conversation.Messages.Count.ShouldBe(1);
        conversation.Traits.Count.ShouldBe(1);
        conversation.IsPublic.ShouldBeFalse();
        conversation.ReasoningEffort.ShouldBe(ReasoningEffort.High);
    }

    [Fact]
    public void Update_WhenLorasGiven_ShouldReplaceTheLoras()
    {
        // Arrange
        var oldLora = Lora.Create(new Id<Lora>(Guid.NewGuid().ToString()), "Old", "old");
        var newLora = Lora.Create(new Id<Lora>(Guid.NewGuid().ToString()), "New", "new");
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            loras: [oldLora]
        );

        // Act
        conversation.Update(
            conversation.Name,
            conversation.PersonaId,
            conversation.ModelConfigId,
            [],
            [],
            true,
            null,
            loras: [newLora]
        );

        // Assert
        conversation.Loras.ShouldBe([newLora]);
    }

    [Fact]
    public void Update_WhenToolSetIdGiven_ShouldReplaceTheToolSetId()
    {
        // Arrange
        var newToolSetId = DatabaseExtensions.CreateId<ToolSet>();
        var conversation = Conversation.Create(
            DatabaseExtensions.CreateId<Conversation>(),
            DatabaseExtensions.CreateId<Persona>(),
            DatabaseExtensions.CreateId<ModelConfig>(),
            [],
            [],
            toolSetId: DatabaseExtensions.CreateId<ToolSet>()
        );

        // Act
        conversation.Update(
            conversation.Name,
            conversation.PersonaId,
            conversation.ModelConfigId,
            [],
            [],
            true,
            null,
            toolSetId: newToolSetId
        );

        // Assert
        conversation.ToolSetId.ShouldBe(newToolSetId);
    }

    [Fact]
    public void Update_WhenToolSetIdOmitted_ShouldClearTheToolSetId()
    {
        // Arrange
        var conversation = Conversation.Create(
            DatabaseExtensions.CreateId<Conversation>(),
            DatabaseExtensions.CreateId<Persona>(),
            DatabaseExtensions.CreateId<ModelConfig>(),
            [],
            [],
            toolSetId: DatabaseExtensions.CreateId<ToolSet>()
        );

        // Act
        conversation.Update(
            conversation.Name,
            conversation.PersonaId,
            conversation.ModelConfigId,
            [],
            [],
            true,
            null
        );

        // Assert
        conversation.ToolSetId.ShouldBeNull();
    }

    [Fact]
    public void Update_WhenMessagesAreNull_ShouldKeepTheMessages()
    {
        // Arrange
        var id = new Id<Conversation>(Guid.NewGuid().ToString());
        var message = Message.Create(
            new Id<Message>(Guid.NewGuid().ToString()),
            id,
            "Hello",
            Role.User,
            0
        );
        var conversation = Conversation.Create(
            id,
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [message],
            []
        );

        // Act
        conversation.Update(
            "Renamed",
            conversation.PersonaId,
            conversation.ModelConfigId,
            null,
            [],
            false,
            null
        );

        // Assert
        conversation.Messages.ShouldBe([message]);
        conversation.Name.ShouldBe("Renamed");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidName_ShouldDefaultToNewConversation(string newName)
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
        conversation.Update(
            newName,
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            true,
            null
        );

        // Assert
        conversation.Name.ShouldBe("New Conversation");
    }

    [Fact]
    public void Update_WhenNameIsNull_ShouldKeepTheName()
    {
        // Arrange
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            "Generated name"
        );

        // Act
        conversation.Update(
            null,
            conversation.PersonaId,
            conversation.ModelConfigId,
            null,
            [],
            true,
            null
        );

        // Assert
        conversation.Name.ShouldBe("Generated name");
    }

    [Fact]
    public void ApplyGeneratedName_WhenNameIsDefault_ShouldSetTheName()
    {
        // Arrange
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            []
        );

        // Act
        var applied = conversation.ApplyGeneratedName("  Trip planning ");

        // Assert
        applied.ShouldBeTrue();
        conversation.Name.ShouldBe("Trip planning");
        conversation.HasDefaultName.ShouldBeFalse();
    }

    [Fact]
    public void ApplyGeneratedName_WhenRenamedMeanwhile_ShouldKeepTheUsersName()
    {
        // Arrange
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            "My name"
        );

        // Act
        var applied = conversation.ApplyGeneratedName("Trip planning");

        // Assert
        applied.ShouldBeFalse();
        conversation.Name.ShouldBe("My name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ApplyGeneratedName_WhenNameIsBlank_ShouldKeepTheDefault(string name)
    {
        // Arrange
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            []
        );

        // Act
        var applied = conversation.ApplyGeneratedName(name);

        // Assert
        applied.ShouldBeFalse();
        conversation.HasDefaultName.ShouldBeTrue();
    }
}
