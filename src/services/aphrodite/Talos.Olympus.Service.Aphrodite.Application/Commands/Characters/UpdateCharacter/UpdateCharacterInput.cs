using MediatR;
using Talos.Olympus.Core.Application.Common;
using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Aphrodite.Domain.Characters;

namespace Talos.Olympus.Service.Aphrodite.Application.Commands.Characters.UpdateCharacter;

public sealed record UpdateCharacterInput(
    Id<Character> CharacterId,
    string Name,
    int Age,
    List<CharacterDetail> Details
) : IRequest<IdResponse<Character>>;