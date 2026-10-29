namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

/// <summary>The request breaks a business rule (e.g. it references data that doesn't fit together); surfaced as 400.</summary>
public sealed class BadRequestException : Exception
{
    public BadRequestException() { }

    public BadRequestException(string message)
        : base(message) { }

    public BadRequestException(string message, Exception innerException)
        : base(message, innerException) { }
}
