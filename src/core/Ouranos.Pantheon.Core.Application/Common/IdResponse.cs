using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Common;

public sealed record IdResponse<T>(
    Id<T> Id
);