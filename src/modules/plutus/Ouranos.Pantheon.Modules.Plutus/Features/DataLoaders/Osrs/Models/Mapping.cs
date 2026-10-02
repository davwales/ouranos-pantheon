namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs.Models;

public sealed record Mapping(
    int Id,
    string Name,
    string Icon,
    string Examine,
    object Members,
    long? LowAlch,
    long? HighAlch,
    int? Limit,
    long Value
)
{
    public bool IsMembers =>
        Members switch
        {
            bool isMembers => isMembers,
            string membersStr => !bool.TryParse(membersStr, out var isMembers) || isMembers,
            _ => true,
        };
}
