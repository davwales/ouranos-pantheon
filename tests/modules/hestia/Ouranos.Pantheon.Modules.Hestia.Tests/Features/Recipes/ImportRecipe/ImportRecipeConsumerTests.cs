using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ImportRecipe;

public sealed class ImportRecipeConsumerTests
{
    private readonly IRecipeScraper _scraper = Substitute.For<IRecipeScraper>();
    private readonly IRecipeExtractor _extractor = Substitute.For<IRecipeExtractor>();
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly IEventStoreOperations _events = Substitute.For<IEventStoreOperations>();
    private readonly ImportRecipeConsumer _consumer;

    public ImportRecipeConsumerTests()
    {
        _session.Events.Returns(_events);
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);

        _consumer = new ImportRecipeConsumer(
            Substitute.For<ILogger<ImportRecipeConsumer>>(),
            _scraper,
            _extractor,
            _store
        );
    }

    private static ImportRecipeRequested ValidMessage()
    {
        return new ImportRecipeRequested(
            new Id<Recipe>(Guid.NewGuid().ToString()),
            "https://example.com/recipe",
            DateTimeOffset.UtcNow
        );
    }

    private static ScrapedJsonLdRecipe ValidScrapedRecipe()
    {
        return new ScrapedJsonLdRecipe(
            "Chocolate Cake",
            """{"@type":"Recipe","name":"Chocolate Cake","recipeIngredient":["2 cups flour","1/2 tsp salt","3 eggs"],"recipeInstructions":["Mix everything.","Bake at 350°F."]}"""
        );
    }

    private static ExtractedRecipe ValidExtractedRecipe(string? description = null)
    {
        return new ExtractedRecipe(
            "Chocolate Cake",
            description,
            [
                new ExtractedIngredient(2m, "cup", "flour"),
                new ExtractedIngredient(0.5m, "tsp", "salt"),
                new ExtractedIngredient(3m, null, "eggs"),
            ],
            ["Mix everything.", "Bake at 350°F."]
        );
    }

    private void StubRecipe(Guid recipeId)
    {
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Recipe?>(
                    Recipe
                        .CreateImport(recipeId, "https://example.com/recipe", DateTimeOffset.UtcNow)
                        .State
                )
            );
    }

    [Fact]
    public async Task Handle_WhenScrapeAndExtractSucceed_ShouldAppendRecipeImportSucceededEvent()
    {
        // Arrange
        var message = ValidMessage();
        var recipeId = Guid.Parse(message.RecipeId.Value);
        StubRecipe(recipeId);
        _scraper
            .ScrapeAsync(message.Url, Arg.Any<CancellationToken>())
            .Returns(ValidScrapedRecipe());
        _extractor
            .ExtractAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValidExtractedRecipe());
        Guid? capturedStreamId = null;
        object[]? capturedEvents = null;
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call =>
            {
                capturedStreamId = call.Arg<Guid>();
                capturedEvents = call.Arg<object[]>();
            });

        // Act
        await _consumer.Handle(message, CancellationToken.None);

        // Assert
        capturedStreamId.ShouldBe(recipeId);
        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(1);
        var @event = capturedEvents[0].ShouldBeOfType<RecipeImportSucceeded>();
        @event.Title.ShouldBe("Chocolate Cake");
        @event.Notes.ShouldBe(string.Empty);
        @event.Steps.Count.ShouldBe(2);
        @event.Steps[0].ShouldBe(new Step("Mix everything."));
        @event.Steps[1].ShouldBe(new Step("Bake at 350°F."));
        @event.Ingredients.Count.ShouldBe(3);
        @event.Ingredients[0].ShouldBe(new Ingredient(2m, "cup", "flour"));
        @event.Ingredients[1].ShouldBe(new Ingredient(0.5m, "tsp", "salt"));
        @event.Ingredients[2].ShouldBe(new Ingredient(3m, "whole", "eggs"));
        @event.ImportedAt.ShouldNotBe(default);

        await _session.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenExtractionIncludesDescription_ShouldPersistAsNotes()
    {
        // Arrange
        var message = ValidMessage();
        var recipeId = Guid.Parse(message.RecipeId.Value);
        StubRecipe(recipeId);
        _scraper
            .ScrapeAsync(message.Url, Arg.Any<CancellationToken>())
            .Returns(ValidScrapedRecipe());
        _extractor
            .ExtractAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValidExtractedRecipe(description: "A rich chocolate cake."));
        object[]? capturedEvents = null;
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call => capturedEvents = call.Arg<object[]>());

        // Act
        await _consumer.Handle(message, CancellationToken.None);

        // Assert
        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(1);
        var @event = capturedEvents[0].ShouldBeOfType<RecipeImportSucceeded>();
        @event.Notes.ShouldBe("A rich chocolate cake.");
    }

    [Fact]
    public async Task Handle_WhenNoRecipeMetadata_ShouldAppendRecipeImportFailedEvent()
    {
        // Arrange
        var message = ValidMessage();
        var recipeId = Guid.Parse(message.RecipeId.Value);
        StubRecipe(recipeId);
        _scraper
            .ScrapeAsync(message.Url, Arg.Any<CancellationToken>())
            .Returns((ScrapedJsonLdRecipe?)null);
        object[]? capturedEvents = null;
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call => capturedEvents = call.Arg<object[]>());

        // Act
        var act = async () => await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        await _extractor.DidNotReceiveWithAnyArgs().ExtractAsync(default!, default);
        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(1);
        var @event = capturedEvents[0].ShouldBeOfType<RecipeImportFailed>();
        @event.Reason.ShouldBe("The page contains no usable recipe metadata.");
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExtractionFails_ShouldAppendRecipeImportFailedEvent()
    {
        // Arrange
        var message = ValidMessage();
        var recipeId = Guid.Parse(message.RecipeId.Value);
        StubRecipe(recipeId);
        _scraper
            .ScrapeAsync(message.Url, Arg.Any<CancellationToken>())
            .Returns(ValidScrapedRecipe());
        _extractor
            .ExtractAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ExtractedRecipe?)null);
        object[]? capturedEvents = null;
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call => capturedEvents = call.Arg<object[]>());

        // Act
        var act = async () => await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(1);
        var @event = capturedEvents[0].ShouldBeOfType<RecipeImportFailed>();
        @event.Reason.ShouldBe("Recipe extraction produced no usable result.");
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExtractionFailsValidation_ShouldAppendRecipeImportFailedEvent()
    {
        // Arrange
        var message = ValidMessage();
        var recipeId = Guid.Parse(message.RecipeId.Value);
        StubRecipe(recipeId);
        _scraper
            .ScrapeAsync(message.Url, Arg.Any<CancellationToken>())
            .Returns(ValidScrapedRecipe());
        _extractor
            .ExtractAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                new ExtractedRecipe(
                    "Chocolate Cake",
                    null,
                    [new ExtractedIngredient(null, null, " ")],
                    [" "]
                )
            );
        object[]? capturedEvents = null;
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call => capturedEvents = call.Arg<object[]>());

        // Act
        var act = async () => await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(1);
        var @event = capturedEvents[0].ShouldBeOfType<RecipeImportFailed>();
        @event.Reason.ShouldBe("The extracted recipe failed validation.");
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenScraperThrows_ShouldAppendRecipeImportFailedEventAndNotRethrow()
    {
        // Arrange
        var message = ValidMessage();
        var recipeId = Guid.Parse(message.RecipeId.Value);
        StubRecipe(recipeId);
        _scraper
            .ScrapeAsync(message.Url, Arg.Any<CancellationToken>())
            .Returns(
                Task.FromException<ScrapedJsonLdRecipe?>(
                    new HttpRequestException("Network unavailable")
                )
            );
        object[]? capturedEvents = null;
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call => capturedEvents = call.Arg<object[]>());

        // Act
        var act = async () => await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(1);
        var @event = capturedEvents[0].ShouldBeOfType<RecipeImportFailed>();
        @event.Reason.ShouldBe("Network unavailable");
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExtractorThrows_ShouldAppendRecipeImportFailedEventAndNotRethrow()
    {
        // Arrange
        var message = ValidMessage();
        var recipeId = Guid.Parse(message.RecipeId.Value);
        StubRecipe(recipeId);
        _scraper
            .ScrapeAsync(message.Url, Arg.Any<CancellationToken>())
            .Returns(ValidScrapedRecipe());
        _extractor
            .ExtractAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromException<ExtractedRecipe?>(
                    new HttpRequestException("ML service unavailable")
                )
            );
        object[]? capturedEvents = null;
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call => capturedEvents = call.Arg<object[]>());

        // Act
        var act = async () => await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(1);
        var @event = capturedEvents[0].ShouldBeOfType<RecipeImportFailed>();
        @event.Reason.ShouldBe("ML service unavailable");
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSaveFailsAfterAppendingSuccess_ShouldPropagateException()
    {
        // Arrange
        var message = ValidMessage();
        var recipeId = Guid.Parse(message.RecipeId.Value);
        StubRecipe(recipeId);
        _scraper
            .ScrapeAsync(message.Url, Arg.Any<CancellationToken>())
            .Returns(ValidScrapedRecipe());
        _extractor
            .ExtractAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ValidExtractedRecipe());
        _session
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Database unavailable")));

        // Act
        var act = async () => await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenRecipeDoesNotExist_ShouldCompleteWithoutSaving()
    {
        // Arrange
        var message = ValidMessage();
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(null));

        // Act
        var act = async () => await _consumer.Handle(message, CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
        await _scraper.DidNotReceiveWithAnyArgs().ScrapeAsync(default!, default);
        await _session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var message = ValidMessage();
        var ct = new CancellationToken(true);

        // Act
        var act = async () => await _consumer.Handle(message, ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
        await _scraper.DidNotReceiveWithAnyArgs().ScrapeAsync(default!, default);
    }
}
