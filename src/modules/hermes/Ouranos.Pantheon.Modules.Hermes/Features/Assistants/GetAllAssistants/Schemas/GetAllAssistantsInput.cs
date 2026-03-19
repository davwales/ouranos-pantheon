using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants.Schemas;

public sealed record GetAllAssistantsInput(
    string[]? Filter = null
) : IQuery<WrapperResponse<IQueryable<GetAllAssistantsResponse>>>;
