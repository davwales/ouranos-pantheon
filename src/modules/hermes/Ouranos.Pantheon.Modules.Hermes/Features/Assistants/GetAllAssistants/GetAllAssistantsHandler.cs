using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants;

public sealed class GetAllAssistantsHandler
    : QueryHandler<GetAllAssistantsInput, WrapperResponse<IQueryable<GetAllAssistantsResponse>>>
{
    private static readonly FilterBuilder<Assistant> FilterBuilder = new FilterBuilder<Assistant>()
        .On(nameof(Assistant.AssistantName), a => a.AssistantName)
        .On(nameof(Assistant.Model), a => a.Model);

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

    public override async Task<WrapperResponse<IQueryable<GetAllAssistantsResponse>>> Handle(
        GetAllAssistantsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all assistants query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var queryable = _dbContext.Assistants
            .AsQueryable()
            .FilterBy(query.Filter, FilterBuilder)
            .Select(a => new GetAllAssistantsResponse(
                    a.Id,
                    a.Model,
                    a.SystemPrompt,
                    a.AssistantName,
                    a.UserName,
                    a.Temperature,
                    a.MaxTokens,
                    a.RepeatPenalty
                )
            );

        _logger.LogDebug("Successfully handled get all assistants request.");
        return await Task.FromResult(new WrapperResponse<IQueryable<GetAllAssistantsResponse>>(queryable));
    }
}
