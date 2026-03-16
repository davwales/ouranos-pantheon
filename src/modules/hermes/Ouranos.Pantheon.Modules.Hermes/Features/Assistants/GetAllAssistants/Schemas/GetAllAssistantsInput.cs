using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants.Schemas;

public sealed record GetAllAssistantsInput : IQuery<WrapperResponse<IQueryable<Assistant>>>;
