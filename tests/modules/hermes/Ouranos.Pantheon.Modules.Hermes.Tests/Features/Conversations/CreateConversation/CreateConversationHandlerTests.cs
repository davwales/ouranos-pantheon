using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;
using MessageDto = Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos.MessageDto;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.CreateConversation;

public sealed class CreateConversationHandlerTests
{
    private readonly Fixture _fixture = new();
    private readonly CreateConversationHandler _handler;
    private readonly ILogger<CreateConversationHandler> _logger = Substitute.For<
        ILogger<CreateConversationHandler>
    >();
    private readonly HermesDbContext _dbContext;
    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly Persona _persona = Persona.Create(
        DatabaseExtensions.CreateId<Persona>(),
        "Persona",
        "A persona."
    );
    private readonly ModelConfig _modelConfig = ModelConfig.Create(
        DatabaseExtensions.CreateId<ModelConfig>(),
        "Model",
        "model-identifier",
        "System prompt."
    );

    public CreateConversationHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _dbContext.Personas.Add(_persona);
        _dbContext.ModelConfigs.Add(_modelConfig);
        _dbContext.SaveChanges();
        var options = Options.Create(new HermesOptions());
        _handler = new CreateConversationHandler(_logger, _dbContext, _mlClient, options);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateConversationAndReturnResponse()
    {
        // Arrange
        var personaId = _persona.Id;
        var modelConfigId = _modelConfig.Id;
        var name = _fixture.Create<string>();
        var command = new CreateConversationInput(personaId, modelConfigId, [], [], name);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CreateConversationResponse>();
        result.Id.ShouldNotBe(default);
        result.Name.ShouldBe(name);

        var savedConversation = await _dbContext.Conversations.FindAsync(result.Id);
        savedConversation.ShouldNotBeNull();
        savedConversation.Name.ShouldBe(name);
    }

