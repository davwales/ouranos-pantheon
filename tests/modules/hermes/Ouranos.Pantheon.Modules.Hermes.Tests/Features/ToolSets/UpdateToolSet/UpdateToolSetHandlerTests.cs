using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.UpdateToolSet;

public sealed class UpdateToolSetHandlerTests
{
    private readonly UpdateToolSetHandler _handler;
    private readonly ILogger<UpdateToolSetHandler> _logger = Substitute.For<
        ILogger<UpdateToolSetHandler>
    >();
    private readonly HermesDbContext _dbContext;
    private readonly IHermesToolSource _toolSource = Substitute.For<IHermesToolSource>();

    public UpdateToolSetHandlerTests()
    {
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new UpdateToolSetHandler(_logger, _dbContext, [_toolSource]);

        _toolSource
            .ListTools()
            .Returns([
                new AvailableTool(ToolSource.Native, "echo", "Echoes text."),
                new AvailableTool(ToolSource.Native, "image", "Draws."),
            ]);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateToolSetAndReturnId()
    {
        // Arrange
        var toolSet = await SeedToolSet(("echo", "Old."), ("search", null));
        var echoId = toolSet.Tools.Single(t => t.Name == "echo").Id;

        var command = new UpdateToolSetInput(
            toolSet.Id,
            "  Writing  ",
            "New desc",
            "New instr",
            [
                new UpdateToolSetToolInput(ToolSource.Native, "echo", "New."),
                new UpdateToolSetToolInput(ToolSource.Native, "image"),
            ],
            false
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ToolSetId.ShouldBe(toolSet.Id);
        _dbContext.ChangeTracker.Clear();

        var saved = await _dbContext
            .ToolSets.Include(s => s.Tools)
            .SingleAsync(s => s.Id == toolSet.Id);

        saved.Name.ShouldBe("Writing");
        saved.Description.ShouldBe("New desc");
        saved.Instructions.ShouldBe("New instr");
        saved.IsPublic.ShouldBeFalse();
        saved.Tools.Select(t => t.Name).Order().ShouldBe(["echo", "image"]);

        var echo = saved.Tools.Single(t => t.Name == "echo");
        echo.Id.ShouldBe(echoId);
        echo.AdditionalDescription.ShouldBe("New.");
        (await _dbContext.ToolSetTools.CountAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenToolsNull_ShouldRemoveAllTools()
    {
        // Arrange
        var toolSet = await SeedToolSet(("echo", null));
        var command = new UpdateToolSetInput(toolSet.Id, "Research");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _dbContext.ChangeTracker.Clear();
        (await _dbContext.ToolSetTools.CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenSavedToolNoLongerOffered_ShouldKeepIt()
    {
        // Arrange
        var toolSet = await SeedToolSet(("retired", "Old."));
        var command = new UpdateToolSetInput(
            toolSet.Id,
            "Research",
            Tools: [new UpdateToolSetToolInput(ToolSource.Native, "retired", "New.")]
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _dbContext.ChangeTracker.Clear();

        var saved = await _dbContext
            .ToolSets.Include(s => s.Tools)
            .SingleAsync(s => s.Id == toolSet.Id);
        var tool = saved.Tools.ShouldHaveSingleItem();

        tool.Name.ShouldBe("retired");
        tool.AdditionalDescription.ShouldBe("New.");
    }

    [Fact]
    public async Task Handle_WhenToolNotOfferedAndNotSaved_ShouldThrowArgumentException()
    {
        // Arrange
        var toolSet = await SeedToolSet(("echo", null));
        var command = new UpdateToolSetInput(
            toolSet.Id,
            "Writing",
            Tools: [new UpdateToolSetToolInput(ToolSource.Native, "missing")]
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await handle.ShouldThrowAsync<ArgumentException>();
        exception.Message.ShouldContain("missing");
        _dbContext.ChangeTracker.Clear();
        (await _dbContext.ToolSets.SingleAsync()).Name.ShouldBe("Research");
    }

    [Fact]
    public async Task Handle_WhenToolSetNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateToolSetInput(DatabaseExtensions.CreateId<ToolSet>(), "Research");

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new UpdateToolSetInput(DatabaseExtensions.CreateId<ToolSet>(), "Research");
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }

    private async Task<ToolSet> SeedToolSet(
        params (string Name, string? AdditionalDescription)[] tools
    )
    {
        var id = DatabaseExtensions.CreateId<ToolSet>();

        var toolSet = ToolSet.Create(
            id,
            "Research",
            "Desc",
            "Instr",
            tools.Select(t =>
                ToolSetTool.Create(
                    DatabaseExtensions.CreateId<ToolSetTool>(),
                    id,
                    ToolSource.Native,
                    t.Name,
                    t.AdditionalDescription
                )
            )
        );

        await _dbContext.SeedData(toolSet);
        _dbContext.ChangeTracker.Clear();
        return toolSet;
    }
}
