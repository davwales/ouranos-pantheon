using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Hermes.Application.Interfaces.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;

public sealed class GenerateCompletionHandler :
    CommandHandler<GenerateCompletionInput, StreamResponse<string, GenerateCompletionResponse>>
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

    protected override async Task<StreamResponse<string, GenerateCompletionResponse>> Handle(
        GenerateCompletionInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle generate completion query '{@query}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var stream = new StreamResponse<string, GenerateCompletionResponse>(
            async token => await Task.FromResult(
                _generateCompletion.GenerateCompletionStream(command.Conversation, token)
            ),
            async chunk => await Task.FromResult(new GenerateCompletionResponse(chunk))
        );

        _logger.LogDebug("Successfully handled generate completion request.");
        return await Task.FromResult(stream);
    }
}