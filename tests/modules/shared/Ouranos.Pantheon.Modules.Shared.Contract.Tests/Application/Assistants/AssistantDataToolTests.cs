using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Application.Assistants;

public sealed class AssistantDataToolTests
{
    public sealed record TestContext(string Topic);

    public sealed record TestArguments(string Query);

    public sealed record TestData(string Topic, string Query);

    private sealed class TestTool(Func<TestContext, TestArguments, AssistantData<TestData>> query)
        : AssistantDataTool<TestContext, TestArguments, TestData>(Substitute.For<ILogger>())
    {
        public int Calls { get; private set; }

        public override string Name => "test";

        public override string Description => "A test tool.";

        protected override ValueTask<AssistantData<TestData>> QueryAsync(
            TestContext context,
            TestArguments arguments,
            CancellationToken cancellationToken
        )
        {
            Calls++;
            return ValueTask.FromResult(query(context, arguments));
        }
    }

    [Fact]
    public async Task InvokeAsync_WhenQueryReturnsData_ShouldSerializeItForTheModel()
    {
        // Arrange
        var tool = new TestTool(
            (context, arguments) => new TestData(context.Topic, arguments.Query)
        );

        // Act
        var result = await tool.InvokeAsync(
            new TestContext("prices"),
            """{"query":"latest"}""",
            CancellationToken.None
        );

        // Assert
        result.Output.ShouldBe("""{"topic":"prices","query":"latest"}""");
        result.Action.ShouldBeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenQueryReturnsANote_ShouldSerializeTheNote()
    {
        // Arrange
        var tool = new TestTool((_, _) => AssistantData<TestData>.WithNote("Nothing here."));

        // Act
        var result = await tool.InvokeAsync(
            new TestContext("prices"),
            """{"query":"latest"}""",
            CancellationToken.None
        );

        // Assert
        result.Output.ShouldBe("""{"note":"Nothing here."}""");
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentsAreInvalid_ShouldNotQuery()
    {
        // Arrange
        var tool = new TestTool(
            (context, arguments) => new TestData(context.Topic, arguments.Query)
        );

        // Act
        var result = await tool.InvokeAsync(
            new TestContext("prices"),
            "not json",
            CancellationToken.None
        );

        // Assert
        result.Output.ShouldBe(AssistantTool<TestContext, TestArguments>.InvalidArgumentsOutput);
        tool.Calls.ShouldBe(0);
    }
}
