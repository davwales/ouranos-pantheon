using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Application;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel;

public sealed class DeleteModelHandler : IPantheonHandler<DeleteModelInput, DeleteModelResponse>
{
    private readonly HermesDbContext _dbContext;
    private readonly ILogger<DeleteModelHandler> _logger;

    public DeleteModelHandler(ILogger<DeleteModelHandler> logger, HermesDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<DeleteModelResponse> Handle(
        DeleteModelInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete model command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var model = await _dbContext.ModelConfigs.FirstOrDefaultAsync(
            m => m.Id == command.ModelId,
            cancellationToken
        );

        Guard.Against.NotFound(command.ModelId, model);

        _dbContext.ModelConfigs.Remove(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully handled delete model request.");
        return new DeleteModelResponse(command.ModelId);
    }
}
