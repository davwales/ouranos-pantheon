namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

/// <summary>The request conflicts with the resource's current state; surfaced as 409.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException() { }

    public ConflictException(string message)
        : base(message) { }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