    [Fact]
    public async Task Handle_WhenReasoningEffortProvided_ShouldSaveIt()
    {
        // Arrange
        var command = new CreateConversationInput(
            _persona.Id,
            _modelConfig.Id,
            [],
            [],
            _fixture.Create<string>(),
            ReasoningEffort: ReasoningEffort.Medium
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await _dbContext.Conversations.FindAsync(result.Id);
        saved.ShouldNotBeNull();
        saved.ReasoningEffort.ShouldBe(ReasoningEffort.Medium);
    }

    [Fact]
    public async Task Handle_WhenNameIsNull_ShouldDefaultToNewConversation()
    {
        // Arrange
        var personaId = _persona.Id;
        var modelConfigId = _modelConfig.Id;
        var command = new CreateConversationInput(personaId, modelConfigId, [], []);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.ShouldBe("New Conversation");
    }

    [Fact]
    public async Task Handle_WhenNameGenerationEnabled_ShouldCallMlClient()
    {
        // Arrange
        var personaId = _persona.Id;
        var modelConfigId = _modelConfig.Id;
        var generatedName = _fixture.Create<string>();

        _mlClient
            .GenerateResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new GeneratedResponse(generatedName, null));

        var handler = new CreateConversationHandler(
            _logger,
            _dbContext,
            _mlClient,
            Options.Create(
                new HermesOptions(
                    ConversationNameSystemPrompt: "Generate a name.",
                    ConversationNameModel: "test-model",
                    ConversationNameReasoningEffort: ReasoningEffort.None,
                    CompactionSummaryPrompt: HermesOptions.DefaultCompactionSummaryPrompt,
                    CompactionTemperature: HermesOptions.DefaultCompactionTemperature,
                    CompactionMaxTokens: HermesOptions.DefaultCompactionMaxTokens,
                    CompactionReasoningEffort: ReasoningEffort.Low,
                    ImageGeneration: new HermesImageGenerationOptions()
                )
            )
        );
        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [new CreateConversationMessageInput("Hello", Role.User)]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await _mlClient
            .Received(1)
            .GenerateResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            );
        result.Name.ShouldBe(
            generatedName.Trim().Length > 60 ? generatedName.Trim()[..60] : generatedName.Trim()
        );
    }

    [Fact]
    public async Task Handle_WhenNameGenerationReturnsLongString_ShouldTruncateTo60Chars()
    {
        // Arrange
        var personaId = _persona.Id;
        var modelConfigId = _modelConfig.Id;
        var longName = new string('a', 80);

        _mlClient
            .GenerateResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new GeneratedResponse(longName, null));

        var handler = new CreateConversationHandler(
            _logger,
            _dbContext,
            _mlClient,
            Options.Create(
                new HermesOptions(
                    ConversationNameSystemPrompt: "Generate a name.",
                    ConversationNameModel: "test-model",
                    ConversationNameReasoningEffort: ReasoningEffort.None,
                    CompactionSummaryPrompt: HermesOptions.DefaultCompactionSummaryPrompt,
                    CompactionTemperature: HermesOptions.DefaultCompactionTemperature,
                    CompactionMaxTokens: HermesOptions.DefaultCompactionMaxTokens,
                    CompactionReasoningEffort: ReasoningEffort.Low,
                    ImageGeneration: new HermesImageGenerationOptions()
                )
            )
        );
        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [new CreateConversationMessageInput("Hello", Role.User)]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Length.ShouldBe(60);
    }

    [Fact]
    public async Task Handle_WhenNameGenerationThrows_ShouldFallbackToDefaultName()
    {
        // Arrange
        var personaId = _persona.Id;
        var modelConfigId = _modelConfig.Id;

        _mlClient
            .GenerateResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns<GeneratedResponse>(_ =>
                throw new HttpRequestException("ML service unavailable")
            );

        var handler = new CreateConversationHandler(
            _logger,
            _dbContext,
            _mlClient,
            Options.Create(
                new HermesOptions(
                    ConversationNameSystemPrompt: "Generate a name.",
                    ConversationNameModel: "test-model",
                    ConversationNameReasoningEffort: ReasoningEffort.None,
                    CompactionSummaryPrompt: HermesOptions.DefaultCompactionSummaryPrompt,
                    CompactionTemperature: HermesOptions.DefaultCompactionTemperature,
                    CompactionMaxTokens: HermesOptions.DefaultCompactionMaxTokens,
                    CompactionReasoningEffort: ReasoningEffort.Low,
                    ImageGeneration: new HermesImageGenerationOptions()
                )
            )
        );
        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [new CreateConversationMessageInput("Hello", Role.User)]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.ShouldBe("New Conversation");
    }

    [Fact]
    public async Task Handle_WhenTraitIdsProvided_ShouldLoadTraitsFromDb()
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
            _fixture.Create<string>()
        );
        await _dbContext.SeedData(trait1, trait2);

        var personaId = _persona.Id;
        var modelConfigId = _modelConfig.Id;
        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [trait1.Id, trait2.Id],
            [],
            _fixture.Create<string>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedConversation = await _dbContext.Conversations.FindAsync(result.Id);
        savedConversation.ShouldNotBeNull();
        savedConversation.Traits.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenLoraIdsProvided_ShouldLoadLorasFromDb()
    {
        // Arrange
        var lora = Lora.Create(
            new Id<Lora>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        await _dbContext.SeedData(lora);

        var command = new CreateConversationInput(
            _persona.Id,
            _modelConfig.Id,
            [],
            [],
            _fixture.Create<string>(),
            LoraIds: [lora.Id]
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedConversation = await _dbContext.Conversations.FindAsync(result.Id);
        savedConversation.ShouldNotBeNull();
        savedConversation.Loras.ShouldHaveSingleItem().Id.ShouldBe(lora.Id);
    }

    [Fact]
    public async Task Handle_WhenMessagesProvided_ShouldAssignSequentialSortOrder()
    {
        // Arrange
        var personaId = _persona.Id;
        var modelConfigId = _modelConfig.Id;

        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [
                new CreateConversationMessageInput("First", Role.User),
                new CreateConversationMessageInput("Second", Role.Assistant),
                new CreateConversationMessageInput("Third", Role.User),
            ],
            _fixture.Create<string>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedConversation = await _dbContext.Conversations.FindAsync(result.Id);
        savedConversation.ShouldNotBeNull();
        savedConversation.Messages.Count.ShouldBe(3);
        savedConversation
            .Messages.OrderBy(m => m.SortOrder)
            .Select(m => m.SortOrder)
            .ShouldBe([0, 1, 2]);
    }

    [Fact]
    public async Task Handle_WhenMessagesHaveParts_ShouldSaveTheParts()
    {
        // Arrange
        var command = new CreateConversationInput(
            _persona.Id,
            _modelConfig.Id,
            [],
            [
                new CreateConversationMessageInput("Hi", Role.User),
                new CreateConversationMessageInput(
                    "Hello",
                    Role.Assistant,
                    [new ReasoningMessagePartDto("Thinking"), new TextMessagePartDto("Hello")]
                ),
            ],
            _fixture.Create<string>()
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var assistant = await _dbContext
            .Messages.Include(m => m.Parts)
            .SingleAsync(m => m.Role == Role.Assistant);

        assistant.Parts.Count.ShouldBe(2);

        assistant
            .Parts.OrderBy(p => p.SortOrder)
            .First()
            .ShouldBeOfType<ReasoningMessagePart>()
            .Content.ShouldBe("Thinking");
    }

    [Fact]
    public async Task Handle_WhenSystemAndAssistantMessagesProvided_ShouldMapRolesCorrectly()
    {
        // Arrange
        var personaId = _persona.Id;
        var modelConfigId = _modelConfig.Id;

        _mlClient
            .GenerateResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new GeneratedResponse("Generated Name", null));

        var handler = new CreateConversationHandler(
            _logger,
            _dbContext,
            _mlClient,
            Options.Create(
                new HermesOptions(
                    ConversationNameSystemPrompt: "Generate a name.",
                    ConversationNameModel: "test-model",
                    ConversationNameReasoningEffort: ReasoningEffort.None,
                    CompactionSummaryPrompt: HermesOptions.DefaultCompactionSummaryPrompt,
                    CompactionTemperature: HermesOptions.DefaultCompactionTemperature,
                    CompactionMaxTokens: HermesOptions.DefaultCompactionMaxTokens,
                    CompactionReasoningEffort: ReasoningEffort.Low,
                    ImageGeneration: new HermesImageGenerationOptions()
                )
            )
        );

        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [
                new CreateConversationMessageInput("System prompt", Role.System),
                new CreateConversationMessageInput("Hello", Role.User),
                new CreateConversationMessageInput("Hi there", Role.Assistant),
            ]
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.ShouldBe("Generated Name");
        await _mlClient
            .Received(1)
            .GenerateResponseAsync(
                "test-model",
                "Generate a name.",
                Arg.Is<IReadOnlyList<ResponseInputItemDto>>(input =>
                    input.SequenceEqual(
                        new ResponseInputItemDto[]
                        {
                            new MessageInputItemDto(
                                new MessageDto("System prompt", RoleDto.System)
                            ),
                            new MessageInputItemDto(new MessageDto("Hello", RoleDto.User)),
                            new MessageInputItemDto(new MessageDto("Hi there", RoleDto.Assistant)),
                            new MessageInputItemDto(
                                new MessageDto(
                                    "Generate a name using the prior content.",
                                    RoleDto.User
                                )
                            ),
                        }
                    )
                ),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                ReasoningEffort.None,
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenTokenCountsProvided_ShouldRecordTokenUsage()
    {
        // Arrange
        var personaId = _persona.Id;
        var modelConfigId = _modelConfig.Id;
        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [new CreateConversationMessageInput("Hello", Role.User)],
            _fixture.Create<string>(),
            InputTokenCount: 100,
            OutputTokenCount: 50,
            TotalTokenCount: 150
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedConversation = await _dbContext.Conversations.FindAsync(result.Id);
        savedConversation.ShouldNotBeNull();
        savedConversation.InputTokenCount.ShouldBe(100);
        savedConversation.OutputTokenCount.ShouldBe(50);
        savedConversation.TotalTokenCount.ShouldBe(150);
    }

    [Fact]
    public async Task Handle_WhenTokenCountsNotProvided_ShouldNotRecordTokenUsage()
    {
        // Arrange
        var personaId = _persona.Id;
        var modelConfigId = _modelConfig.Id;
        var command = new CreateConversationInput(
            personaId,
            modelConfigId,
            [],
            [],
            _fixture.Create<string>()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedConversation = await _dbContext.Conversations.FindAsync(result.Id);
        savedConversation.ShouldNotBeNull();
        savedConversation.InputTokenCount.ShouldBeNull();
        savedConversation.OutputTokenCount.ShouldBeNull();
        savedConversation.TotalTokenCount.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenCreatingInFolder_ShouldSucceed()
    {
        // Arrange
        var folder = Folder.Create(new Id<Folder>(Guid.NewGuid().ToString()), "Unprotected Folder");
        await _dbContext.SeedData(folder);

        var command = new CreateConversationInput(
            _persona.Id,
            _modelConfig.Id,
            [],
            [],
            _fixture.Create<string>(),
            FolderId: folder.Id
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CreateConversationResponse>();
        result.Id.ShouldNotBe(default);

        var savedConversation = await _dbContext.Conversations.FindAsync(result.Id);
        savedConversation.ShouldNotBeNull();
        savedConversation.FolderId.ShouldBe(folder.Id);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreateConversationInput(
            _persona.Id,
            _modelConfig.Id,
            [],
            [],
            _fixture.Create<string>()
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    private async Task<Id<ToolSet>> SeedToolSetAsync()
    {
        var toolSet = ToolSet.Create(
            DatabaseExtensions.CreateId<ToolSet>(),
            _fixture.Create<string>(),
            null,
            null,
            []
        );
        await _dbContext.SeedData(toolSet);

        return toolSet.Id;
    }

    [Fact]
    public async Task Handle_WhenToolSetIdProvided_ShouldSaveIt()
    {
        // Arrange
        var toolSetId = await SeedToolSetAsync();
        var command = new CreateConversationInput(
            _persona.Id,
            _modelConfig.Id,
            [],
            [],
            _fixture.Create<string>(),
            ToolSetId: toolSetId
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await _dbContext.Conversations.FindAsync(result.Id);
        saved.ShouldNotBeNull();
        saved.ToolSetId.ShouldBe(toolSetId);
    }

    [Fact]
    public async Task Handle_WhenToolSetDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateConversationInput(
            _persona.Id,
            _modelConfig.Id,
            [],
            [],
            _fixture.Create<string>(),
            ToolSetId: DatabaseExtensions.CreateId<ToolSet>()
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
        _dbContext.Conversations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenALoraIdIsUnknown_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateConversationInput(
            _persona.Id,
            _modelConfig.Id,
            [],
            [],
            _fixture.Create<string>(),
            LoraIds: [new Id<Lora>(Guid.NewGuid().ToString())]
        );

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );
    }

    [Theory]
    [InlineData("persona")]
    [InlineData("model")]
    [InlineData("trait")]
    public async Task Handle_WhenAReferencedIdIsUnknown_ShouldThrowNotFoundException(string unknown)
    {
        // Arrange
        var command = new CreateConversationInput(
            unknown == "persona" ? new Id<Persona>(Guid.NewGuid().ToString()) : _persona.Id,
            unknown == "model" ? new Id<ModelConfig>(Guid.NewGuid().ToString()) : _modelConfig.Id,
            unknown == "trait" ? [new Id<Trait>(Guid.NewGuid().ToString())] : [],
            [],
            _fixture.Create<string>()
        );

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );
        exception.Message.ShouldContain(unknown);
        (await _dbContext.Conversations.CountAsync()).ShouldBe(0);
    }
}
