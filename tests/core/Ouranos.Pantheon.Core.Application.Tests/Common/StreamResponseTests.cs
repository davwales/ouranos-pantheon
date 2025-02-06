using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Tests.Utils;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Core.Application.Tests.Common;

public sealed class StreamResponseTests
{
    [Fact]
    public async Task Stream_ShouldYieldExpectedResponse()
    {
        // Arrange
        var fixture = new Fixture();
        var values = fixture.CreateMany<string>();
        var streamResponse = new StreamResponse<string, TestEntity>(
            async _ => await Task.FromResult(values.ToAsyncEnumerable()),
            async str => await Task.FromResult(new TestEntity(new Id<TestEntity>(str)))
        );

        // Act
        var actualStream = streamResponse.GetStream();

        // Assert
        await actualStream.ShouldMatchAsync(
            values.Select(x => new TestEntity(new Id<TestEntity>(x))).ToList(),
            (x1, x2) => x1.Id.ShouldBe(x2.Id)
        );
    }
}