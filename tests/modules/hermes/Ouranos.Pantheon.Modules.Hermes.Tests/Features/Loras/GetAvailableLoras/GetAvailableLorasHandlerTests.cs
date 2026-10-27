using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAvailableLoras;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAvailableLoras.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableLoras;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Loras.GetAvailableLoras;

public sealed class GetAvailableLorasHandlerTests
{
    private readonly ILogger<GetAvailableLorasHandler> _logger = Substitute.For<
        ILogger<GetAvailableLorasHandler>
    >();
    private readonly HermesDbContext _dbContext = DbContextExtensions.Mock<HermesDbContext>();
    private readonly GetAvailableLorasHandler _handler;

    public GetAvailableLorasHandlerTests()
    {
        _handler = new GetAvailableLorasHandler(_logger, _dbContext);
    }

    private static AvailableLora Row(string identifier, bool isCompatible = true)
    {
        return AvailableLora.Create(
            DatabaseExtensions.CreateId<AvailableLora>(),
            identifier,
            "Title",
            "flux-1",
            0.7f,
            "phrase",
            "hint",
            isCompatible
        );
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GetAvailableLorasHandler(null!, _dbContext));
    }

    [Fact]
    public void Constructor_WhenDbContextIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GetAvailableLorasHandler(_logger, null!));
    }

    [Fact]
    public async Task Handle_WhenLorasExist_ShouldReturnThemSortedByIdentifier()
    {
        // Arrange
        var detail = Row("detail");
        await _dbContext.SeedData(Row("pixel-art", isCompatible: false), detail);

        // Act
        var result = await _handler.Handle(new GetAvailableLorasInput(), CancellationToken.None);

        // Assert
        result.Select(l => l.Identifier).ShouldBe(["detail", "pixel-art"]);
        result[0]
            .ShouldBe(
                new GetAvailableLorasResponse(
                    detail.Id,
                    "detail",
                    "Title",
                    "flux-1",
                    0.7f,
                    "phrase",
                    "hint",
                    IsCompatible: true
                )
            );
    }

    [Fact]
    public async Task Handle_WhenFilteredByCompatibility_ShouldReturnOnlyMatchingLoras()
    {
        // Arrange
        await _dbContext.SeedData(Row("detail"), Row("pixel-art", isCompatible: false));

        // Act
        var result = await _handler.Handle(
            new GetAvailableLorasInput(Filter: ["IsCompatible:eq:true"]),
            CancellationToken.None
        );

        // Assert
        result.ShouldHaveSingleItem().Identifier.ShouldBe("detail");
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            _handler.Handle(new GetAvailableLorasInput(), new CancellationToken(true))
        );
    }
}
