using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe;

public sealed class ImportRecipeHandler(
    ILogger<ImportRecipeHandler> logger,
    IMessageBus bus,
    IHestiaMartenStore store
) : IPantheonHandler<ImportRecipeInput, IdResponse<Recipe>>
{
    private readonly ILogger<ImportRecipeHandler> _logger = Guard.Against.Null(logger);
    private readonly IMessageBus _bus = Guard.Against.Null(bus);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<IdResponse<Recipe>> Handle(
        ImportRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle import recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        Guard.Against.NullOrWhiteSpace(command.Url);
        Guard.Against.OutOfRange(command.Url.Length, nameof(command.Url), 1, 2_000);
        Guard.Against.InvalidInput(
            command.Url,
            nameof(command.Url),
            static url =>
                Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
            "Url must be an absolute http(s) URL."
        );

        var recipeId = Guid.NewGuid();
        var result = Recipe.CreateImport(recipeId, command.Url, DateTimeOffset.UtcNow);

        using var session = _store.LightweightSession();
        session.Events.StartStream(recipeId, [.. result.Events]);
        await session.SaveChangesAsync(cancellationToken);

        var id = new Id<Recipe>(recipeId.ToString());
        await _bus.PublishAsync(new ImportRecipeRequested(id, command.Url, DateTimeOffset.UtcNow));

        var response = new IdResponse<Recipe>(id);

        _logger.LogDebug("Successfully handled import recipe request for recipe '{recipeId}'.", id);
        return response;
    }
}
