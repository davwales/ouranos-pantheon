namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;

public sealed record AuthMessage(string Key, string Secret, string Action = "auth");
