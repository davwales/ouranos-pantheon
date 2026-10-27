using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.SyncLoras;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableLoras;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using TickerQ.Utilities.Base;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.SyncLoras;

public sealed class SyncLorasJobTests
{
    private static readonly ImageModelsDto ImageModels = new(
        "flux",
        [new ImageModelDto("flux", "flux-1"), new ImageModelDto("sdxl", "sdxl-1")]
    );

    private readonly ILogger<SyncLorasJob> _logger = Substitute.For<ILogger<SyncLorasJob>>();
    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly string _dbName = Guid.NewGuid().ToString();

    public SyncLorasJobTests()
    {
        _mlClient.GetImageModelsAsync(Arg.Any<CancellationToken>()).Returns(ImageModels);
    }

    private SyncLorasJob CreateJob(HermesImageGenerationOptions? imageOptions = null)
    {
        return new SyncLorasJob(
            _logger,
            _mlClient,
            DbContextExtensions.MockFactory<HermesDbContext>(_dbName),
            Options.Create(
                new HermesOptions
                {
                    ImageGeneration = imageOptions ?? new HermesImageGenerationOptions(),
                }
            )
        );
    }

    private Task ExecuteAsync(SyncLorasJob job)
    {
        return job.Execute(Substitute.For<TickerFunctionContext>(), CancellationToken.None);
    }

    private async Task<List<AvailableLora>> CatalogueAsync()
    {
        await using var db = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        return await db.AvailableLoras.OrderBy(l => l.Identifier).ToListAsync();
    }

    private static AvailableLora Row(string identifier, string? title = null)
    {
        return AvailableLora.Create(
            DatabaseExtensions.CreateId<AvailableLora>(),
            identifier,
            title,
            null,
            null,
            null,
            null,
            isCompatible: true
        );
    }

    private void SetupLoras(params ImageLoraDto[] loras)
    {
        _mlClient.GetImageLorasAsync(Arg.Any<CancellationToken>()).Returns([.. loras]);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new SyncLorasJob(
                null!,
                _mlClient,
                DbContextExtensions.MockFactory<HermesDbContext>(_dbName),
                Options.Create(new HermesOptions())
            )
        );
    }

    [Fact]
    public void Constructor_WhenMlClientIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new SyncLorasJob(
                _logger,
                null!,
                DbContextExtensions.MockFactory<HermesDbContext>(_dbName),
                Options.Create(new HermesOptions())
            )
        );
    }

    [Fact]
    public void Constructor_WhenDbContextFactoryIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new SyncLorasJob(_logger, _mlClient, null!, Options.Create(new HermesOptions()))
        );
    }

    [Fact]
    public void Constructor_WhenOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new SyncLorasJob(
                _logger,
                _mlClient,
                DbContextExtensions.MockFactory<HermesDbContext>(_dbName),
                null!
            )
        );
    }

    [Fact]
    public async Task Execute_WhenNewLorasAreListed_ShouldAddThemWithTheirDetails()
    {
        // Arrange
        SetupLoras(new ImageLoraDto("detail", "Detail", "flux-1", 0.7f, "detailed", "Use 0.7"));

        // Act
        await ExecuteAsync(CreateJob());

        // Assert
        var lora = (await CatalogueAsync()).ShouldHaveSingleItem();
        lora.Identifier.ShouldBe("detail");
        lora.Title.ShouldBe("Detail");
        lora.CompatClass.ShouldBe("flux-1");
        lora.DefaultWeight.ShouldBe(0.7f);
        lora.TriggerPhrase.ShouldBe("detailed");
        lora.UsageHint.ShouldBe("Use 0.7");
        lora.IsCompatible.ShouldBeTrue();
    }

    [Theory]
    [InlineData("sdxl-1", null, false)]
    [InlineData("sdxl-1", "sdxl", true)]
    [InlineData(null, null, true)]
    [InlineData("sdxl-1", "missing-model", true)]
    public async Task Execute_ShouldMarkLorasCompatibleUnlessTheirCompatClassDiffersFromTheImageModels(
        string? compatClass,
        string? configuredModel,
        bool isCompatible
    )
    {
        // Arrange
        SetupLoras(new ImageLoraDto("detail", null, compatClass, null, null, null));

        // Act
        await ExecuteAsync(CreateJob(new HermesImageGenerationOptions { Model = configuredModel }));

        // Assert
        (await CatalogueAsync())
            .ShouldHaveSingleItem()
            .IsCompatible.ShouldBe(isCompatible);
    }

    [Fact]
    public async Task Execute_WhenLorasChangeOrDisappear_ShouldUpdateAndRemoveTheirRows()
    {
        // Arrange
        await using (var seed = DbContextExtensions.Mock<HermesDbContext>(_dbName))
        {
            await seed.SeedData(Row("detail", "Old title"), Row("removed"));
        }

        SetupLoras(new ImageLoraDto("detail", "New title", null, null, null, null));

        // Act
        await ExecuteAsync(CreateJob());

        // Assert
        (await CatalogueAsync())
            .ShouldHaveSingleItem()
            .Title.ShouldBe("New title");
    }

    [Fact]
    public async Task Execute_WhenNoLorasAreListed_ShouldKeepTheCatalogue()
    {
        // Arrange
        await using (var seed = DbContextExtensions.Mock<HermesDbContext>(_dbName))
        {
            await seed.SeedData(Row("detail"));
        }

        SetupLoras();

        // Act
        await ExecuteAsync(CreateJob());

        // Assert
        (await CatalogueAsync())
            .ShouldHaveSingleItem()
            .Identifier.ShouldBe("detail");
        await _mlClient.DidNotReceive().GetImageModelsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WhenOuranosMlFails_ShouldThrowAndKeepTheCatalogue()
    {
        // Arrange
        await using (var seed = DbContextExtensions.Mock<HermesDbContext>(_dbName))
        {
            await seed.SeedData(Row("detail"));
        }

        _mlClient
            .GetImageLorasAsync(Arg.Any<CancellationToken>())
            .Returns<List<ImageLoraDto>>(_ => throw new HttpRequestException("down"));

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(() => ExecuteAsync(CreateJob()));
        (await CatalogueAsync()).ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Execute_WhenImageGenerationIsDisabled_ShouldNotCallOuranosMl()
    {
        // Act
        await ExecuteAsync(CreateJob(new HermesImageGenerationOptions { IsEnabled = false }));

        // Assert
        await _mlClient.DidNotReceive().GetImageLorasAsync(Arg.Any<CancellationToken>());
    }
}
