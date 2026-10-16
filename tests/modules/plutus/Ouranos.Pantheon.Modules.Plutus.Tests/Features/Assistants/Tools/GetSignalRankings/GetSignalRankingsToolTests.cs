using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSignalRankings;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSignalRankings.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskMarketAnalyst.MarketAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.Tools.GetSignalRankings;

public sealed class GetSignalRankingsToolTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly GetSignalRankingsTool _tool;

    public GetSignalRankingsToolTests()
    {
        _bus.InvokeAsync<PagedResponse<GetSignalRankingsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new PagedResponse<GetSignalRankingsResponse>(
                    [
                        new GetSignalRankingsResponse(
                            Factory.TwistedBowId,
                            "Twisted bow",
                            null,
                            DailyAveragePrice: 1450000000,
                            DailyVolume: 2,
                            OverallScore: 0.5m,
                            BuyScore: 0.4m,
                            SellScore: -0.1m,
                            FlipScore: 0.7m,
                            MerchScore: null,
                            SignalCount: 7,
                            BullishCount: 5,
                            BearishCount: 1
                        ),
                    ],
                    1,
                    0,
                    1
                )
            );

        _tool = new GetSignalRankingsTool(
            Substitute.For<ILogger<GetSignalRankingsTool>>(),
            _bus,
            Options.Create(new PlutusOptions())
        );
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool.InvokeAsync(Factory.Context(), arguments, CancellationToken.None).AsTask();
    }

    [Theory]
    [InlineData("""{"intent":"Flip","take":3}""", "FlipScore", 3, 0.7)]
    [InlineData("""{"intent":null,"take":null}""", "OverallScore", 10, 0.5)]
    public async Task InvokeAsync_ShouldRankByTheIntentsScore(
        string arguments,
        string sortField,
        int take,
        double score
    )
    {
        // Act
        var result = await InvokeAsync(arguments);

        // Assert
        result.ShouldContainData(
            new SignalRankingsData([
                new SignalRankingData(
                    Factory.TwistedBowId,
                    "Twisted bow",
                    (decimal)score,
                    5,
                    1,
                    1450000000,
                    2
                ),
            ])
        );
        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetSignalRankingsResponse>>(
                Arg.Is<GetSignalRankingsInput>(i =>
                    i.SortField == sortField && i.SortDirection == "desc" && i.Take == take
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenTheIntentHasNoScore_ShouldOmitTheScore()
    {
        // Act
        var result = await InvokeAsync("""{"intent":"Merch","take":null}""");

        // Assert
        result.ShouldContainData<SignalRankingsData>().Symbols[0].Score.ShouldBeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenThereAreNoRankings_ShouldSaySo()
    {
        // Arrange
        _bus.InvokeAsync<PagedResponse<GetSignalRankingsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new PagedResponse<GetSignalRankingsResponse>([], 0, 0, 0));

        // Act
        var result = await InvokeAsync("""{"intent":null,"take":null}""");

        // Assert
        result.ShouldContainNote().ShouldBe(GetSignalRankingsTool.NoSignals);
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetSignalRankingsTool(
                Substitute.For<ILogger<GetSignalRankingsTool>>(),
                null!,
                Options.Create(new PlutusOptions())
            )
        );
    }

    [Fact]
    public void Constructor_WhenOptionsAreNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetSignalRankingsTool(Substitute.For<ILogger<GetSignalRankingsTool>>(), _bus, null!)
        );
    }
}
