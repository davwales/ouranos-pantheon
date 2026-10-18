using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.UpdateConversation;

public sealed class UpdateConversationHandlerTests
{
    private readonly Fixture _fixture = new();
    private readonly UpdateConversationHandler _handler;
    private readonly ILogger<UpdateConversationHandler> _logger = Substitute.For<
        ILogger<UpdateConversationHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public UpdateConversationHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new UpdateConversationHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateConversationAndReturnId()
    {
        // Arrange
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(conversation);

        var newName = _fixture.Create<string>();
        var newPersonaId = new Id<Persona>(Guid.NewGuid().ToString());
        var newModelConfigId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var command = new UpdateConversationInput(
            conversation.Id,
            newName,
            newPersonaId,
            newModelConfigId,
            [],
            true
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Conversation>>();
        result.Id.ShouldBe(conversation.Id);

        var updated = await _dbContext.Conversations.FindAsync(conversation.Id);
        updated.ShouldNotBeNull();
        updated.Name.ShouldBe(newName);
        updated.PersonaId.ShouldBe(newPersonaId);
        updated.ModelConfigId.ShouldBe(newModelConfigId);
    }

    [Fact]
    public async Task Handle_WhenLoraIdsChange_ShouldReplaceTheConversationLoras()
    {
        // Arrange
        var oldLora = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        var newLora = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        await _dbContext.SeedData(oldLora, newLora);

        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            _fixture.Create<string>(),
            loras: [oldLora]
        );

        await _dbContext.SeedData(conversation);

        var command = new UpdateConversationInput(
            conversation.Id,
            conversation.Name,
            conversation.PersonaId,
            conversation.ModelConfigId,
            [],
            true,
            LoraIds: [newLora.Id]
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext
            .Conversations.Include(c => c.Loras)
            .SingleAsync(c => c.Id == conversation.Id);

        updated.Loras.ShouldHaveSingleItem().Id.ShouldBe(newLora.Id);
    }

    [Fact]
    public async Task Handle_WhenReasoningEffortChanges_ShouldSaveIt()
    {
        // Arrange
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            _fixture.Create<string>(),
            reasoningEffort: ReasoningEffort.High
        );

        await _dbContext.SeedData(conversation);

        var command = new UpdateConversationInput(
            conversation.Id,
            conversation.Name,
            conversation.PersonaId,
            conversation.ModelConfigId,
            [],
            true,
            ReasoningEffort: ReasoningEffort.None
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Conversations.FindAsync(conversation.Id);
        updated.ShouldNotBeNull();
        updated.ReasoningEffort.ShouldBe(ReasoningEffort.None);
    }

    [Fact]
    public async Task Handle_WhenMessagesProvided_ShouldReplaceMessages()
    {
        // Arrange
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var existingMessage = Message.Create(
            new Id<Message>(Guid.NewGuid().ToString()),
            conversationId,
            "Old message",
            Role.User,
            0
        );
        var conversation = Conversation.Create(
            conversationId,
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [existingMessage],
            [],
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(conversation);

        var command = new UpdateConversationInput(
            conversation.Id,
            _fixture.Create<string>(),
            conversation.PersonaId,
            conversation.ModelConfigId,
            [],
            true,
            [new UpdateConversationMessageInput("Hello World", Role.User)]
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.ShouldBe(conversation.Id);

        var messages = await _dbContext
            .Messages.Where(m => m.ConversationId == conversation.Id)
            .ToListAsync();
        var message = messages.ShouldHaveSingleItem();
        message.Content.ShouldBe("Hello World");
        message.Role.ShouldBe(Role.User);
    }

    [Fact]
    public async Task Handle_WhenMessagesAreNull_ShouldKeepTheExistingMessages()
    {
        // Arrange
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var conversation = Conversation.Create(
            conversationId,
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [
                Message.Create(
                    new Id<Message>(Guid.NewGuid().ToString()),
                    conversationId,
                    "Hi",
                    Role.User,
                    0
                ),
                Message.Create(
                    new Id<Message>(Guid.NewGuid().ToString()),
                    conversationId,
                    "Hello",
                    Role.Assistant,
                    1
                ),
            ],
            [],
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(conversation);

        var newName = _fixture.Create<string>();
        var command = new UpdateConversationInput(
            conversation.Id,
            newName,
            conversation.PersonaId,
            conversation.ModelConfigId,
            [],
            false
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext
            .Conversations.Include(c => c.Messages)
            .SingleAsync(c => c.Id == conversation.Id);
        updated.Name.ShouldBe(newName);
        updated.IsPublic.ShouldBeFalse();
        updated.Messages.OrderBy(m => m.SortOrder).Select(m => m.Content).ShouldBe(["Hi", "Hello"]);
    }

    [Fact]
    public async Task Handle_WhenFolderIdIsUnchanged_ShouldKeepTheFolder()
    {
        // Arrange
        var folder = Folder.Create(
            new Id<Folder>(Guid.NewGuid().ToString()),
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(folder);

        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            _fixture.Create<string>(),
            folderId: folder.Id
        );
        await _dbContext.SeedData(conversation);

        var command = new UpdateConversationInput(
            conversation.Id,
            _fixture.Create<string>(),
            conversation.PersonaId,
            conversation.ModelConfigId,
            [],
            true,
            FolderId: folder.Id
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _dbContext.Conversations.FindAsync(conversation.Id);
        updated.ShouldNotBeNull();
        updated.FolderId.ShouldBe(folder.Id);
    }

    [Fact]
    public async Task Handle_WhenMessagesHaveParts_ShouldSaveThePartsInOrder()
    {
        // Arrange
        var conversation = Conversation.Create(
            new Id<Conversation>(Guid.NewGuid().ToString()),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            [],
            _fixture.Create<string>()
        );

        await _dbContext.SeedData(conversation);

        var command = new UpdateConversationInput(
            conversation.Id,
            _fixture.Create<string>(),
            conversation.PersonaId,
            conversation.ModelConfigId,
            [],
            true,
            [
                new UpdateConversationMessageInput("Hi", Role.User),
                new UpdateConversationMessageInput(
                    "Hello",
                    Role.Assistant,
                    [new ReasoningMessagePartDto("Thinking"), new TextMessagePartDto("Hello")]
                ),
            ]
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var assistant = await _dbContext
            .Messages.Include(m => m.Parts)
            .SingleAsync(m => m.Role == Role.Assistant);

        var parts = assistant.Parts.OrderBy(p => p.SortOrder).ToList();
        parts[0].ShouldBeOfType<ReasoningMessagePart>().Content.ShouldBe("Thinking");
        parts[1].ShouldBeOfType<TextMessagePart>().Content.ShouldBe("Hello");
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateConversationInput(
            new Id<Conversation>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            true
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new UpdateConversationInput(
            new Id<Conversation>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            true
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
