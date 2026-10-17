using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora.Schemas;

public sealed record CreateLoraResponse(Id<Lora> LoraId);
