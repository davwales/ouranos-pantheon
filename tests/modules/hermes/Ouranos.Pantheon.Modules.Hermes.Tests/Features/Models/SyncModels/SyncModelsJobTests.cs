using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.SyncModels;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableModels;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Models.SyncModels;

public sealed class SyncModelsJobTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogger<SyncModelsJob> _logger = Substitute.For<ILogger<SyncModelsJob>>();
    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly string _dbName;

    public SyncModelsJobTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbName = Guid.NewGuid().ToString();
    }

    [Fact]
    public async Task Execute_WhenNewModelsAvailable_ShouldCreateAvailableModelRows()
    {
        // Arrange
        await using var db = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        var dbFactory = DbContextExtensions.MockFactory<HermesDbContext>(_dbName);

        var remoteModels = new List<AvailableModelDto>
        {
            new("llama3.2", "meta"),
            new("gpt-4o", "openai"),
        };

        _mlClient.GetAvailableModelsAsync(Arg.Any<CancellationToken>()).Returns(remoteModels);

        var job = new SyncModelsJob(_logger, _mlClient, dbFactory);

        // Act
        await job.Execute(
            Substitute.For<TickerQ.Utilities.Base.TickerFunctionContext>(),
            CancellationToken.None
        );

        // Assert
        var availableModels = await db.AvailableModels.ToListAsync();
        availableModels.Count.ShouldBe(2);
        availableModels.ShouldContain(m => m.ModelIdentifier == "llama3.2" && m.OwnedBy == "meta");
        availableModels.ShouldContain(m => m.ModelIdentifier == "gpt-4o" && m.OwnedBy == "openai");
    }

    [Fact]
    public async Task Execute_WhenModelRemovedFromRemote_ShouldMarkModelConfigUnavailable()
    {
        // Arrange
        await using var seedDb = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        var dbFactory = DbContextExtensions.MockFactory<HermesDbContext>(_dbName);

        var modelConfig = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            "Old Model",
            "removed-model",
            "system prompt"
        );
        await seedDb.SeedData(modelConfig);

        var remoteModels = new List<AvailableModelDto> { new("other-model", "some-org") };

        _mlClient.GetAvailableModelsAsync(Arg.Any<CancellationToken>()).Returns(remoteModels);

        var job = new SyncModelsJob(_logger, _mlClient, dbFactory);

        // Act
        await job.Execute(
            Substitute.For<TickerQ.Utilities.Base.TickerFunctionContext>(),
            CancellationToken.None
        );

        // Assert
        await using var verifyDb = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        var updated = await verifyDb.ModelConfigs.FirstAsync();
        updated.IsUnavailable.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_WhenPreviouslyUnavailableModelBecomesAvailable_ShouldMarkAvailable()
    {
        // Arrange
        await using var seedDb = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        var dbFactory = DbContextExtensions.MockFactory<HermesDbContext>(_dbName);

        var modelConfig = ModelConfig.Create(
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            "Restored Model",
            "restored-model",
            "system prompt"
        );
        modelConfig.MarkUnavailable();
        await seedDb.SeedData(modelConfig);

        var remoteModels = new List<AvailableModelDto> { new("restored-model", "some-org") };

        _mlClient.GetAvailableModelsAsync(Arg.Any<CancellationToken>()).Returns(remoteModels);

        var job = new SyncModelsJob(_logger, _mlClient, dbFactory);

        // Act
        await job.Execute(
            Substitute.For<TickerQ.Utilities.Base.TickerFunctionContext>(),
            CancellationToken.None
        );

        // Assert
        await using var verifyDb = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        var updated = await verifyDb.ModelConfigs.FirstAsync();
        updated.IsUnavailable.ShouldBeFalse();
    }

    [Fact]
    public async Task Execute_WhenStaleAvailableModelNotInRemote_ShouldRemoveFromAvailableModels()
    {
        // Arrange
        await using var seedDb = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        var dbFactory = DbContextExtensions.MockFactory<HermesDbContext>(_dbName);

        var staleAvailableModel = AvailableModel.Create(
            new Id<AvailableModel>(Guid.NewGuid().ToString()),
            "stale-model",
            "stale-org"
        );
        await seedDb.SeedData(staleAvailableModel);

        _mlClient.GetAvailableModelsAsync(Arg.Any<CancellationToken>()).Returns([]);

        var job = new SyncModelsJob(_logger, _mlClient, dbFactory);

        // Act
        await job.Execute(
            Substitute.For<TickerQ.Utilities.Base.TickerFunctionContext>(),
            CancellationToken.None
        );

        // Assert
        await using var verifyDb = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        var availableModels = await verifyDb.AvailableModels.ToListAsync();
        availableModels.ShouldBeEmpty();
    }

    [Fact]
    public async Task Execute_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var dbFactory = DbContextExtensions.MockFactory<HermesDbContext>();
        var job = new SyncModelsJob(_logger, _mlClient, dbFactory);
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () =>
            await job.Execute(
                Substitute.For<TickerQ.Utilities.Base.TickerFunctionContext>(),
                cancellationToken
            );

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
