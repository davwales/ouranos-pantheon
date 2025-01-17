using MassTransit;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.Service.Hermes.Application.Interfaces.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;

public sealed class GenerateCompletionHandler :
    IQueryHandler<GenerateCompletionInput, StreamResponse<string, GenerateCompletionResponse>>
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

    public async Task Consume(ConsumeContext<GenerateCompletionInput> context)
    {
        _logger.LogTrace("Attempting to handle generate completion query '{@query}'.", context.Message);
        context.CancellationToken.ThrowIfCancellationRequested();

        var stream = new StreamResponse<string, GenerateCompletionResponse>(
            async token => await Task.FromResult(
                _generateCompletion.GenerateCompletionStream(context.Message.Conversation, token)
            ),
            async chunk => await Task.FromResult(new GenerateCompletionResponse(chunk))
        );

        _logger.LogDebug("Successfully handled generate completion request.");
        await context.RespondAsync(stream);
    }
}