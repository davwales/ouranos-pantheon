using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Marten;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.AskKitchenAssistant;

public sealed class KitchenAssistantTests
{
    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IQuerySession _session = Substitute.For<IQuerySession>();
    private readonly KitchenAssistant _assistant;

    public KitchenAssistantTests()
    {
        _store.QuerySession().Returns(_session);
        _assistant = new KitchenAssistant(
            Substitute.For<ILogger<KitchenAssistant>>(),
            _mlClient,
            _store,
            Options.Create(
                new HestiaOptions(
                    new RecipeImportOptions(),
                    new KitchenAssistantOptions("kitchen-model", 512, 0.3f)
                )
            )
        );
    }

    private static Recipe BuildRecipe(Guid id, string notes = "Best served warm.")
    {
        return Recipe
            .Create(
                id,
                "Chocolate Cake",
                null,
                [new Step("Mix the batter."), new Step("Bake for 30 minutes.")],
                [new Ingredient(1.5m, "cups", "flour"), new Ingredient(0m, "pinch", "salt")],
                notes
            )
            .State;
    }

    private static async IAsyncEnumerable<ChatCompletionChunk> EmptyStream(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        yield break;
    }

    private static async Task<List<AssistantEvent>> CollectAsync(
        IAsyncEnumerable<AssistantEvent> stream
    )
    {
        var events = new List<AssistantEvent>();
        await foreach (var assistantEvent in stream)
        {
            events.Add(assistantEvent);
        }

        return events;
    }

    [Fact]
    public async Task Handle_WhenRecipeExists_ShouldStreamWithConfiguredModelAndRecipePrompt()
    {
        // Arrange
        var id = Guid.NewGuid();

        _session
            .LoadAsync<Recipe>(id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(BuildRecipe(id)));

        List<MessageDto>? sentMessages = null;

        _mlClient
            .StreamChatCompletionAsync(
                "kitchen-model",
                Arg.Do<List<MessageDto>>(m => sentMessages = m),
                0.3f,
                512,
                Arg.Any<float?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(EmptyStream());

        var input = new AssistantCompletionInput<KitchenAssistantContext>(
            [new AssistantMessageInput("Can I use butter?", AssistantRole.User)],
            new KitchenAssistantContext(new Id<Recipe>(id.ToString()))
        );

        // Act
        var events = await CollectAsync(_assistant.Handle(input));

        // Assert
        events.ShouldBe([new AssistantDoneEvent()]);
        sentMessages.ShouldNotBeNull();
        sentMessages.Count.ShouldBe(2);
        sentMessages[0].Role.ShouldBe(RoleDto.System);
        sentMessages[0].Content.ShouldContain("# Recipe: Chocolate Cake");
        sentMessages[1].ShouldBe(new MessageDto("Can I use butter?", RoleDto.User));
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(null));

        var input = new AssistantCompletionInput<KitchenAssistantContext>(
            [],
            new KitchenAssistantContext(new Id<Recipe>(Guid.NewGuid().ToString()))
        );

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => CollectAsync(_assistant.Handle(input)));
    }

    [Fact]
    public async Task Handle_WhenRecipeIdIsNotAGuid_ShouldThrowNotFoundException()
    {
        // Arrange
        var input = new AssistantCompletionInput<KitchenAssistantContext>(
            [],
            new KitchenAssistantContext(new Id<Recipe>("not-a-guid"))
        );

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => CollectAsync(_assistant.Handle(input)));
    }
}
