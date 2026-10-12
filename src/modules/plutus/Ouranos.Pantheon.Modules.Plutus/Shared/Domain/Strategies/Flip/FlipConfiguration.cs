using System.Text.Json.Serialization;
using Ardalis.GuardClauses;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

/// <param name="CheckInHours">How often the trader collects fills and re-places orders.</param>
/// <param name="MaxOpenOrders">Most buy and sell orders open at once.</param>
/// <param name="LookbackHours">History the bid and ask are quoted from.</param>
/// <param name="BidOffset">
///     Bid position within the recent spread: 0 bids the recent sell-side price, positive values
///     bid higher (fill sooner), negative values lower.
/// </param>
/// <param name="AskOffset">
///     Ask position within the recent spread: 0 asks the recent buy-side price, positive values
///     ask lower (sell sooner), negative values higher.
/// </param>
/// <param name="MinMargin">Smallest margin after tax, as a fraction of the bid, worth an order.</param>
/// <param name="MaxHoldHours">Inventory held longer than this is re-listed at the sell-side price.</param>
/// <param name="MaxPositionPercent">Largest share of equity committed to one symbol.</param>
/// <param name="MaxPriceMove">
///     Symbols whose recent sell-side price is further than this from its 7-day level are skipped;
///     0 disables the check.
/// </param>
/// <param name="MinPrice">Symbols cheaper than this are never flipped.</param>
public sealed record FlipConfiguration(
    int CheckInHours = FlipConfiguration.DefaultCheckInHours,
    int MaxOpenOrders = FlipConfiguration.DefaultMaxOpenOrders,
    int LookbackHours = 24,
    decimal BidOffset = 0m,
    decimal AskOffset = 0m,
    decimal MinMargin = 0.03m,
    int MaxHoldHours = 72,
    decimal MaxPositionPercent = 0.125m,
    decimal MaxPriceMove = 0.1m,
    decimal MinPrice = 0m
) : StrategyConfiguration
{
    public const int DefaultCheckInHours = 8;

    public const int DefaultMaxOpenOrders = 8;

    public FlipConfiguration()
        : this(DefaultCheckInHours) { }

    [JsonIgnore]
    public override StrategyMode Mode => StrategyMode.Flip;

    public override void Validate()
    {
        Guard.Against.NegativeOrZero(CheckInHours);
        Guard.Against.NegativeOrZero(MaxOpenOrders);
        Guard.Against.NegativeOrZero(LookbackHours);
        Guard.Against.NegativeOrZero(MaxHoldHours);
        Guard.Against.OutOfRange(MaxPositionPercent, nameof(MaxPositionPercent), 0.0001m, 1m);
        Guard.Against.Negative(MaxPriceMove);
        Guard.Against.Negative(MinPrice);
    }
}
