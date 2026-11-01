using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.PublicMode.GetPublicMode;
using Ouranos.Pantheon.Modules.Hermes.Features.PublicMode.GetPublicMode.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.PublicMode.GetPublicMode;

public sealed class GetPublicModeHandlerTests
{
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();
    private readonly ILogger<GetPublicModeHandler> _logger = Substitute.For<
        ILogger<GetPublicModeHandler>
    >();
    private readonly GetPublicModeHandler _handler;

    public GetPublicModeHandlerTests()
    {
        _handler = new GetPublicModeHandler(_logger, _flagsmith);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GetPublicModeHandler(null!, _flagsmith));
    }

    [Fact]
    public void Constructor_WhenFlagsmithIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GetPublicModeHandler(_logger, null!));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldReturnFlagState(bool enabled)
    {
        // Arrange
        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(enabled));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));

        // Act
        var result = await _handler.Handle(new GetPublicModeInput(), CancellationToken.None);

        // Assert
        result.IsEnabled.ShouldBe(enabled);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(new GetPublicModeInput(), cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
