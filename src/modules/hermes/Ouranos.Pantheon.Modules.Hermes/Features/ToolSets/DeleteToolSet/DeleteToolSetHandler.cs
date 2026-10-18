using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.DeleteToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.DeleteToolSet;

public sealed class DeleteToolSetHandler
    : IPantheonHandler<DeleteToolSetInput, DeleteToolSetResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<DeleteToolSetHandler> _logger;

    public DeleteToolSetHandler(ILogger<DeleteToolSetHandler> logger, HermesDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<DeleteToolSetResponse> Handle(
        DeleteToolSetInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete tool set command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var toolSet = await _dbContext
            .ToolSets.Include(s => s.Tools)
            .FirstOrDefaultAsync(s => s.Id == command.ToolSetId, cancellationToken);

        Guard.Against.NotFound(command.ToolSetId, toolSet);

        _dbContext.ToolSets.Remove(toolSet);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled delete tool set request.");
        return new DeleteToolSetResponse(command.ToolSetId);
    }
}
