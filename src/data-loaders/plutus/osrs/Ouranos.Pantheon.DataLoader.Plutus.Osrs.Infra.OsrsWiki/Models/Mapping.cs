namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.OsrsWiki.Models;

public sealed record Mapping(
    int Id,
    string Name,
    string Icon,
    string Examine,
    bool Members,
    int? LowAlch,
    int? HighAlch,
    int? Limit,
    int Value
);