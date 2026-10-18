using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Assistants.ToolSources;

public sealed class ToolSetResolverTests
{
    private readonly ILogger<ToolSetResolver> _logger = Substitute.For<ILogger<ToolSetResolver>>();
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly IDbContextFactory<HermesDbContext> _dbContextFactory;
    private readonly HermesChatContext _context = new(null, []);

    public ToolSetResolverTests()
    {
        _dbContextFactory = DbContextExtensions.MockFactory<HermesDbContext>(_dbName);
    }

    [Fact]
    public void Constructor_WhenLoggerNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        ILogger<ToolSetResolver> logger = null!;

        // Act
        var create = () => new ToolSetResolver(logger, _dbContextFactory, []);

        // Assert
        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenDbContextFactoryNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDbContextFactory<HermesDbContext> dbContextFactory = null!;

        // Act
        var create = () => new ToolSetResolver(_logger, dbContextFactory, []);

        // Assert
        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenToolSourcesNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<IHermesToolSource> toolSources = null!;

        // Act
        var create = () => new ToolSetResolver(_logger, _dbContextFactory, toolSources);

        // Assert
        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task ResolveAsync_WhenContextNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var resolver = CreateResolver();

        // Act
        var resolve = async () => await resolver.ResolveAsync(null, null!);

        // Assert
        await resolve.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ResolveAsync_WhenToolSetIdNull_ShouldReturnEmpty()
    {
        // Arrange
        var dbContextFactory = Substitute.For<IDbContextFactory<HermesDbContext>>();
        var resolver = new ToolSetResolver(
            _logger,
            dbContextFactory,
            [new NativeToolSource([CreateTool("echo")])]
        );

        // Act
        var result = await resolver.ResolveAsync(null, _context);

        // Assert
        result.ShouldBe(ResolvedToolSet.Empty);
        await dbContextFactory.DidNotReceive().CreateDbContextAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveAsync_WhenToolSetNotFound_ShouldReturnEmpty()
    {
        // Arrange
        var resolver = CreateResolver(CreateTool("echo"));

        // Act
        var result = await resolver.ResolveAsync(DatabaseExtensions.CreateId<ToolSet>(), _context);

        // Assert
        result.ShouldBe(ResolvedToolSet.Empty);
    }

    [Fact]
    public async Task ResolveAsync_WhenToolSetFound_ShouldReturnItsToolsAndInstructions()
    {
        // Arrange
        var toolSetId = await SeedToolSetAsync("Use echo.", ("echo", "Be brief."));
        var resolver = CreateResolver(CreateTool("echo"), CreateTool("image"));

        // Act
        var result = await resolver.ResolveAsync(toolSetId, _context);

        // Assert
        result.Instructions.ShouldBe("Use echo.");
        var tool = result.Tools.ShouldHaveSingleItem();
        tool.Name.ShouldBe("echo");
        tool.Description.ShouldBe("echo description\n\nBe brief.");
    }

    [Fact]
    public async Task ResolveAsync_WhenToolIsUnavailable_ShouldFilterItOut()
    {
        // Arrange
        var toolSetId = await SeedToolSetAsync(null, ("echo", null), ("image", null));
        var resolver = CreateResolver(CreateTool("echo"), CreateTool("image", isAvailable: false));

        // Act
        var result = await resolver.ResolveAsync(toolSetId, _context);

        // Assert
        result.Tools.ShouldHaveSingleItem().Name.ShouldBe("echo");
    }

    [Fact]
    public async Task ResolveAsync_WhenToolSetHasNoTools_ShouldReturnNoToolsWithInstructions()
    {
        // Arrange
        var toolSetId = await SeedToolSetAsync("Use echo.");
        var resolver = CreateResolver(CreateTool("echo"));

        // Act
        var result = await resolver.ResolveAsync(toolSetId, _context);

        // Assert
        result.Tools.ShouldBeEmpty();
        result.Instructions.ShouldBe("Use echo.");
    }

    [Fact]
    public async Task ResolveAsync_WhenToolSetFound_ShouldPassTheSetEntriesToTheMatchingSource()
    {
        // Arrange
        var toolSetId = await SeedToolSetAsync(null, ("echo", null), ("image", null));
        IEnumerable<ToolSetTool>? received = null;
        var source = Substitute.For<IHermesToolSource>();
        source.Source.Returns(ToolSource.Native);
        source.Resolve(Arg.Do<IEnumerable<ToolSetTool>>(e => received = [.. e])).Returns([]);
        var resolver = new ToolSetResolver(_logger, _dbContextFactory, [source]);

        // Act
        await resolver.ResolveAsync(toolSetId, _context);

        // Assert
        received.ShouldNotBeNull();
        received.Select(e => e.Name).ShouldBe(["echo", "image"], ignoreOrder: true);
    }

    [Fact]
    public async Task ResolveAsync_WhenToolIsAvailabilityChecked_ShouldUseTheGivenContext()
    {
        // Arrange
        var toolSetId = await SeedToolSetAsync(null, ("echo", null));
        HermesChatContext? checkedContext = null;
        var tool = CreateTool("echo");
        tool.IsAvailable(Arg.Do<HermesChatContext>(c => checkedContext = c)).Returns(true);
        var resolver = CreateResolver(tool);

        // Act
        await resolver.ResolveAsync(toolSetId, _context);

        // Assert
        checkedContext.ShouldBe(_context);
    }

    private ToolSetResolver CreateResolver(params IAssistantTool<HermesChatContext>[] tools)
    {
        return new ToolSetResolver(_logger, _dbContextFactory, [new NativeToolSource(tools)]);
    }

    private static IAssistantTool<HermesChatContext> CreateTool(
        string name,
        bool isAvailable = true
    )
    {
        var tool = Substitute.For<IAssistantTool<HermesChatContext>>();
        tool.Name.Returns(name);
        tool.Description.Returns($"{name} description");
        tool.IsAvailable(Arg.Any<HermesChatContext>()).Returns(isAvailable);
        return tool;
    }

    private async Task<Id<ToolSet>> SeedToolSetAsync(
        string? instructions,
        params (string Name, string? AdditionalDescription)[] tools
    )
    {
        var toolSetId = DatabaseExtensions.CreateId<ToolSet>();
        var toolSet = ToolSet.Create(
            toolSetId,
            "Set",
            null,
            instructions,
            tools.Select(t =>
                ToolSetTool.Create(
                    DatabaseExtensions.CreateId<ToolSetTool>(),
                    toolSetId,
                    ToolSource.Native,
                    t.Name,
                    t.AdditionalDescription
                )
            )
        );

        await using var dbContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        await dbContext.SeedData(toolSet);

        return toolSetId;
    }
}
