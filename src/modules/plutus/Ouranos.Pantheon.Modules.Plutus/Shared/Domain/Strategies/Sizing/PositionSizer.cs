namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Sizing;

/// <summary>
///     Sizes a position the same way for backtests and live recommendations: the budget share,
///     the liquidity cap and the per-symbol buy limit each bound the volume. Positions whose
///     capped value falls below <c>minFillFraction</c> of the intended budget are rejected so a
///     thin item never occupies a portfolio slot it cannot meaningfully fill.
/// </summary>
public static class PositionSizer
{
    public static PositionSize Size(
        decimal price,
        decimal positionBudget,
        decimal fillCap,
        decimal? buyLimit,
        decimal limitWindowsPerDay,
        decimal minFillFraction
    )
    {
        if (price <= 0 || positionBudget <= 0)
        {
            return new PositionSize(0m, PositionLimit.Budget);
        }

        var volume = Math.Floor(positionBudget / price);
        var limitedBy = PositionLimit.Budget;

        if (fillCap < volume)
        {
            volume = fillCap;
            limitedBy = PositionLimit.Liquidity;
        }

        if (buyLimit.HasValue)
        {
            var dailyLimit = Math.Floor(buyLimit.Value * limitWindowsPerDay);
            if (dailyLimit < volume)
            {
                volume = dailyLimit;
                limitedBy = PositionLimit.BuyLimit;
            }
        }

        if (volume < 1 || volume * price < positionBudget * minFillFraction)
        {
            return new PositionSize(0m, limitedBy);
        }

        return new PositionSize(volume, limitedBy);
    }
}
