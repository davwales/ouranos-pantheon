using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableLoras;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using TickerQ.Utilities.Base;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.SyncLoras;

/// <summary>
/// Mirrors the image server's LoRAs into the available-LoRA catalogue. LoRA configs aren't
/// touched: one whose LoRA leaves the catalogue reads as unavailable until it returns.
/// </summary>
public sealed class SyncLorasJob(
    ILogger<SyncLorasJob> logger,
    IOuranosMachineLearningClient mlClient,
    IDbContextFactory<HermesDbContext> dbContextFactory,
    IOptions<HermesOptions> options
)
{
    private readonly ILogger<SyncLorasJob> _logger = Guard.Against.Null(logger);
    private readonly IOuranosMachineLearningClient _mlClient = Guard.Against.Null(mlClient);

    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory = Guard.Against.Null(
        dbContextFactory
    );

    private readonly HermesImageGenerationOptions _options = Guard
        .Against.Null(options)
        .Value.ImageGeneration;

    [TickerFunction(nameof(SyncLorasJob), "0 0 * * * *")]
    public async Task Execute(TickerFunctionContext _, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Starting LoRA sync from Ouranos ML.");
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.IsEnabled)
        {
            _logger.LogDebug("Skipped LoRA sync: image generation is disabled.");
            return;
        }

        var loras = await _mlClient.GetImageLorasAsync(cancellationToken);

        // An empty list is far likelier to be a misconfigured or restarting image server than
        // every LoRA being removed, so the catalogue is kept rather than wiped.
        if (loras.Count == 0)
        {
            _logger.LogWarning("Skipped LoRA sync: Ouranos ML listed no LoRAs.");
            return;
        }

        var imageModels = await _mlClient.GetImageModelsAsync(cancellationToken);
        var modelCompatClass = ModelCompatClass(imageModels);

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.AvailableLoras.ToDictionaryAsync(
            l => l.Identifier,
            cancellationToken
        );

        foreach (var lora in loras)
        {
            var isCompatible =
                lora.CompatClass is null
                || modelCompatClass is null
                || lora.CompatClass == modelCompatClass;

            if (existing.Remove(lora.Id, out var row))
            {
                row.Update(
                    lora.Title,
                    lora.CompatClass,
                    lora.DefaultWeight,
                    lora.TriggerPhrase,
                    lora.UsageHint,
                    isCompatible
                );
                continue;
            }

            db.AvailableLoras.Add(
                AvailableLora.Create(
                    DatabaseExtensions.CreateId<AvailableLora>(),
                    lora.Id,
                    lora.Title,
                    lora.CompatClass,
                    lora.DefaultWeight,
                    lora.TriggerPhrase,
                    lora.UsageHint,
                    isCompatible
                )
            );
        }

        db.AvailableLoras.RemoveRange(existing.Values);
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "LoRA sync completed with {Count} LoRAs for compat class '{compatClass}'.",
            loras.Count,
            modelCompatClass
        );
    }

    private string? ModelCompatClass(ImageModelsDto imageModels)
    {
        var modelId = _options.Model ?? imageModels.DefaultModel;
        var model = imageModels.Models.FirstOrDefault(m => m.Id == modelId);
        if (model is null)
        {
            _logger.LogWarning(
                "Image model '{model}' is not on Ouranos ML; LoRA compatibility is unknown.",
                modelId
            );
        }

        return model?.CompatClass;
    }
}
