using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableModels;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using TickerQ.Utilities.Base;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.SyncModels;

public sealed class SyncModelsJob
{
    private readonly ILogger<SyncModelsJob> _logger;
    private readonly IOuranosMachineLearningClient _mlClient;
    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory;

    public SyncModelsJob(
        ILogger<SyncModelsJob> logger,
        IOuranosMachineLearningClient mlClient,
        IDbContextFactory<HermesDbContext> dbContextFactory
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(mlClient);
        Guard.Against.Null(dbContextFactory);

        _logger = logger;
        _mlClient = mlClient;
        _dbContextFactory = dbContextFactory;
    }

    [TickerFunction(nameof(SyncModelsJob), "0 0 * * * *")]
    public async Task Execute(TickerFunctionContext _, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Starting model sync from Ouranos ML.");
        cancellationToken.ThrowIfCancellationRequested();

        var availableModels = await _mlClient.GetAvailableModelsAsync(cancellationToken);
        var remoteIdentifiers = availableModels.Select(m => m.ModelIdentifier).ToHashSet();

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        await SyncAvailableModelRows(db, availableModels, cancellationToken);
        await UpdateModelConfigAvailability(db, remoteIdentifiers, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Model sync completed successfully.");
    }

    private static async Task SyncAvailableModelRows(
        HermesDbContext db,
        IReadOnlyList<AvailableModelDto> availableModels,
        CancellationToken cancellationToken
    )
    {
        var existingModels = await db.AvailableModels.ToDictionaryAsync(
            m => m.ModelIdentifier,
            m => m,
            cancellationToken
        );

        foreach (var dto in availableModels)
        {
            if (existingModels.TryGetValue(dto.ModelIdentifier, out var existing))
            {
                existing.Update(dto.ModelIdentifier, dto.OwnedBy);
                continue;
            }

            var newModel = AvailableModel.Create(
                DatabaseExtensions.CreateId<AvailableModel>(),
                dto.ModelIdentifier,
                dto.OwnedBy
            );

            db.AvailableModels.Add(newModel);
        }

        var remoteIdentifiers = availableModels.Select(m => m.ModelIdentifier).ToHashSet();
        var staleModels = existingModels
            .Values.Where(m => !remoteIdentifiers.Contains(m.ModelIdentifier))
            .ToList();

        foreach (var stale in staleModels)
        {
            db.AvailableModels.Remove(stale);
        }
    }

    private static async Task UpdateModelConfigAvailability(
        HermesDbContext db,
        HashSet<string> remoteIdentifiers,
        CancellationToken cancellationToken
    )
    {
        var modelConfigs = await db.ModelConfigs.ToListAsync(cancellationToken);

        foreach (var config in modelConfigs)
        {
            var wasUnavailable = config.IsUnavailable;
            var isNowUnavailable = !remoteIdentifiers.Contains(config.ModelIdentifier);

            if (wasUnavailable && !isNowUnavailable)
            {
                config.MarkAvailable();
            }
            else if (!wasUnavailable && isNowUnavailable)
            {
                config.MarkUnavailable();
            }
        }
    }
}
