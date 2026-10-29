using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

public static class RoleExtensions
{
    // A summary stands in for the assistant turns it replaced.
    public static RoleDto ToRoleDto(this Role role)
    {
        return role switch
        {
            Role.System => RoleDto.System,
            Role.User => RoleDto.User,
            Role.Assistant => RoleDto.Assistant,
            Role.Summary => RoleDto.Assistant,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null),
        };
    }
}
