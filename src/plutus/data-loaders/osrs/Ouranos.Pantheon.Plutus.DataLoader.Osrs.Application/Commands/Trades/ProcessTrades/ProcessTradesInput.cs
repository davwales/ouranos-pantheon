using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.DataLoader.Osrs.Application.Queries.Trades.GetTrades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Osrs.Application.Commands.Trades.ProcessTrades;

public sealed record ProcessTradesInput(List<GetTradesResponse> Trades) : ICommand;