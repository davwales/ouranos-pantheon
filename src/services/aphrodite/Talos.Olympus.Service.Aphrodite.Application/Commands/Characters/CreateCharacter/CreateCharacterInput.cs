using MediatR;
using Talos.Olympus.Core.Application.Common;
using Talos.Olympus.Service.Aphrodite.Domain.Characters;

namespace Talos.Olympus.Service.Aphrodite.Application.Commands.Characters.CreateCharacter;

public sealed record CreateCharacterInput(
    string Name,
    int Age,
    List<CharacterDetail> Details
) : IRequest<IdResponse<Character>>;