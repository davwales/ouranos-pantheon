using System.Net;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

/// <summary>
/// An error reported by OuranosMl, either as an error status before a stream starts or as an
/// error event mid-stream. <see cref="Exception.Message"/> is OuranosMl's own text and is safe to
/// show to users.
/// </summary>
public sealed class OuranosMlRequestException(
    string type,
    string message,
    HttpStatusCode? statusCode = null
) : Exception(message)
{
    public string Type { get; } = type;

    public HttpStatusCode? StatusCode { get; } = statusCode;
}
