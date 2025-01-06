using MediatR;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Aphrodite.Domain.Characters;

namespace Ouranos.Pantheon.Service.Aphrodite.Application.Commands.Characters.UpdateCharacter;

public sealed record UpdateCharacterInput(
    Id<Character> CharacterId,
    string Name,
    int Age,
    List<CharacterDetail> Details
) : IRequest<IdResponse<Character>>;