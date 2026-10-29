using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Ffxiv.XivApi;

public sealed class GetItemsTests
{
    private readonly ILogger<GetItems> _logger = Substitute.For<ILogger<GetItems>>();
    private readonly IGithubClient _githubClient = Substitute.For<IGithubClient>();

    private GetItems CreateGetItems(IMemoryCache cache, int itemCacheMinutes = 60)
    {
        var options = new FfxivDataLoaderOptions() with
        {
            XivApi = new XivApiOptions("https://test.api", itemCacheMinutes),
        };

        return new GetItems(cache, _logger, _githubClient, Options.Create(options));
    }

    private void SetupServerItems(params ItemResponse[] items)
    {
        _githubClient
            .GetItems(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(items.ToList()));
    }

    [Theory]
    [InlineData("1", false, "Sword")]
    [InlineData("1", true, "Sword")]
    [InlineData("2", false, "Iron Ore")]
    public async Task GetItemAsync_WhenItemVariantExists_ShouldReturnIt(
        string symbolCode,
        bool isHighQuality,
        string expectedName
    )
    {
        // Arrange
        SetupServerItems(
            new ItemResponse
            {
                Key = 1,
                Name = "Sword",
                CanBeHq = true,
            },
            new ItemResponse
            {
                Key = 2,
                Name = "Iron Ore",
                CanBeHq = false,
            }
        );
        var getItems = CreateGetItems(new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = await getItems.GetItemAsync(symbolCode, isHighQuality);

        // Assert
        result.ShouldNotBeNull();
        result.SymbolCode.ShouldBe(symbolCode);
        result.IsHighQuality.ShouldBe(isHighQuality);
        result.SymbolName.ShouldBe(expectedName);
    }

    [Theory]
    [InlineData("2", true)]
    [InlineData("3", false)]
    public async Task GetItemAsync_WhenItemVariantDoesNotExist_ShouldReturnNull(
        string symbolCode,
        bool isHighQuality
    )
    {
        // Arrange
        SetupServerItems(
            new ItemResponse
            {
                Key = 2,
                Name = "Iron Ore",
                CanBeHq = false,
            }
        );
        var getItems = CreateGetItems(new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = await getItems.GetItemAsync(symbolCode, isHighQuality);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetItemAsync_WhenKeyIsDuplicated_ShouldReturnFirstItem()
    {
        // Arrange
        SetupServerItems(
            new ItemResponse
            {
                Key = 1,
                Name = "First",
                CanBeHq = false,
            },
            new ItemResponse
            {
                Key = 1,
                Name = "Second",
                CanBeHq = false,
            }
        );
        var getItems = CreateGetItems(new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = await getItems.GetItemAsync("1", false);

        // Assert
        result.ShouldNotBeNull();
        result.SymbolName.ShouldBe("First");
    }

    [Fact]
    public async Task GetItemAsync_WhenCalledTwice_ShouldUseCacheOnSecondCall()
    {
        // Arrange
        SetupServerItems(
            new ItemResponse
            {
                Key = 1,
                Name = "Potion",
                CanBeHq = false,
            }
        );
        var getItems = CreateGetItems(new MemoryCache(new MemoryCacheOptions()));

        // Act
        await getItems.GetItemAsync("1", false);
        await getItems.GetItemAsync("1", true);

        // Assert
        await _githubClient.Received(1).GetItems(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetItemAsync_WhenCacheIsEmpty_ShouldExpireAfterConfiguredItemCacheMinutes()
    {
        // Arrange
        SetupServerItems();
        var cache = Substitute.For<IMemoryCache>();
        var entry = Substitute.For<ICacheEntry>();
        cache.CreateEntry(Arg.Any<object>()).Returns(entry);
        var getItems = CreateGetItems(cache, itemCacheMinutes: 7);

        // Act
        await getItems.GetItemAsync("1", false);

        // Assert
        entry.AbsoluteExpirationRelativeToNow.ShouldBe(TimeSpan.FromMinutes(7));
    }
}
