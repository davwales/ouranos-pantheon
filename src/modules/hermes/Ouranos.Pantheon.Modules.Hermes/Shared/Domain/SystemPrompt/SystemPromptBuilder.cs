using System.Text;
using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.SystemPrompt;

/// <summary>
/// Builder for constructing a structured system prompt from domain entities or raw data.
/// The composed prompt contains sections that guide the model on how to interpret
/// personas and traits, so that model-specific system prompts can focus on behavioral
/// details rather than explaining the platform itself.
/// </summary>
public sealed class SystemPromptBuilder
{
    private string? _personaName;
    private string? _personaDescription;
    private string? _personaPersonality;
    private string? _personaScenario;
    private string _modelSystemPrompt = string.Empty;
    private readonly List<(string Name, string Content)> _traits = [];

    /// <summary>
    /// Sets the persona from a domain entity.
    /// </summary>
    public SystemPromptBuilder WithPersona(Persona persona)
    {
        Guard.Against.Null(persona);

        _personaName = persona.Name;
        _personaDescription = persona.Description;
        _personaPersonality = persona.Personality;
        _personaScenario = persona.Scenario;

        return this;
    }

    /// <summary>
    /// Sets the persona from raw data.
    /// </summary>
    public SystemPromptBuilder WithPersona(
        string name,
        string description,
        string? personality = null,
        string? scenario = null
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(description);

        _personaName = name;
        _personaDescription = description;
        _personaPersonality = personality;
        _personaScenario = scenario;

        return this;
    }

    /// <summary>
    /// Sets the model from its system prompt string.
    /// </summary>
    public SystemPromptBuilder WithModel(string systemPrompt)
    {
        Guard.Against.NullOrWhiteSpace(systemPrompt);

        _modelSystemPrompt = systemPrompt;

        return this;
    }

    /// <summary>
    /// Sets the model config, extracting its system prompt.
    /// </summary>
    public SystemPromptBuilder WithModel(ModelConfig modelConfig)
    {
        Guard.Against.Null(modelConfig);

        _modelSystemPrompt = modelConfig.SystemPrompt;

        return this;
    }

    /// <summary>
    /// Adds a trait from a domain entity.
    /// </summary>
    public SystemPromptBuilder AddTrait(Trait trait)
    {
        Guard.Against.Null(trait);

        _traits.Add((trait.Name, trait.Content));

        return this;
    }

    /// <summary>
    /// Adds a trait from raw data.
    /// </summary>
    public SystemPromptBuilder AddTrait(string name, string content)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(content);

        _traits.Add((name, content));

        return this;
    }

    /// <summary>
    /// Adds multiple traits from domain entities.
    /// </summary>
    public SystemPromptBuilder AddTraits(IEnumerable<Trait> traits)
    {
        Guard.Against.Null(traits);

        foreach (var trait in traits)
        {
            AddTrait(trait);
        }

        return this;
    }

    /// <summary>
    /// Builds the composed system prompt string.
    /// </summary>
    public string Build()
    {
        if (string.IsNullOrWhiteSpace(_personaName))
        {
            throw new InvalidOperationException(
                "Persona is required. Call WithPersona before Build."
            );
        }

        if (string.IsNullOrWhiteSpace(_modelSystemPrompt))
        {
            throw new InvalidOperationException("Model is required. Call WithModel before Build.");
        }

        List<Action<StringBuilder>> sections = [ComposeModel, ComposePersona, ComposeTraits];

        var builder = new StringBuilder();
        foreach (var section in sections)
        {
            builder.AppendLine();
            section(builder);
            builder.AppendLine();
        }

        return builder.ToString().Trim();
    }

    private void ComposePersona(StringBuilder builder)
    {
        builder.Append(
            $"""
            ## Persona

            You are acting as a character defined below. Adopt this persona fully - respond in their voice, reflect their knowledge and mannerisms, and stay in character throughout the conversation.

            **Name:** {_personaName}
            **Description:** {_personaDescription}
            """
        );

        if (!string.IsNullOrWhiteSpace(_personaPersonality))
        {
            builder.AppendLine();
            builder.Append($"**Personality:** {_personaPersonality}");
        }

        if (!string.IsNullOrWhiteSpace(_personaScenario))
        {
            builder.AppendLine();
            builder.Append($"**Scenario:** {_personaScenario}");
        }
    }

    private void ComposeModel(StringBuilder builder)
    {
        builder.Append(
            $"""
            ## Instructions

            {_modelSystemPrompt}
            """
        );
    }

    private void ComposeTraits(StringBuilder builder)
    {
        if (_traits.Count == 0)
        {
            return;
        }

        builder.Append(
            """
            ## Traits

            The following traits modify your persona. Each trait takes precedence over your base persona and the instructions above. If a trait contradicts another part of this system prompt, follow the trait.
            """
        );

        foreach (var (name, content) in _traits)
        {
            builder.AppendLine();
            builder.AppendLine();

            builder.Append(
                $"""
                ### {name}

                {content}
                """
            );
        }
    }
}
