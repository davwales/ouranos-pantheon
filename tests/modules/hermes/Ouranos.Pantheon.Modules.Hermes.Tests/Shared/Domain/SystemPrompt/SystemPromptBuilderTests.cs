using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.SystemPrompt;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.SystemPrompt;

public sealed class SystemPromptBuilderTests
{
    [Fact]
    public void Build_WhenFullPersona_ShouldIncludeAllPersonaFields()
    {
        // Arrange
        var builder = new SystemPromptBuilder()
            .WithPersona("TestBot", "A helpful bot", "Friendly", "In a chat")
            .WithModel("You are an assistant.");

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe(
            """
            ## Instructions

            You are an assistant.

            ## Persona

            You are acting as a character defined below. Adopt this persona fully — respond in their voice, reflect their knowledge and mannerisms, and stay in character throughout the conversation.

            **Name:** TestBot
            **Description:** A helpful bot
            **Personality:** Friendly
            **Scenario:** In a chat
            """
        );
    }

    [Fact]
    public void Build_WhenNoPersonalityOrScenario_ShouldOmitThem()
    {
        // Arrange
        var builder = new SystemPromptBuilder()
            .WithPersona("TestBot", "A helpful bot")
            .WithModel("You are an assistant.");

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe(
            """
            ## Instructions

            You are an assistant.

            ## Persona

            You are acting as a character defined below. Adopt this persona fully — respond in their voice, reflect their knowledge and mannerisms, and stay in character throughout the conversation.

            **Name:** TestBot
            **Description:** A helpful bot
            """
        );
    }

    [Fact]
    public void Build_WhenTraitsProvidedFromRawData_ShouldIncludeTraitSection()
    {
        // Arrange
        var builder = new SystemPromptBuilder()
            .WithPersona("TestBot", "A helpful bot")
            .WithModel("Base prompt.")
            .AddTrait("Kindness", "Always be kind")
            .AddTrait("Brevity", "Be concise");

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe(
            """
            ## Instructions

            Base prompt.

            ## Persona

            You are acting as a character defined below. Adopt this persona fully — respond in their voice, reflect their knowledge and mannerisms, and stay in character throughout the conversation.

            **Name:** TestBot
            **Description:** A helpful bot

            ## Traits

            The following traits modify your persona. Each trait takes precedence over your base persona and the instructions above. If a trait contradicts another part of this system prompt, follow the trait.

            ### Kindness

            Always be kind

            ### Brevity

            Be concise
            """
        );
    }

    [Fact]
    public void Build_WhenNoTraits_ShouldOmitTraitSection()
    {
        // Arrange
        var builder = new SystemPromptBuilder()
            .WithPersona("TestBot", "A helpful bot")
            .WithModel("Base prompt.");

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe(
            """
            ## Instructions

            Base prompt.

            ## Persona

            You are acting as a character defined below. Adopt this persona fully — respond in their voice, reflect their knowledge and mannerisms, and stay in character throughout the conversation.

            **Name:** TestBot
            **Description:** A helpful bot
            """
        );
    }

    [Fact]
    public void Build_WhenTraitsProvided_ShouldIncludePrecedenceGuidance()
    {
        // Arrange
        var builder = new SystemPromptBuilder()
            .WithPersona("Bot", "A bot")
            .WithModel("Instructions.")
            .AddTrait("Bold", "Be bold");

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe(
            """
            ## Instructions

            Instructions.

            ## Persona

            You are acting as a character defined below. Adopt this persona fully — respond in their voice, reflect their knowledge and mannerisms, and stay in character throughout the conversation.

            **Name:** Bot
            **Description:** A bot

            ## Traits

            The following traits modify your persona. Each trait takes precedence over your base persona and the instructions above. If a trait contradicts another part of this system prompt, follow the trait.

            ### Bold

            Be bold
            """
        );
    }

    [Fact]
    public void Build_WhenPersonaProvided_ShouldIncludePersonaGuidance()
    {
        // Arrange
        var builder = new SystemPromptBuilder()
            .WithPersona("Bot", "A bot")
            .WithModel("Instructions.");

        // Act
        var result = builder.Build();

        // Assert
        result.ShouldBe(
            """
            ## Instructions

            Instructions.

            ## Persona

            You are acting as a character defined below. Adopt this persona fully — respond in their voice, reflect their knowledge and mannerisms, and stay in character throughout the conversation.

            **Name:** Bot
            **Description:** A bot
            """
        );
    }

    [Fact]
    public void Build_WhenPersonaNotSet_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new SystemPromptBuilder().WithModel("Prompt.");

        // Act
        var act = () => builder.Build();

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldContain("Persona");
    }

    [Fact]
    public void Build_WhenModelNotSet_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new SystemPromptBuilder().WithPersona("Bot", "A bot");

        // Act
        var act = () => builder.Build();

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldContain("Model is required");
    }

    [Fact]
    public void Build_ShouldOrderSectionsCorrectly()
    {
        // Arrange
        var builder = new SystemPromptBuilder()
            .WithPersona("Bot", "A bot", "Friendly", "In a chat")
            .WithModel("Instructions.")
            .AddTrait("Bold", "Be bold");

        // Act
        var result = builder.Build();

        // Assert
        var instructionsIndex = result.IndexOf("## Instructions");
        var personaIndex = result.IndexOf("## Persona");
        var traitsIndex = result.IndexOf("## Traits");

        instructionsIndex.ShouldBeLessThan(personaIndex);
        personaIndex.ShouldBeLessThan(traitsIndex);
    }

    [Fact]
    public void WithPersona_WhenNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new SystemPromptBuilder();

        // Act
        var act = () => builder.WithPersona((Persona)null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void WithModel_WhenNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new SystemPromptBuilder();

        // Act
        var act = () => builder.WithModel((ModelConfig)null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void AddTrait_WhenNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new SystemPromptBuilder();

        // Act
        var act = () => builder.AddTrait((Trait)null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void WithPersona_WhenNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new SystemPromptBuilder();

        // Act
        var act = () => builder.WithPersona(null!, "description");

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void WithModel_WhenNullString_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new SystemPromptBuilder();

        // Act
        var act = () => builder.WithModel((string)null!);

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void AddTrait_WhenNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new SystemPromptBuilder();

        // Act
        var act = () => builder.AddTrait(null!, "content");

        // Assert
        act.ShouldThrow<ArgumentException>();
    }
}
