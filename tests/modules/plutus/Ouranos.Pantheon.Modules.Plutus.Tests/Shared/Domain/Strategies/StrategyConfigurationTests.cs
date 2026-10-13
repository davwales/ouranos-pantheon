using System.Text.Json;
using System.Text.Json.Serialization;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies;

public sealed class StrategyConfigurationTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public void Serialize_WhenSwing_ShouldEmitTheModeDiscriminatorOnce()
    {
        // Arrange
        StrategyConfiguration configuration = StrategyTestFactory.Swing();

        // Act
        var json = JsonSerializer.Serialize(configuration, Options);

        // Assert
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("mode").GetString().ShouldBe("Swing");

        document
            .RootElement.EnumerateObject()
            .Count(p => p.Name.Equals("mode", StringComparison.OrdinalIgnoreCase))
            .ShouldBe(1);

        document
            .RootElement.GetProperty("inputWeights")[0]
            .GetProperty("kind")
            .GetString()
            .ShouldBe("SignalTaxAdjustedRoi");
    }

    [Fact]
    public void Serialize_WhenFlip_ShouldEmitTheFlipDiscriminator()
    {
        // Arrange
        StrategyConfiguration configuration = new FlipConfiguration(MaxOpenOrders: 3);

        // Act
        var json = JsonSerializer.Serialize(configuration, Options);

        // Assert
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("mode").GetString().ShouldBe("Flip");
        document.RootElement.GetProperty("maxOpenOrders").GetInt32().ShouldBe(3);
    }

    [Fact]
    public void Deserialize_WhenSwingRoundTrips_ShouldKeepEveryField()
    {
        // Arrange
        StrategyConfiguration original = new SwingConfiguration(
            [
                new InputWeight(InputKind.SignalRsi, -0.5m),
                new InputWeight(InputKind.SignalTrendMomentum, 2m),
            ],
            BuyThreshold: 0.2m,
            SellThreshold: -0.1m,
            MaxPositions: 5,
            MaxPositionPercent: 0.1m,
            HoldPeriodDays: 7
        );

        // Act
        var result = JsonSerializer.Deserialize<StrategyConfiguration>(
            JsonSerializer.Serialize(original, Options),
            Options
        );

        // Assert
        var swing = result.ShouldBeOfType<SwingConfiguration>();
        var expected = (SwingConfiguration)original;
        swing.InputWeights.ShouldBe(expected.InputWeights);
        (swing with { InputWeights = expected.InputWeights }).ShouldBe(expected);
    }

    [Fact]
    public void Deserialize_WhenFlipRoundTrips_ShouldKeepEveryField()
    {
        // Arrange
        StrategyConfiguration original = new FlipConfiguration(
            CheckInHours: 4,
            MaxOpenOrders: 3,
            LookbackHours: 12,
            BidOffset: 0.1m,
            AskOffset: -0.05m,
            MinMargin: 0.02m,
            MaxHoldHours: 48,
            MaxPositionPercent: 0.3m,
            MaxPriceMove: 0.2m,
            MinPrice: 1000m
        );

        // Act
        var result = JsonSerializer.Deserialize<StrategyConfiguration>(
            JsonSerializer.Serialize(original, Options),
            Options
        );

        // Assert
        result.ShouldBeOfType<FlipConfiguration>().ShouldBe(original);
    }

    [Fact]
    public void Deserialize_WhenModeIsNotTheFirstProperty_ShouldStillSelectTheType()
    {
        // Arrange
        const string json = """{"maxOpenOrders":3,"checkInHours":4,"mode":"Flip"}""";

        // Act
        var result = JsonSerializer.Deserialize<StrategyConfiguration>(json, Options);

        // Assert
        var flip = result.ShouldBeOfType<FlipConfiguration>();
        flip.MaxOpenOrders.ShouldBe(3);
        flip.CheckInHours.ShouldBe(4);
        flip.LookbackHours.ShouldBe(new FlipConfiguration().LookbackHours);
    }

    [Fact]
    public void Deserialize_WhenSwingModeComesLast_ShouldReadTheWeights()
    {
        // Arrange
        const string json =
            """{"inputWeights":[{"kind":"SignalRsi","weight":1.5}],"buyThreshold":0.3,"mode":"Swing"}""";

        // Act
        var result = JsonSerializer.Deserialize<StrategyConfiguration>(json, Options);

        // Assert
        var swing = result.ShouldBeOfType<SwingConfiguration>();
        swing.InputWeights.ShouldBe([new InputWeight(InputKind.SignalRsi, 1.5m)]);
        swing.BuyThreshold.ShouldBe(0.3m);
        swing.SellThreshold.ShouldBeNull();
    }

    [Fact]
    public void Deserialize_WhenModeIsUnknown_ShouldThrowJsonException()
    {
        // Arrange
        const string json = """{"mode":"Arbitrage"}""";

        // Act & Assert
        Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<StrategyConfiguration>(json, Options)
        );
    }

    [Theory]
    [InlineData("maxPositions")]
    [InlineData("maxPositionPercent")]
    [InlineData("holdPeriodDays")]
    [InlineData("buyThreshold")]
    [InlineData("sellThreshold")]
    public void Validate_WhenSwingLocksAnUnsetField_ShouldThrowArgumentException(string key)
    {
        // Arrange
        var configuration = StrategyTestFactory.Swing() with
        {
            LockedParameters = [key],
        };

        // Act & Assert
        Should.Throw<ArgumentException>(configuration.Validate);
    }

    [Fact]
    public void Validate_WhenSwingLocksASetLimit_ShouldNotThrow()
    {
        // Arrange
        var configuration = StrategyTestFactory.Swing() with
        {
            MaxPositions = 8,
            LockedParameters = ["maxPositions"],
        };

        // Act & Assert
        Should.NotThrow(configuration.Validate);
    }

    [Fact]
    public void Deserialize_WhenLockedParametersAreMissingOrNull_ShouldDefaultToEmpty()
    {
        // Act
        var missing = JsonSerializer.Deserialize<StrategyConfiguration>(
            """{"mode":"Flip"}""",
            Options
        );
        var nulled = JsonSerializer.Deserialize<StrategyConfiguration>(
            """{"mode":"Flip","lockedParameters":null}""",
            Options
        );

        // Assert
        missing.ShouldNotBeNull().LockedParameters.ShouldBeEmpty();
        nulled.ShouldNotBeNull().LockedParameters.ShouldBeEmpty();
    }

    [Fact]
    public void Deserialize_WhenLockedParametersRoundTrip_ShouldKeepThem()
    {
        // Arrange
        StrategyConfiguration configuration = new FlipConfiguration() with
        {
            LockedParameters = ["bidOffset"],
        };

        // Act
        var json = JsonSerializer.Serialize(configuration, Options);
        var restored = JsonSerializer.Deserialize<StrategyConfiguration>(json, Options);

        // Assert
        restored.ShouldNotBeNull().LockedParameters.ShouldBe(["bidOffset"]);
    }
}
