using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe;

public sealed class ImportRecipeHandler(ILogger<ImportRecipeHandler> logger, IMessageBus bus)
    : IPantheonHandler<ImportRecipeInput, IdResponse<Recipe>>
{
    private readonly ILogger<ImportRecipeHandler> _logger = Guard.Against.Null(logger);
    private readonly IMessageBus _bus = Guard.Against.Null(bus);

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

        var recipeId = new Id<Recipe>(Guid.NewGuid().ToString());

        await _bus.PublishAsync(
            new ImportRecipeRequested(recipeId, command.Url, DateTimeOffset.UtcNow)
        );

        var response = new IdResponse<Recipe>(recipeId);

        _logger.LogDebug(
            "Successfully handled import recipe request for recipe '{recipeId}'.",
            recipeId
        );
        return response;
    }
}
