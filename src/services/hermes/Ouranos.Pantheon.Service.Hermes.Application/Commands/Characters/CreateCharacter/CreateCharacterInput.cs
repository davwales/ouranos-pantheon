using MediatR;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Characters.CreateCharacter;

public sealed record CreateCharacterInput(
    string Name,
    int Age,
    List<CharacterDetail> Details
) : IRequest<IdResponse<Character>>;