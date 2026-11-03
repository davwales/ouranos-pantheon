using System.ClientModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateConversationName;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateConversationName.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;
using MessageDto = Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos.MessageDto;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.GenerateConversationName;

public sealed class GenerateConversationNameConsumerTests
{
    private readonly ILogger<GenerateConversationNameConsumer> _logger = Substitute.For<
        ILogger<GenerateConversationNameConsumer>
    >();
    private readonly HermesDbContext _dbContext = DbContextExtensions.Mock<HermesDbContext>();
    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly GenerateConversationNameConsumer _consumer;

    public GenerateConversationNameConsumerTests()
    {
        _consumer = new GenerateConversationNameConsumer(
            _logger,
            _dbContext,
            _mlClient,
            Options.Create(NamingEnabledOptions())
        );
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GenerateConversationNameConsumer(
                null!,
                _dbContext,
                _mlClient,
                Options.Create(new HermesOptions())
            )
        );
    }

    [Fact]
    public void Constructor_WhenDbContextIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GenerateConversationNameConsumer(
                _logger,
                null!,
                _mlClient,
                Options.Create(new HermesOptions())
            )
        );
    }

    [Fact]
    public void Constructor_WhenMlClientIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GenerateConversationNameConsumer(
                _logger,
                _dbContext,
                null!,
                Options.Create(new HermesOptions())
            )
        );
    }

    [Fact]
    public void Constructor_WhenOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GenerateConversationNameConsumer(_logger, _dbContext, _mlClient, null!)
        );
    }

    [Fact]
    public async Task Handle_WhenModelReturnsAName_ShouldSaveIt()
    {
        // Arrange
        var conversation = await SeedConversationAsync();
        ModelReturns("  Trip planning  ");

        // Act
        await _consumer.Handle(new GenerateConversationNameRequested(conversation.Id));

        // Assert
        (await _dbContext.Conversations.FindAsync(conversation.Id))
            .ShouldNotBeNull()
            .Name.ShouldBe("Trip planning");
    }

    [Fact]
    public async Task Handle_WhenMessagesEndWithAReply_ShouldSendThemInOrderAndAskForAName()
    {
        // Arrange
        var conversation = await SeedConversationAsync(
            ("Hi there", Role.Assistant),
            ("System prompt", Role.System),
            ("Hello", Role.User)
        );
        ModelReturns("Greeting");

        // Act
        await _consumer.Handle(new GenerateConversationNameRequested(conversation.Id));

        // Assert
        await _mlClient
            .Received(1)
            .GenerateResponseAsync(
                "test-model",
                "Generate a name.",
                Arg.Is<IReadOnlyList<ResponseInputItemDto>>(input =>
                    input.SequenceEqual(
                        new ResponseInputItemDto[]
                        {
                            new MessageInputItemDto(new MessageDto("Hi there", RoleDto.Assistant)),
                            new MessageInputItemDto(
                                new MessageDto("System prompt", RoleDto.System)
                            ),
                            new MessageInputItemDto(new MessageDto("Hello", RoleDto.User)),
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
    public async Task Handle_WhenLastMessageIsNotFromTheUser_ShouldAppendANamingInstruction()
    {
        // Arrange
        var conversation = await SeedConversationAsync(
            ("Hello", Role.User),
            ("Hi there", Role.Assistant)
        );
        ModelReturns("Greeting");

        // Act
        await _consumer.Handle(new GenerateConversationNameRequested(conversation.Id));

        // Assert
        await _mlClient
            .Received(1)
            .GenerateResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Is<IReadOnlyList<ResponseInputItemDto>>(input =>
                    input.Count == 3
                    && input[2]
                        == new MessageInputItemDto(
                            new MessageDto("Generate a name using the prior content.", RoleDto.User)
                        )
                ),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenNameIsTooLong_ShouldTruncateTo60Characters()
    {
        // Arrange
        var conversation = await SeedConversationAsync();
        ModelReturns(new string('a', 80));

        // Act
        await _consumer.Handle(new GenerateConversationNameRequested(conversation.Id));

        // Assert
        (await _dbContext.Conversations.FindAsync(conversation.Id))
            .ShouldNotBeNull()
            .Name.Length.ShouldBe(60);
    }

    [Fact]
    public async Task Handle_WhenRenamedBeforeNaming_ShouldNotCallTheModel()
    {
        // Arrange
        var conversation = await SeedConversationAsync();
        conversation.Update(
            "My name",
            conversation.PersonaId,
            conversation.ModelConfigId,
            null,
            [],
            true,
            null
        );
        await _dbContext.SaveChangesAsync();

        // Act
        await _consumer.Handle(new GenerateConversationNameRequested(conversation.Id));

        // Assert
        await _mlClient
            .DidNotReceiveWithAnyArgs()
            .GenerateResponseAsync(default!, default!, default!);
        conversation.Name.ShouldBe("My name");
    }

    [Fact]
    public async Task Handle_WhenConversationWasDeleted_ShouldDoNothing()
    {
        // Arrange
        var conversationId = DatabaseExtensions.CreateId<Conversation>();

        // Act
        await _consumer.Handle(new GenerateConversationNameRequested(conversationId));

        // Assert
        await _mlClient
            .DidNotReceiveWithAnyArgs()
            .GenerateResponseAsync(default!, default!, default!);
    }

    [Fact]
    public async Task Handle_WhenNamingIsDisabled_ShouldNotCallTheModel()
    {
        // Arrange
        var conversation = await SeedConversationAsync();
        var consumer = new GenerateConversationNameConsumer(
            _logger,
            _dbContext,
            _mlClient,
            Options.Create(new HermesOptions())
        );

        // Act
        await consumer.Handle(new GenerateConversationNameRequested(conversation.Id));

        // Assert
        await _mlClient
            .DidNotReceiveWithAnyArgs()
            .GenerateResponseAsync(default!, default!, default!);
    }

    public static TheoryData<Exception> ModelFailures =>
        new()
        {
            new HttpRequestException("ML service unavailable"),
            new ClientResultException("Bad response"),
            new TaskCanceledException("The model timed out"),
        };

    [Theory]
    [MemberData(nameof(ModelFailures))]
    public async Task Handle_WhenTheModelFails_ShouldKeepTheDefaultName(Exception failure)
    {
        // Arrange
        var conversation = await SeedConversationAsync();
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
            .Returns<GeneratedResponse>(_ => throw failure);

        // Act
        await _consumer.Handle(new GenerateConversationNameRequested(conversation.Id));

        // Assert
        conversation.HasDefaultName.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenCancelledDuringNaming_ShouldPropagateTheCancellation()
    {
        // Arrange
        var conversation = await SeedConversationAsync();
        using var cancellation = new CancellationTokenSource();
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
            {
                cancellation.Cancel();
                throw new OperationCanceledException(cancellation.Token);
            });

        // Act
        var handle = async () =>
            await _consumer.Handle(
                new GenerateConversationNameRequested(conversation.Id),
                cancellation.Token
            );

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var conversationId = DatabaseExtensions.CreateId<Conversation>();

        // Act
        var handle = async () =>
            await _consumer.Handle(
                new GenerateConversationNameRequested(conversationId),
                new CancellationToken(true)
            );

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    private async Task<Conversation> SeedConversationAsync(
        params (string Content, Role Role)[] messages
    )
    {
        var conversationId = DatabaseExtensions.CreateId<Conversation>();
        var entries = messages.Length > 0 ? messages : [("Plan a trip", Role.User)];
        var conversation = Conversation.Create(
            conversationId,
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            entries.Select(
                (m, index) =>
                    Message.Create(
                        DatabaseExtensions.CreateId<Message>(),
                        conversationId,
                        m.Content,
                        m.Role,
                        index
                    )
            ),
            []
        );
        await _dbContext.SeedData(conversation);

        return conversation;
    }

    private void ModelReturns(string content)
    {
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
            .Returns(new GeneratedResponse(content, null));
    }

    private static HermesOptions NamingEnabledOptions()
    {
        return new HermesOptions(
            ConversationNameSystemPrompt: "Generate a name.",
            ConversationNameModel: "test-model",
            ConversationNameReasoningEffort: ReasoningEffort.None,
            CompactionSummaryPrompt: HermesOptions.DefaultCompactionSummaryPrompt,
            CompactionTemperature: HermesOptions.DefaultCompactionTemperature,
            CompactionMaxTokens: HermesOptions.DefaultCompactionMaxTokens,
            CompactionReasoningEffort: ReasoningEffort.Low,
            ImageGeneration: new HermesImageGenerationOptions()
        );
    }
}
