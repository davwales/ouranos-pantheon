using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;

/// <summary>How a market charges for selling; every simulator and recommender sells through this.</summary>
public sealed record MarketRules(FlatTax? Tax)
{
    public static readonly MarketRules TaxFree = new(Tax: null);

    public decimal TaxRate => Tax?.Rate ?? 0m;

    public static MarketRules For(Market market)
    {
        Guard.Against.Null(market);

        return new MarketRules(market.Taxes.Flat);
    }

    /// <summary>
    ///     Sales tax on one unit: the rate applied to the price, rounded down, waived when it would
    ///     be below the minimum (e.g. prices under 50 at a 2% rate) and capped at the maximum.
    /// </summary>
    public decimal UnitTax(decimal price)
    {
        if (Tax is null)
        {
            return 0m;
        }

        var amount = Math.Floor(price * Tax.Rate);
        return amount < Tax.Minimum ? 0m : Math.Min(amount, Tax.Maximum);
    }

    public decimal NetSale(decimal price, decimal quantity)
    {
        return quantity * (price - UnitTax(price));
    }

    /// <summary>Profit per unit after tax, as a fraction of the bid.</summary>
    public decimal NetMargin(decimal bid, decimal ask)
    {
        return bid > 0 ? (ask - UnitTax(ask) - bid) / bid : 0m;
    }

    /// <summary>
    ///     Lowest whole-unit ask that recovers the unit cost after tax. Proceeds after tax never
    ///     fall as the ask rises, so this is a binary search between the cost and an ask known to
    ///     work.
    /// </summary>
    public decimal BreakEvenAsk(decimal unitCost)
    {
        var low = Math.Ceiling(unitCost);

        var high = Math.Min(
            Math.Ceiling(unitCost / (1 - TaxRate)),
            Math.Ceiling(unitCost + (Tax?.Maximum ?? 0m))
        );

        while (low < high)
        {
            var mid = Math.Floor((low + high) / 2);
            if (mid - UnitTax(mid) >= unitCost)
            {
                high = mid;
            }
            else
            {
                low = mid + 1;
            }
        }

        return low;
    }
}
