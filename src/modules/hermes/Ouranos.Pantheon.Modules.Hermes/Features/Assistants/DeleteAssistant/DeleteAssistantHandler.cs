using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.DeleteAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.DeleteAssistant;

public sealed class DeleteAssistantHandler : CommandHandler<DeleteAssistantInput, IdResponse<Assistant>>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<DeleteAssistantHandler> _logger;

    public DeleteAssistantHandler(
        ILogger<DeleteAssistantHandler> logger,
        HermesDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<IdResponse<Assistant>> Handle(
        DeleteAssistantInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete assistant command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var assistant = await _dbContext.Assistants
            .FirstOrDefaultAsync(a => a.Id == command.AssistantId, cancellationToken);

        Guard.Against.NotFound(command.AssistantId, assistant);

        _dbContext.Assistants.Remove(assistant);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var response = new IdResponse<Assistant>(command.AssistantId);

        _logger.LogDebug("Successfully handled delete assistant request.");
        return response;
    }
}
