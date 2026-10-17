using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.DeleteLora.Schemas;

public sealed record DeleteLoraResponse(Id<Lora> LoraId);
