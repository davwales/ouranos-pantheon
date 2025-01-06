using System.Runtime.CompilerServices;
using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Service.Aphrodite.Application.Interfaces.Conversations;

namespace Ouranos.Pantheon.Service.Aphrodite.Application.Commands.Conversations.GenerateCompletion;

public sealed class
    GenerateCompletionHandler : IStreamRequestHandler<GenerateCompletionInput, GenerateCompletionResponse>
{
    private readonly IGenerateCompletion _generateCompletion;
    private readonly ILogger<GenerateCompletionHandler> _logger;

    public GenerateCompletionHandler(
        ILogger<GenerateCompletionHandler> logger,
        IGenerateCompletion generateCompletion
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(generateCompletion);

        _logger = logger;
        _generateCompletion = generateCompletion;
    }

    public async IAsyncEnumerable<GenerateCompletionResponse> Handle(
        GenerateCompletionInput request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle generate completion request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        await foreach (var chunk in _generateCompletion.GenerateCompletionStream(request.Conversation,
                           cancellationToken))
        {
            yield return new GenerateCompletionResponse(chunk);

            cancellationToken.ThrowIfCancellationRequested();
        }

        _logger.LogDebug("Successfully handled get completion request.");
    }
}