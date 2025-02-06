using MassTransit;
using MassTransit.Mediator;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Tests.Utils;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using Ouranos.Pantheon.Tests.Utils.Shouldly;

namespace Ouranos.Pantheon.Core.Application.Tests.Mediator;

public sealed class DispatcherTests
{
    private readonly Dispatcher _dispatcher;
    private readonly Mock<IMediator> _mockMediator = new();

    public DispatcherTests()
    {
        _dispatcher = new Dispatcher(_mockMediator.Object);
    }

    [Fact]
    public async Task Send_ShouldInvokeMediator()
    {
        // Arrange
        var request = new TestRequest();
        var cts = new CancellationTokenSource();

        // Act
        await _dispatcher.Send(request, cts.Token);

        // Assert
        _mockMediator.Verify(
            m => m.Send<IRequest>(request, cts.Token),
            Times.Once
        );
    }

    [Fact]
    public async Task Send_ShouldInvokeMediatorAndReturnExpectedResult()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedResult = fixture.Create<TestEntity>();
        var request = new TestRequestWithResult();
        var cts = new CancellationTokenSource();

        var mockRequestClient = SetupRequestClient(request, expectedResult, cts.Token);

        // Act
        var actualResult = await _dispatcher.Send(request, cts.Token);

        // Assert
        actualResult.ShouldBe(expectedResult);
        mockRequestClient.Verify(
            x => x.GetResponse<TestEntity>(request, cts.Token, default),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateStream_ShouldInvokeMediatorAndReturnExpectedResult()
    {
        // Arrange
        var fixture = new Fixture();
        var request = new TestStreamRequest();
        var cts = new CancellationTokenSource();
        var streamedResults = fixture.CreateMany<string>().ToList();

        var mockRequestClient = SetupRequestClient(request, new StreamResponse<string, TestEntity>(
            async _ => await Task.FromResult(streamedResults.ToAsyncEnumerable()),
            async str => await Task.FromResult(new TestEntity(new Id<TestEntity>(str)))
        ), cts.Token);

        // Act
        var stream = _dispatcher.CreateStream(request, cts.Token);

        // Assert
        await stream.ShouldMatchAsync(
            streamedResults.Select(s => new TestEntity(new Id<TestEntity>(s))).ToList(),
            (actual, expected) => actual.Id.ShouldBe(expected.Id)
        );

        mockRequestClient.Verify(
            x => x.GetResponse<StreamResponse<string, TestEntity>>(
                request, cts.Token, default),
            Times.Once
        );
    }

    private Mock<IRequestClient<IRequest<T>>> SetupRequestClient<T>(
        IRequest<T> request,
        T response,
        CancellationToken cancellationToken
    ) where T : class
    {
        var mockResponse = new Mock<Response<T>>();
        var mockRequestClient = new Mock<IRequestClient<IRequest<T>>>();

        _mockMediator
            .Setup(x => x.CreateRequestClient<IRequest<T>>(default))
            .Returns(mockRequestClient.Object);

        mockRequestClient
            .Setup(x => x.GetResponse<T>(request, cancellationToken, default))
            .ReturnsAsync(mockResponse.Object);

        mockResponse
            .SetupGet(x => x.Message)
            .Returns(response);

        return mockRequestClient;
    }

    public sealed record TestRequest : IRequest;

    public sealed class TestRequestWithResult : IRequest<TestEntity>;

    public sealed class TestStreamRequest : IRequest<StreamResponse<string, TestEntity>>;
}