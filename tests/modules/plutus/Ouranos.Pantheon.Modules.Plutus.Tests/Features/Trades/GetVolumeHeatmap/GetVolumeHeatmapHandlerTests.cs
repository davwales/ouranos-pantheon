using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetVolumeHeatmap;

public sealed class GetVolumeHeatmapHandlerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new IdCustomization());
    private readonly GetVolumeHeatmapHandler _handler;
    private readonly ILogger<GetVolumeHeatmapHandler> _logger = Substitute.For<
        ILogger<GetVolumeHeatmapHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public GetVolumeHeatmapHandlerTests()
    {
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetVolumeHeatmapHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetVolumeHeatmapInput(new Id<Market>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var act = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void ComputeCells_WhenRowsExist_ShouldComputePercentagesGlobally()
    {
        // Arrange
        var rows = new List<HeatmapRow>
        {
            new(DayOfWeek: 0, Hour: 0, TotalTrades: 60),
            new(DayOfWeek: 0, Hour: 12, TotalTrades: 40),
            new(DayOfWeek: 3, Hour: 6, TotalTrades: 25),
        };

        // Act
        var cells = GetVolumeHeatmapHandler.ComputeCells(rows);

        // Assert
        cells.Count.ShouldBe(3);

        cells[0].DayOfWeek.ShouldBe(0);
        cells[0].Hour.ShouldBe(0);
        cells[0].TotalTrades.ShouldBe(60);
        cells[0].Percentage.ShouldBe(48m);

        cells[1].DayOfWeek.ShouldBe(0);
        cells[1].Hour.ShouldBe(12);
        cells[1].TotalTrades.ShouldBe(40);
        cells[1].Percentage.ShouldBe(32m);

        cells[2].DayOfWeek.ShouldBe(3);
        cells[2].Hour.ShouldBe(6);
        cells[2].TotalTrades.ShouldBe(25);
        cells[2].Percentage.ShouldBe(20m);
    }

    [Fact]
    public void ComputeCells_WhenRowsExist_ShouldComputePercentagesAcrossAllDays()
    {
        // Arrange
        var rows = new List<HeatmapRow>
        {
            new(DayOfWeek: 1, Hour: 0, TotalTrades: 10),
            new(DayOfWeek: 1, Hour: 8, TotalTrades: 30),
            new(DayOfWeek: 5, Hour: 16, TotalTrades: 50),
            new(DayOfWeek: 5, Hour: 20, TotalTrades: 50),
        };

        // Act
        var cells = GetVolumeHeatmapHandler.ComputeCells(rows);

        // Assert
        cells.Count.ShouldBe(4);

        var day1Hour0 = cells.First(c => c.DayOfWeek == 1 && c.Hour == 0);
        day1Hour0.Percentage.ShouldBe(7.14m);

        var day1Hour8 = cells.First(c => c.DayOfWeek == 1 && c.Hour == 8);
        day1Hour8.Percentage.ShouldBe(21.43m);

        var day5Hour16 = cells.First(c => c.DayOfWeek == 5 && c.Hour == 16);
        day5Hour16.Percentage.ShouldBe(35.71m);

        var day5Hour20 = cells.First(c => c.DayOfWeek == 5 && c.Hour == 20);
        day5Hour20.Percentage.ShouldBe(35.71m);
    }

    [Fact]
    public void ComputeCells_WhenNoTrades_ShouldReturnEmptyList()
    {
        // Arrange
        var rows = new List<HeatmapRow>();

        // Act
        var cells = GetVolumeHeatmapHandler.ComputeCells(rows);

        // Assert
        cells.ShouldBeEmpty();
    }

    [Fact]
    public void ComputeCells_WhenSingleRow_ShouldHavePercentage100()
    {
        // Arrange
        var rows = new List<HeatmapRow> { new(DayOfWeek: 4, Hour: 10, TotalTrades: 7) };

        // Act
        var cells = GetVolumeHeatmapHandler.ComputeCells(rows);

        // Assert
        cells.Count.ShouldBe(1);
        cells[0].DayOfWeek.ShouldBe(4);
        cells[0].Hour.ShouldBe(10);
        cells[0].TotalTrades.ShouldBe(7);
        cells[0].Percentage.ShouldBe(100m);
    }
}
