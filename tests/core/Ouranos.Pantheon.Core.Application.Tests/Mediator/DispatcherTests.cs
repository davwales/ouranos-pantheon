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
    private readonly IMediator _mediator;

    public DispatcherTests()
    {
        _mediator = Substitute.For<IMediator>();
        _dispatcher = new Dispatcher(_mediator);
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
        await _mediator.Received(1).Send<IRequest>(request, cts.Token);
    }

    [Fact]
    public async Task Send_ShouldInvokeMediatorAndReturnExpectedResult()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedResult = fixture.Create<TestEntity>();
        var request = new TestRequestWithResult();
        var cts = new CancellationTokenSource();
        var requestClient = SetupRequestClient(request, expectedResult, cts.Token);

        // Act
        var actualResult = await _dispatcher.Send(request, cts.Token);

        // Assert
        actualResult.ShouldBe(expectedResult);
        await requestClient.Received(1).GetResponse<TestEntity>(request, cts.Token);
    }

    [Fact]
    public async Task CreateStream_ShouldInvokeMediatorAndReturnExpectedResult()
    {
        // Arrange
        var fixture = new Fixture();
        var request = new TestStreamRequest();
        var cts = new CancellationTokenSource();
        var streamedResults = fixture.CreateMany<string>().ToList();

        var requestClient = SetupRequestClient(request, new StreamResponse<string, TestEntity>(
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

        await requestClient
            .Received(1)
            .GetResponse<StreamResponse<string, TestEntity>>(request, cts.Token);
    }

    private IRequestClient<IRequest<T>> SetupRequestClient<T>(
        IRequest<T> request,
        T response,
        CancellationToken cancellationToken
    ) where T : class
    {
        var subResponse = Substitute.For<Response<T>>();
        var requestClient = Substitute.For<IRequestClient<IRequest<T>>>();

        _mediator.CreateRequestClient<IRequest<T>>().Returns(requestClient);
        requestClient.GetResponse<T>(request, cancellationToken).Returns(subResponse);
        subResponse.Message.Returns(response);

        return requestClient;
    }

    public sealed record TestRequest : IRequest;

    public sealed class TestRequestWithResult : IRequest<TestEntity>;

    public sealed class TestStreamRequest : IRequest<StreamResponse<string, TestEntity>>;
}