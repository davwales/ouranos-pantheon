namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;

public sealed class InvalidFilterException : Exception
{
    public InvalidFilterException() { }

    public InvalidFilterException(string message)
        : base(message) { }

    public InvalidFilterException(string message, Exception innerException)
        : base(message, innerException) { }
}
