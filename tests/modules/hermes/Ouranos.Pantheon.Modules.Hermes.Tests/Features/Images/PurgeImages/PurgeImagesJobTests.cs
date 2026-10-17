using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.PurgeImages;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Images.PurgeImages;

public sealed class PurgeImagesJobTests
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    private PurgeImagesJob CreateJob()
    {
        return new PurgeImagesJob(
            Substitute.For<ILogger<PurgeImagesJob>>(),
            DbContextExtensions.MockFactory<HermesDbContext>(_dbName),
            Options.Create(
                new HermesOptions
                {
                    ImageGeneration = new HermesImageGenerationOptions
                    {
                        UnsavedRetention = TimeSpan.FromHours(24),
                    },
                }
            )
        );
    }

    private static GeneratedImage Image(TimeSpan age)
    {
        return GeneratedImage
            .Create(
                new Id<GeneratedImage>(Guid.NewGuid().ToString()),
                [1],
                "image/png",
                64,
                64,
                "A fox",
                "flux"
            )
            .WithCreatedAt(DateTimeOffset.UtcNow - age);
    }

    [Fact]
    public async Task Expired_ShouldMatchOnlyOldImagesNoMessageReferences()
    {
        // Arrange
        var oldOrphan = Image(TimeSpan.FromHours(25));
        var recentOrphan = Image(TimeSpan.FromHours(1));
        var oldReferenced = Image(TimeSpan.FromDays(30));

        await using (var dbContext = DbContextExtensions.Mock<HermesDbContext>(_dbName))
        {
            await dbContext.SeedData(oldOrphan);
            await dbContext.SeedData(recentOrphan);
            await dbContext.SeedData(oldReferenced);
            await dbContext.SeedData(
                Message.Create(
                    new Id<Message>(Guid.NewGuid().ToString()),
                    new Id<Conversation>(Guid.NewGuid().ToString()),
                    "Here it is.",
                    Role.Assistant,
                    0,
                    [
                        ImageMessagePart.Create(
                            new Id<MessagePart>(Guid.NewGuid().ToString()),
                            oldReferenced.Id,
                            64,
                            64,
                            "A fox"
                        ),
                    ]
                )
            );
        }

        // Act
        await using var queryContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);

        var expired = PurgeImagesJob
            .Expired(queryContext, DateTimeOffset.UtcNow - TimeSpan.FromHours(24))
            .Select(i => i.Id)
            .ToList();

        // Assert
        expired.ShouldBe([oldOrphan.Id]);
    }

    [Fact]
    public async Task Execute_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () =>
            CreateJob()
                .Execute(Substitute.For<TickerQ.Utilities.Base.TickerFunctionContext>(), cts.Token);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }
}
