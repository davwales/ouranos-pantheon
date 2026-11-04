using Flagsmith;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Features.FeatureFlags;
using Ouranos.Pantheon.Modules.Shared.Features.FeatureFlags.Schemas;
using Ouranos.Pantheon.Modules.Shared.Infra.Flagsmith;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Features.FeatureFlags;

public sealed class GetFeatureFlagsHandlerTests
{
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();
    private readonly IFlags _flags = Substitute.For<IFlags>();
    private readonly ILogger<GetFeatureFlagsHandler> _logger = Substitute.For<
        ILogger<GetFeatureFlagsHandler>
    >();
    private readonly IOptions<FlagsmithOptions> _options = Options.Create(
        new FlagsmithOptions
        {
            DefaultFlags = new Dictionary<string, bool>
            {
                ["module.fails_closed"] = true,
                ["module.overridden"] = true,
            },
        }
    );
    private readonly GetFeatureFlagsHandler _handler;

    public GetFeatureFlagsHandlerTests()
    {
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(_flags));
        _handler = new GetFeatureFlagsHandler(_logger, _flagsmith, _options);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetFeatureFlagsHandler(null!, _flagsmith, _options)
        );
    }

    [Fact]
    public void Constructor_WhenFlagsmithIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetFeatureFlagsHandler(_logger, null!, _options)
        );
    }

    [Fact]
    public void Constructor_WhenOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetFeatureFlagsHandler(_logger, _flagsmith, null!)
        );
    }

    [Fact]
    public async Task Handle_WhenFlagsmithReturnsFlags_ShouldOverlayThemOnDefaults()
    {
        // Arrange
        _flags
            .AllFlags()
            .Returns([
                new Flag(new Feature("module.overridden"), false, string.Empty),
                new Flag(new Feature("module.remote_only"), true, string.Empty),
            ]);

        // Act
        var result = await _handler.Handle(new GetFeatureFlagsRequest(), CancellationToken.None);

        // Assert
        result.Flags.ShouldBe(
            new Dictionary<string, bool>
            {
                ["module.fails_closed"] = true,
                ["module.overridden"] = false,
                ["module.remote_only"] = true,
            },
            ignoreOrder: true
        );
    }

    [Fact]
    public async Task Handle_WhenFlagsmithIsUnreachable_ShouldReturnDefaults()
    {
        // Arrange
        _flags.AllFlags().Returns((List<IFlag>)null!);

        // Act
        var result = await _handler.Handle(new GetFeatureFlagsRequest(), CancellationToken.None);

        // Assert
        result.Flags.ShouldBe(_options.Value.DefaultFlags, ignoreOrder: true);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () =>
            await _handler.Handle(new GetFeatureFlagsRequest(), cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
