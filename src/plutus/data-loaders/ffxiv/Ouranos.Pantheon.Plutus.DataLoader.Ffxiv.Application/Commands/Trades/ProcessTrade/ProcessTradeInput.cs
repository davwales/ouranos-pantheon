using Ouranos.Pantheon.Core.Application.Mediator;

namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Application.Commands.Trades.ProcessTrade;

public sealed record ProcessTradeInput(
    string ItemCode,
    IReadOnlyCollection<ProcessTradeSaleInput> Sales
) : ICommand;