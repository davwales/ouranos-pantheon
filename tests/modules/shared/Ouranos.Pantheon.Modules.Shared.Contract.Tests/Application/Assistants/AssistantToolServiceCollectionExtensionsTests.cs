using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Application.Assistants;

public sealed class AssistantToolServiceCollectionExtensionsTests
{
    public interface ITopicScope
    {
        string Topic { get; }
    }

    public sealed record FirstContext(string Topic) : ITopicScope;

    public sealed record SecondContext(string Topic, int Page) : ITopicScope;

    public sealed class TopicTool()
        : AssistantTool<ITopicScope, AssistantToolNoArguments>(Substitute.For<ILogger>())
    {
        public override string Name => "topic";

        public override string Description => "Echoes the topic.";

        protected override ValueTask<AssistantToolResult> ExecuteAsync(
            ITopicScope context,
            AssistantToolNoArguments arguments,
            CancellationToken cancellationToken
        )
        {
            return ValueTask.FromResult(new AssistantToolResult(context.Topic));
        }
    }

    [Fact]
    public void AddAssistantTool_ShouldRegisterTheToolForTheContext()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAssistantTool<FirstContext, TopicTool>();

        // Assert
        services.ShouldContainServiceWithImplementation<IAssistantTool<FirstContext>, TopicTool>(
            ServiceLifetime.Scoped
        );
    }

    [Fact]
    public async Task AddAssistantTool_WhenToolIsTypedToAScope_ShouldServeEveryContextThatImplementsIt()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services
            .AddAssistantTool<FirstContext, TopicTool>()
            .AddAssistantTool<SecondContext, TopicTool>();

        // Assert
        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var first = scope.ServiceProvider.GetServices<IAssistantTool<FirstContext>>().Single();
        var second = scope.ServiceProvider.GetServices<IAssistantTool<SecondContext>>().Single();

        (
            await first.InvokeAsync(new FirstContext("prices"), "{}", CancellationToken.None)
        ).Output.ShouldBe("prices");

        (
            await second.InvokeAsync(new SecondContext("signals", 2), "{}", CancellationToken.None)
        ).Output.ShouldBe("signals");
    }
}
