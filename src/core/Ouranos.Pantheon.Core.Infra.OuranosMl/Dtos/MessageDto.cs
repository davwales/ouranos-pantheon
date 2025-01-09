namespace Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;

public sealed record MessageDto(
    string Content,
    RoleDto Role
);