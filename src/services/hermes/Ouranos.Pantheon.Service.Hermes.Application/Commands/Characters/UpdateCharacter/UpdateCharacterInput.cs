using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Characters.UpdateCharacter;

public sealed record UpdateCharacterInput(
    Id<Character> CharacterId,
    string Name,
    int Age,
    List<CharacterDetail> Details
) : ICommand<IdResponse<Character>>;