using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Extensions;

/// <summary>
/// Extension methods for converting strongly-typed <see cref="Id{T}"/> values
/// into the <see cref="Guid"/> stream keys used by Marten event-sourced aggregates.
/// </summary>
public static class IdExtensions
{
    /// <summary>
    /// Parses the strongly-typed <see cref="Id{T}"/> value as a <see cref="Guid"/>,
    /// suitable for use as the Marten event stream key for event-sourced aggregates
    /// (i.e. the <see cref="BaseEventSourcedEntity.Id"/> of <typeparamref name="T"/>).
    /// </summary>
    /// <param name="id">
    /// The strongly-typed identifier whose wrapped value is a Guid string.
    /// </param>
    /// <returns>The <see cref="Guid"/> representation of <paramref name="id"/>'s value.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the wrapped value is not a valid Guid.
    /// </exception>
    public static Guid GetStreamId<T>(this Id<T> id)
        where T : BaseEventSourcedEntity
    {
        return Guid.Parse(id.Value);
    }

    /// <summary>
    /// Attempts to parse the strongly-typed <see cref="Id{T}"/> value as a
    /// <see cref="Guid"/> stream key for event-sourced aggregates. Returns
    /// <c>false</c> for null, empty, or non-Guid values without throwing.
    /// </summary>
    /// <param name="id">
    /// The strongly-typed identifier whose wrapped value may be a Guid string.
    /// </param>
    /// <param name="streamId">
    /// When this method returns <c>true</c>, the <see cref="Guid"/> representation
    /// of <paramref name="id"/>'s value; otherwise <see cref="Guid.Empty"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the value was parsed successfully; <c>false</c> otherwise.
    /// </returns>
    public static bool TryGetStreamId<T>(this Id<T> id, out Guid streamId)
        where T : BaseEventSourcedEntity
    {
        return Guid.TryParse(id.Value, out streamId);
    }
}
