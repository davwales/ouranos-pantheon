using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;
using Ouranos.Pantheon.Modules.Hermes.Shared.Infra.Database;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants;

public sealed class GetAllAssistantsHandler
    : QueryHandler<GetAllAssistantsInput, WrapperResponse<IQueryable<Assistant>>>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<GetAllAssistantsHandler> _logger;

    public GetAllAssistantsHandler(
        ILogger<GetAllAssistantsHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<WrapperResponse<IQueryable<Assistant>>> Handle(
        GetAllAssistantsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all assistants query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var queryable = _dbContext.Assistants.AsQueryable();
        var response = new WrapperResponse<IQueryable<Assistant>>(queryable);

        _logger.LogDebug("Successfully handled get all assistants request.");
        return await Task.FromResult(response);
    }
}
