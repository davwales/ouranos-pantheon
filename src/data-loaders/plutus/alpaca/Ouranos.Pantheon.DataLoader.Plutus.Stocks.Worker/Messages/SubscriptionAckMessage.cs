namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Worker.Messages;

public sealed record SubscriptionAckMessage(
    IReadOnlyCollection<string> Trades,
    IReadOnlyCollection<string> Quotes,
    IReadOnlyCollection<string> Bars,
    IReadOnlyCollection<string> UpdatedBars,
    IReadOnlyCollection<string> DailyBars,
    IReadOnlyCollection<string> Statuses,
    IReadOnlyCollection<string> Lulds,
    IReadOnlyCollection<string> Corrections,
    IReadOnlyCollection<string> CancelErrors
)
{
    public const string TypeIndicator = "subscription";
}