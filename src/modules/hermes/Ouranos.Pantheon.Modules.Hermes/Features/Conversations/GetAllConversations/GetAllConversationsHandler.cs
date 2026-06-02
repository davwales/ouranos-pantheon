using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations;

public sealed class GetAllConversationsHandler(
    ILogger<GetAllConversationsHandler> logger,
    HermesDbContext dbContext,
    IFlagsmithClient flagsmith
) : IPantheonHandler<GetAllConversationsInput, List<GetAllConversationsResponse>>
{
    private static readonly FilterBuilder<Conversation> FilterBuilder =
        new FilterBuilder<Conversation>()
            .On(nameof(Conversation.Name), c => c.Name, caseInsensitive: true)
            .On(nameof(Conversation.IsPublic), c => c.IsPublic);

    private static readonly SortBuilder<Conversation> SortBuilder = new SortBuilder<Conversation>()
        .On(nameof(Conversation.Name), c => c.Name)
        .On(nameof(Conversation.UpdatedAt), c => c.UpdatedAt)
        .Default(c => c.UpdatedAt);

    private readonly HermesDbContext _dbContext = Guard.Against.Null(dbContext);
    private readonly IFlagsmithClient _flagsmith = Guard.Against.Null(flagsmith);
    private readonly ILogger<GetAllConversationsHandler> _logger = Guard.Against.Null(logger);

    public async Task<List<GetAllConversationsResponse>> Handle(
        GetAllConversationsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all conversations query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var flags = await _flagsmith.GetEnvironmentFlags();
        var isPublicMode = await flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode);

        IQueryable<Conversation> dbQuery = _dbContext
            .Conversations.AsQueryable()
            .AsNoTracking()
            .Include(c => c.Folder);

        if (query.FolderId is not null)
        {
            dbQuery = dbQuery.Where(c => c.FolderId == query.FolderId);
        }

        if (isPublicMode)
        {
            dbQuery = dbQuery.Where(c => c.IsPublic && (c.FolderId == null || c.Folder.IsPublic));
        }

        var conversations = await dbQuery
            .FilterBy(query.Filter, FilterBuilder)
            .SortBy(query.SortField, query.SortDirection, SortBuilder)
            .Select(c => new GetAllConversationsResponse(
                c.Id,
                c.Name,
                c.IsPublic,
                c.FolderId,
                c.CreatedAt,
                c.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all conversations request.");
        return conversations;
    }
}
