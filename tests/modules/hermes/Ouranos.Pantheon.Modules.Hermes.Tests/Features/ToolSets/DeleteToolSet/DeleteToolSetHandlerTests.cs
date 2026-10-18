using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.DeleteToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.DeleteToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.DeleteToolSet;

public sealed class DeleteToolSetHandlerTests
{
    private readonly DeleteToolSetHandler _handler;
    private readonly ILogger<DeleteToolSetHandler> _logger = Substitute.For<
        ILogger<DeleteToolSetHandler>
    >();
    private readonly HermesDbContext _dbContext;

    public DeleteToolSetHandlerTests()
    {
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new DeleteToolSetHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldDeleteToolSetWithToolsAndReturnId()
    {
        // Arrange
        var toolSetId = new Id<ToolSet>(Guid.NewGuid().ToString());

        var tool = ToolSetTool.Create(
            new Id<ToolSetTool>(Guid.NewGuid().ToString()),
            toolSetId,
            ToolSource.Native,
            "echo"
        );

        var toolSet = ToolSet.Create(toolSetId, "Research", null, null, [tool]);
        await _dbContext.SeedData(toolSet);
        _dbContext.ChangeTracker.Clear();

        var command = new DeleteToolSetInput(toolSetId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ToolSetId.ShouldBe(toolSetId);
        (await _dbContext.ToolSets.FindAsync(toolSetId)).ShouldBeNull();
        (await _dbContext.ToolSetTools.FindAsync(tool.Id)).ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenToolSetNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteToolSetInput(new Id<ToolSet>(Guid.NewGuid().ToString()));

        // Act
        var handle = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeleteToolSetInput(new Id<ToolSet>(Guid.NewGuid().ToString()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var handle = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
