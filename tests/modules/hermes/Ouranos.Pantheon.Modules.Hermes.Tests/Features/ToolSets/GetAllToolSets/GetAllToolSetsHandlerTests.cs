using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.ToolSets.GetAllToolSets;

public sealed class GetAllToolSetsHandlerTests
{
    private readonly GetAllToolSetsHandler _handler;
    private readonly ILogger<GetAllToolSetsHandler> _logger = Substitute.For<
        ILogger<GetAllToolSetsHandler>
    >();
    private readonly HermesDbContext _dbContext;
    private readonly IFlagsmithClient _flagsmith = Substitute.For<IFlagsmithClient>();
    private readonly IHermesToolSource _toolSource = Substitute.For<IHermesToolSource>();

    public GetAllToolSetsHandlerTests()
    {
        _dbContext = DbContextExtensions.Mock<HermesDbContext>();
        _handler = new GetAllToolSetsHandler(_logger, _dbContext, _flagsmith, [_toolSource]);

        _toolSource
            .ListTools()
            .Returns([new AvailableTool(ToolSource.Native, "echo", "Echoes text.")]);
        SetPublicMode(false);
    }

    [Fact]
    public async Task Handle_WhenToolSetsExist_ShouldReturnAllSortedByName()
    {
        // Arrange
        await SeedToolSet("Writing", isPublic: false);
        await SeedToolSet("Research");

        // Act
        var result = await _handler.Handle(new GetAllToolSetsInput(), CancellationToken.None);

        // Assert
        result.Select(s => s.Name).ShouldBe(["Research", "Writing"]);
    }

    [Fact]
    public async Task Handle_WhenNoToolSets_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllToolSetsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenToolSetExists_ShouldMapPropertiesAndToolAvailability()
    {
        // Arrange
        var toolSet = await SeedToolSet(
            "Research",
            "Desc",
            isPublic: false,
            ("search", null),
            ("echo", "Be brief.")
        );

        // Act
        var result = await _handler.Handle(new GetAllToolSetsInput(), CancellationToken.None);

        // Assert
        var item = result.ShouldHaveSingleItem();
        item.Id.ShouldBe(toolSet.Id);
        item.Name.ShouldBe("Research");
        item.Description.ShouldBe("Desc");
        item.IsPublic.ShouldBeFalse();
        item.Tools.ShouldBe([
            new GetAllToolSetsToolResponse(ToolSource.Native, "echo", "Be brief.", true),
            new GetAllToolSetsToolResponse(ToolSource.Native, "search", null, false),
        ]);
    }

    [Fact]
    public async Task Handle_WhenPublicModeEnabled_ShouldOnlyReturnPublicToolSets()
    {
        // Arrange
        var publicToolSet = await SeedToolSet("Research");
        await SeedToolSet("Writing", isPublic: false);
        SetPublicMode(true);

        // Act
        var result = await _handler.Handle(new GetAllToolSetsInput(), CancellationToken.None);

        // Assert
        result.ShouldHaveSingleItem().Id.ShouldBe(publicToolSet.Id);
    }

    [Fact]
    public async Task Handle_WhenFilteredByIsPublic_ShouldReturnMatchingToolSets()
    {
        // Arrange
        await SeedToolSet("Research");
        var privateToolSet = await SeedToolSet("Writing", isPublic: false);
        var query = new GetAllToolSetsInput(Filter: ["IsPublic:eq:false"]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldHaveSingleItem().Id.ShouldBe(privateToolSet.Id);
    }

    [Fact]
    public async Task Handle_WhenSortedByNameDescending_ShouldReturnReverseOrder()
    {
        // Arrange
        await SeedToolSet("Research");
        await SeedToolSet("Writing");
        var query = new GetAllToolSetsInput(SortField: "Name", SortDirection: "desc");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Select(s => s.Name).ShouldBe(["Writing", "Research"]);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllToolSetsInput();
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
        string name,
        string? description = null,
        bool isPublic = true,
        params (string Name, string? AdditionalDescription)[] tools
    )
    {
        var id = DatabaseExtensions.CreateId<ToolSet>();

        var toolSet = ToolSet.Create(
            id,
            name,
            description,
            null,
            tools.Select(t =>
                ToolSetTool.Create(
                    DatabaseExtensions.CreateId<ToolSetTool>(),
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
