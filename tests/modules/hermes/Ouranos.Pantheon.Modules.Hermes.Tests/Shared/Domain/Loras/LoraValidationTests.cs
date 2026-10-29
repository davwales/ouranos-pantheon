using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Loras;

public sealed class LoraValidationTests
{
    private readonly HermesDbContext _dbContext = DbContextExtensions.Mock<HermesDbContext>();

    private static Lora Create(string identifier)
    {
        return Lora.Create(DatabaseExtensions.CreateId<Lora>(), "Name", identifier);
    }

    [Fact]
    public async Task ValidateIdentifierIsFreeAsync_WhenAnotherConfigUsesIt_ShouldThrowConflictException()
    {
        // Arrange
        await _dbContext.SeedData(Create("detail"));

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(() =>
            LoraValidation.ValidateIdentifierIsFreeAsync(_dbContext, " detail ", loraId: null)
        );
    }

    [Fact]
    public async Task ValidateIdentifierIsFreeAsync_WhenOnlyTheSameConfigUsesIt_ShouldNotThrow()
    {
        // Arrange
        var lora = Create("detail");
        await _dbContext.SeedData(lora);

        // Act & Assert
        await Should.NotThrowAsync(() =>
            LoraValidation.ValidateIdentifierIsFreeAsync(_dbContext, "detail", lora.Id)
        );
    }

    [Fact]
    public async Task LoadByIdsAsync_WhenAllIdsExist_ShouldReturnTheirLoras()
    {
        // Arrange
        var detail = Create("detail");
        await _dbContext.SeedData(detail, Create("other"));

        // Act
        var loras = await LoraValidation.LoadByIdsAsync(_dbContext, [detail.Id]);

        // Assert
        loras.ShouldHaveSingleItem().Id.ShouldBe(detail.Id);
    }

    [Fact]
    public async Task LoadByIdsAsync_WhenNoIdsAreGiven_ShouldReturnAnEmptyList()
    {
        // Act
        var loras = await LoraValidation.LoadByIdsAsync(_dbContext, null);

        // Assert
        loras.ShouldBeEmpty();
    }

    [Fact]
    public async Task LoadByIdsAsync_WhenAnIdIsUnknown_ShouldThrowNotFoundException()
    {
        // Arrange
        var detail = Create("detail");
        await _dbContext.SeedData(detail);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() =>
            LoraValidation.LoadByIdsAsync(
                _dbContext,
                [detail.Id, DatabaseExtensions.CreateId<Lora>()]
            )
        );
    }
}
