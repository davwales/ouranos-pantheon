using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Tests.Utils;

/// <summary>
/// Test fixture for <see cref="BaseEventSourcedEntity"/>-derived types,
/// used by unit tests of stream-id-related extension methods.
/// </summary>
public sealed record TestEventSourcedEntity : BaseEventSourcedEntity;
