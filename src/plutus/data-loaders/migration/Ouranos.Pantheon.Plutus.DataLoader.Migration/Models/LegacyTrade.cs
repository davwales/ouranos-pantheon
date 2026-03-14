using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.DataLoader.Migration.Models;

public sealed record LegacyTrade(
    Id<Trade> Id,
    LegacyTradeMetadata Metadata,
    decimal Price,
    decimal Volume,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);