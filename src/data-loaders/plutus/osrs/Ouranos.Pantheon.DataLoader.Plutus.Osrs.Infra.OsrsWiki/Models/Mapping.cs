namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.OsrsWiki.Models;

public sealed record Mapping(
    int Id,
    string Name,
    string Icon,
    string Examine,
    object Members,
    int? LowAlch,
    int? HighAlch,
    int? Limit,
    int Value
)
{
    public bool IsMembers => Members switch
    {
        bool isMembers => isMembers,
        string membersStr => !bool.TryParse(membersStr, out var isMembers) || isMembers,
        _ => true
    };
}