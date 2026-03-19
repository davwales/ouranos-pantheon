using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants;

public sealed class GetAllAssistantsHandler
    : IPantheonHandler<GetAllAssistantsInput, List<GetAllAssistantsResponse>>
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

    public async Task<List<GetAllAssistantsResponse>> Handle(
        GetAllAssistantsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all assistants query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var assistants = await _dbContext.Assistants
            .AsQueryable()
            .AsNoTracking()
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
            )
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all assistants request.");
        return assistants;
    }
}
