using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.GetToolSet;

public sealed class GetToolSetHandlerTests
{
    private readonly GetToolSetHandler _handler;
    private readonly ILogger<GetToolSetHandler> _logger = Substitute.For<
        ILogger<GetToolSetHandler>
    >();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();
    private readonly IHermesToolSource _toolSource = Substitute.For<IHermesToolSource>();

    public GetToolSetHandlerTests()
    {
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetToolSetHandler(_logger, _dbContext, _flagsmith, [_toolSource]);

        _toolSource
            .ListTools()
            .Returns([new AvailableTool(ToolSource.Native, "echo", "Echoes text.")]);
        SetPublicMode(false);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnToolSetWithSortedToolsAndAvailability()
    {
        // Arrange
        var toolSet = await SeedToolSet(isPublic: true, ("search", null), ("echo", "Be brief."));

        // Act
        var result = await _handler.Handle(new GetToolSetInput(toolSet.Id), CancellationToken.None);

        // Assert
        result.Id.ShouldBe(toolSet.Id);
        result.Name.ShouldBe("Research");
        result.Description.ShouldBe("Desc");
        result.Instructions.ShouldBe("Instr");
        result.IsPublic.ShouldBeTrue();
        result.Tools.ShouldBe([
            new GetToolSetToolResponse(ToolSource.Native, "echo", "Be brief.", true),
            new GetToolSetToolResponse(ToolSource.Native, "search", null, false),
        ]);
    }

    [Fact]
    public async Task Handle_WhenToolSetNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetToolSetInput(new Id<ToolSet>(Guid.NewGuid().ToString()));

        // Act
        var handle = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotPublicAndPublicModeEnabled_ShouldThrowNotFoundException()
    {
        // Arrange
        var toolSet = await SeedToolSet(isPublic: false);
        SetPublicMode(true);

        // Act
        var handle = async () =>
            await _handler.Handle(new GetToolSetInput(toolSet.Id), CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotPublicAndPublicModeDisabled_ShouldReturnToolSet()
    {
        // Arrange
        var toolSet = await SeedToolSet(isPublic: false);

        // Act
        var result = await _handler.Handle(new GetToolSetInput(toolSet.Id), CancellationToken.None);

        // Assert
        result.Id.ShouldBe(toolSet.Id);
        result.IsPublic.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenPublicAndPublicModeEnabled_ShouldReturnToolSetWithoutCheckingFlags()
    {
        // Arrange
        var toolSet = await SeedToolSet(isPublic: true);
        SetPublicMode(true);
        _flagsmith.ClearReceivedCalls();

        // Act
        var result = await _handler.Handle(new GetToolSetInput(toolSet.Id), CancellationToken.None);

        // Assert
        result.Id.ShouldBe(toolSet.Id);
        await _flagsmith.DidNotReceive().GetEnvironmentFlags();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetToolSetInput(new Id<ToolSet>(Guid.NewGuid().ToString()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    private void SetPublicMode(bool isEnabled)
    {
        var flags = Substitute.For<IFlags>();
        flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(isEnabled));
        _flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(flags));
    }

    private async Task<ToolSet> SeedToolSet(
        bool isPublic,
        params (string Name, string? AdditionalDescription)[] tools
    )
    {
        var id = new Id<ToolSet>(Guid.NewGuid().ToString());
        var toolSet = ToolSet.Create(
            id,
            "Research",
            "Desc",
            "Instr",
            tools.Select(t =>
                ToolSetTool.Create(
                    new Id<ToolSetTool>(Guid.NewGuid().ToString()),
                    id,
                    ToolSource.Native,
                    t.Name,
                    t.AdditionalDescription
                )
            ),
            isPublic
        );
        await _dbContext.SeedData(toolSet);
        _dbContext.ChangeTracker.Clear();
        return toolSet;
    }
}
