using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Ffxiv.XivApi;

public sealed class GetItemsTests
{
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly ILogger<GetItems> _logger = Substitute.For<ILogger<GetItems>>();
    private readonly IGithubClient _githubClient = Substitute.For<IGithubClient>();
    private readonly GetItems _getItems;

    public GetItemsTests()
    {
        _getItems = new GetItems(
            _cache,
            _logger,
            _githubClient,
            Options.Create(new XivApiOptions("https://test.api", ItemCacheMinutes: 60))
        );
    }

    [Fact]
    public async Task GetItemsAsync_WhenItemsExist_ShouldReturnBothHqAndNqVariants()
    {
        // Arrange
        var serverItems = new List<ItemResponse>
        {
            new() { Key = 1, Name = "Sword", CanBeHq = true },
            new() { Key = 2, Name = "Iron Ore", CanBeHq = false },
        };

        _githubClient.GetItems(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(serverItems));

        // Act
        var result = await _getItems.GetItemsAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        // Sword (NQ + HQ) + Iron Ore (NQ only) = 3
        result.Count.ShouldBe(3);
        result.ShouldContain(i => i.SymbolCode == "1" && !i.IsHighQuality);
        result.ShouldContain(i => i.SymbolCode == "1" && i.IsHighQuality);
        result.ShouldContain(i => i.SymbolCode == "2" && !i.IsHighQuality);
    }

    [Fact]
    public async Task GetItemsAsync_WhenCalledTwice_ShouldUseCacheOnSecondCall()
    {
        // Arrange
        var serverItems = new List<ItemResponse>
        {
            new() { Key = 1, Name = "Potion", CanBeHq = false },
        };

        _githubClient.GetItems(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(serverItems));

        // Act
        await _getItems.GetItemsAsync(CancellationToken.None);
        await _getItems.GetItemsAsync(CancellationToken.None);

        // Assert
        await _githubClient.Received(1).GetItems(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetItemsAsync_WhenGithubReturnsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        _githubClient.GetItems(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<ItemResponse>()));

        // Act
        var result = await _getItems.GetItemsAsync(CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }
}
