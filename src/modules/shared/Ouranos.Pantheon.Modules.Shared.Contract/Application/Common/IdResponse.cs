using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

public sealed record IdResponse<T>(Id<T> Id);
