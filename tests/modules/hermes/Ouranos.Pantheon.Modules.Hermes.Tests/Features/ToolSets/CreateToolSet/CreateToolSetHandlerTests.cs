using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.CreateToolSet;

public sealed class CreateToolSetHandlerTests
{
    private readonly CreateToolSetHandler _handler;
    private readonly ILogger<CreateToolSetHandler> _logger = Substitute.For<
        ILogger<CreateToolSetHandler>
    >();
    private readonly HermesDbContext _dbContext;
    private readonly IHermesToolSource _toolSource = Substitute.For<IHermesToolSource>();

    public CreateToolSetHandlerTests()
    {
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new CreateToolSetHandler(_logger, _dbContext, [_toolSource]);

        _toolSource
            .ListTools()
            .Returns([
                new AvailableTool(ToolSource.Native, "echo", "Echoes text."),
                new AvailableTool(ToolSource.Native, "image", "Draws."),
            ]);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateToolSetAndReturnId()
    {
        // Arrange
        var command = new CreateToolSetInput(
            "  Research  ",
            "Desc",
            "Instr",
            [
                new CreateToolSetToolInput(ToolSource.Native, "echo", "Be brief."),
                new CreateToolSetToolInput(ToolSource.Native, "image"),
            ],
            false
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ToolSetId.ShouldNotBe(default);
        _dbContext.ChangeTracker.Clear();

        var saved = await _dbContext
            .ToolSets.Include(s => s.Tools)
            .SingleAsync(s => s.Id == result.ToolSetId);

        saved.Name.ShouldBe("Research");
        saved.Description.ShouldBe("Desc");
        saved.Instructions.ShouldBe("Instr");
        saved.IsPublic.ShouldBeFalse();
        saved.Tools.Count.ShouldBe(2);
        saved.Tools.ShouldAllBe(t => t.ToolSetId == result.ToolSetId);
        saved.Tools.Single(t => t.Name == "echo").AdditionalDescription.ShouldBe("Be brief.");
        saved.Tools.Single(t => t.Name == "image").AdditionalDescription.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenToolsNull_ShouldCreateEmptyToolSet()
    {
        // Arrange
        var command = new CreateToolSetInput("Research");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _dbContext.ChangeTracker.Clear();

        var saved = await _dbContext
            .ToolSets.Include(s => s.Tools)
            .SingleAsync(s => s.Id == result.ToolSetId);

        saved.Tools.ShouldBeEmpty();
        saved.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenToolNotOffered_ShouldThrowArgumentExceptionAndNotSave()
    {
        // Arrange
        var command = new CreateToolSetInput(
            "Research",
            Tools: [new CreateToolSetToolInput(ToolSource.Native, "missing")]
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await handle.ShouldThrowAsync<ArgumentException>();
        exception.Message.ShouldContain("missing");
        (await _dbContext.ToolSets.CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenDuplicateTools_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateToolSetInput(
            "Research",
            Tools:
            [
                new CreateToolSetToolInput(ToolSource.Native, "echo"),
                new CreateToolSetToolInput(ToolSource.Native, "echo"),
            ]
        );

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new CreateToolSetInput("Research");
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
