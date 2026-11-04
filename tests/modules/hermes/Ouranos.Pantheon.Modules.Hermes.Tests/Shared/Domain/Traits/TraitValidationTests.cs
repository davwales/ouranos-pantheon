using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Traits;

public sealed class TraitValidationTests
{
    private readonly HermesDbContext _dbContext = DbContextExtensions.Mock<HermesDbContext>();

    [Fact]
    public async Task LoadByIdsAsync_WhenTraitIdsNull_ShouldReturnEmpty()
    {
        // Act
        var traits = await TraitValidation.LoadByIdsAsync(_dbContext, null);

        // Assert
        traits.ShouldBeEmpty();
    }

    [Fact]
    public async Task LoadByIdsAsync_WhenAllTraitsExist_ShouldReturnThem()
    {
        // Arrange
        var trait = Trait.Create(DatabaseExtensions.CreateId<Trait>(), "Witty", "Be witty.");
        await _dbContext.SeedData(trait);

        // Act
        var traits = await TraitValidation.LoadByIdsAsync(_dbContext, [trait.Id]);

        // Assert
        traits.ShouldHaveSingleItem().Id.ShouldBe(trait.Id);
    }

    [Fact]
    public async Task LoadByIdsAsync_WhenATraitIdIsUnknown_ShouldThrowNotFoundException()
    {
        // Arrange
        var trait = Trait.Create(DatabaseExtensions.CreateId<Trait>(), "Witty", "Be witty.");
        await _dbContext.SeedData(trait);

        // Act
        var load = async () =>
            await TraitValidation.LoadByIdsAsync(
                _dbContext,
                [trait.Id, DatabaseExtensions.CreateId<Trait>()]
            );

        // Assert
        var exception = await load.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldContain("trait");
    }

    [Fact]
    public async Task LoadByIdsAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Act
        var load = async () =>
            await TraitValidation.LoadByIdsAsync(
                _dbContext,
                [DatabaseExtensions.CreateId<Trait>()],
                new CancellationToken(true)
            );

        // Assert
        await load.ShouldThrowAsync<OperationCanceledException>();
    }
}
