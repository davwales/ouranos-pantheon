namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;

public sealed record MessageDto(
    string Content,
    RoleDto Role
);