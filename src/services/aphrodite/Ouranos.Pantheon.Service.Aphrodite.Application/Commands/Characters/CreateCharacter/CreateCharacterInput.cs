using MediatR;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Service.Aphrodite.Domain.Characters;

namespace Ouranos.Pantheon.Service.Aphrodite.Application.Commands.Characters.CreateCharacter;

public sealed record CreateCharacterInput(
    string Name,
    int Age,
    List<CharacterDetail> Details
) : IRequest<IdResponse<Character>>;