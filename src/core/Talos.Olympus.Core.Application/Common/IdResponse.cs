using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Core.Application.Common;

public sealed record IdResponse<T>(
    Id<T> Id
);