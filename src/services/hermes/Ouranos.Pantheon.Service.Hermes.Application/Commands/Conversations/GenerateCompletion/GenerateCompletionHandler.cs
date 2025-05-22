using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Hermes.Application.Interfaces.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;

public sealed class GenerateCompletionHandler
    : CommandHandler<GenerateCompletionInput, StreamResponse<string, GenerateCompletionResponse>>
{
    private readonly IGenerateChatCompletion _generateChatCompletion;
    private readonly ILogger<GenerateCompletionHandler> _logger;

    public GenerateCompletionHandler(
        ILogger<GenerateCompletionHandler> logger,
        IGenerateChatCompletion generateChatCompletion
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(generateChatCompletion);

        _logger = logger;
        _generateChatCompletion = generateChatCompletion;
    }

    public override async Task<StreamResponse<string, GenerateCompletionResponse>> Handle(
        GenerateCompletionInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle generate completion query '{@query}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var stream = new StreamResponse<string, GenerateCompletionResponse>(
            async token => await Task.FromResult(
                _generateChatCompletion.GenerateCompletionStream(command.Conversation, token)
            ),
            async chunk => await Task.FromResult(new GenerateCompletionResponse(chunk))
        );

        _logger.LogDebug("Successfully handled generate completion request.");
        return await Task.FromResult(stream);
    }
}