using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Ouranos.Pantheon.Modules.Shared.API.Queries;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants;

[ExtendObjectType<Query>]
public sealed class GetAllAssistantsQuery
{
    /// <summary>
    ///     Gets a queryable list of assistants.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />.
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>A list of assistants.</returns>
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Assistant>> GetAllAssistants(
        [Service] IScopedDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllAssistantsInput(), cancellationToken);
        return wrapper.Value;
    }
}
