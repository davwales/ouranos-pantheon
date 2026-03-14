using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Infra.OuranosMl;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;
using Ouranos.Pantheon.Hermes.Service.Application.Commands.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Hermes.Service.Application.Interfaces.Conversations;
using Ouranos.Pantheon.Hermes.Service.Domain.Conversations;

namespace Ouranos.Pantheon.Hermes.Service.Infra.OuranosMl.Conversations;

public sealed class GenerateChatCompletion : IGenerateChatCompletion
{
    private readonly ILogger<GenerateChatCompletion> _logger;
    private readonly IOuranosMachineLearningClient _ouranosClient;

    public GenerateChatCompletion(
        ILogger<GenerateChatCompletion> logger,
        IOuranosMachineLearningClient ouranosClient
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(ouranosClient);

        _logger = logger;
        _ouranosClient = ouranosClient;
    }

    
}