using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Application.Common;

public sealed record IdResponse<T>(Id<T> Id);
