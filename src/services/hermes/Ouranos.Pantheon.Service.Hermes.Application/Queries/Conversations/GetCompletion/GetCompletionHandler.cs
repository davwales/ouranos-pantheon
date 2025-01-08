using System.Runtime.CompilerServices;
using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Service.Hermes.Application.Interfaces.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Application.Queries.Conversations.GetCompletion;

public sealed class GetCompletionHandler : IStreamRequestHandler<GetCompletionInput, Chunk<string>>
{
    private readonly IGenerateCompletion _generateCompletion;
    private readonly ILogger<GetCompletionHandler> _logger;

    public GetCompletionHandler(
        ILogger<GetCompletionHandler> logger,
        IGenerateCompletion generateCompletion
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(generateCompletion);

        _logger = logger;
        _generateCompletion = generateCompletion;
    }

    public async IAsyncEnumerable<Chunk<string>> Handle(
        GetCompletionInput request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get completion request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        await foreach (var chunk in _generateCompletion.GenerateCompletionStream(request.Conversation,
                           cancellationToken))
        {
            yield return new Chunk<string>(chunk);
            cancellationToken.ThrowIfCancellationRequested();
        }

        _logger.LogDebug("Successfully handled get completion request.");
    }
}